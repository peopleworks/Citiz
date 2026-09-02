#!/usr/bin/env python3
"""Generates the synthetic "Citiz voice" packs with ElevenLabs, once, from the verified content.

    python tools/audio/generate_elevenlabs.py --list-voices
    python tools/audio/generate_elevenlabs.py --sample --voice <voice_id> [--voice <voice_id> ...]
    python tools/audio/generate_elevenlabs.py --set 2025 --voice <voice_id> --base-url https://your.host/citiz-audio/
    python tools/audio/generate_elevenlabs.py --set words --voice <voice_id> --base-url https://your.host/citiz-audio/

The API key is read from the ELEVENLABS_API_KEY environment variable, or from tools/audio/.env
(a line `ELEVENLABS_API_KEY=...`; the file is git-ignored). The key never ships with the app: the
app only ever downloads the finished MP3 files from the pack host.

Sets:  2025   -> pack citiz-voice-2025: every prompt and every accepted answer of the 2025 test
       2008   -> pack citiz-voice-2008: every prompt of the 2008 test (answers are in the official recordings)
       words  -> pack citiz-voice-words: the reading and writing vocabulary
Idempotent: clips that already exist under tools/audio/dist/<pack>/v<version>/ are not generated
again, so an interrupted run resumes and a re-run costs nothing. --dry-run prints the character
count (ElevenLabs bills one credit per character) without calling the API.
"""
from __future__ import annotations

import argparse
import datetime as dt
import os
import sys
import time
from pathlib import Path

import requests

from packs_common import DIST, describe, load_content, save_pack, slug

API = "https://api.elevenlabs.io/v1"
MODEL = "eleven_multilingual_v2"
OUTPUT_FORMAT = "mp3_44100_64"
# Stability high and style off: a calm, consistent reading voice; speed 0.9 so a learner can follow.
VOICE_SETTINGS = {"stability": 0.6, "similarity_boost": 0.8, "style": 0.0, "use_speaker_boost": True, "speed": 0.9}
ELEVENLABS_LICENSE = "Generated with ElevenLabs under the paid-plan commercial license; text is USCIS public domain"


def api_key() -> str:
    key = os.environ.get("ELEVENLABS_API_KEY")
    env_file = Path(__file__).resolve().parent / ".env"
    if not key and env_file.exists():
        for line in env_file.read_text(encoding="utf-8").splitlines():
            if line.startswith("ELEVENLABS_API_KEY="):
                key = line.split("=", 1)[1].strip().strip('"').strip("'")
    if not key:
        sys.exit("No ElevenLabs key. Set ELEVENLABS_API_KEY in your environment or in tools/audio/.env (see README.md).")
    return key


def clips_for(set_name: str) -> list[dict]:
    """The clips a set contains: id, role, text, and the reference fields the manifest carries."""
    clips: list[dict] = []
    if set_name in ("2025", "2008"):
        bank = load_content(f"exams/{set_name}/questions.json")["questions"]
        for q in bank:
            number = f"{q['number']:03}"
            clips.append({"id": f"q-{set_name}-{number}", "role": "prompt", "file": f"q{number}.mp3", "text": q["prompt"], "questionId": q["id"]})
            if set_name == "2025" and not q.get("dynamicAnswerKey"):
                for index, answer in enumerate(q["acceptedAnswers"]):
                    clips.append({"id": f"a-{set_name}-{number}-{index}", "role": "answer", "file": f"a{number}-{index}.mp3", "text": answer, "questionId": q["id"], "answerIndex": index})
    elif set_name == "words":
        words: dict[str, str] = {}
        for kind in ("reading", "writing"):
            for group in load_content(f"english/{kind}-vocabulary.json")["groups"]:
                for word in group["words"]:
                    words.setdefault(word.lower(), word)
        for word in sorted(words.values(), key=str.lower):
            clips.append({"id": f"w-{slug(word)}", "role": "word", "file": f"w-{slug(word)}.mp3", "text": word, "word": word})
    else:
        sys.exit(f"unknown set '{set_name}'")
    return clips


def spoken(text: str) -> str:
    """What the voice reads: USCIS's parentheses mark optional words, which are read as part of the answer."""
    return text.replace("(", "").replace(")", "").replace("  ", " ").strip()


def synthesize(key: str, voice: str, text: str, target: Path) -> None:
    for attempt in range(4):
        response = requests.post(
            f"{API}/text-to-speech/{voice}?output_format={OUTPUT_FORMAT}",
            headers={"xi-api-key": key, "Content-Type": "application/json"},
            json={"text": spoken(text), "model_id": MODEL, "voice_settings": VOICE_SETTINGS},
            timeout=120,
        )
        if response.status_code == 429 and attempt < 3:
            time.sleep(5 * (attempt + 1))
            continue
        response.raise_for_status()
        target.write_bytes(response.content)
        return


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--list-voices", action="store_true", help="print the voices available to your account")
    parser.add_argument("--sample", action="store_true", help="synthesize one question with each --voice into tools/audio/dist/samples/")
    parser.add_argument("--set", choices=["2025", "2008", "words"], help="which pack to generate")
    parser.add_argument("--voice", action="append", default=[], help="ElevenLabs voice id (repeatable with --sample)")
    parser.add_argument("--base-url", help="where the pack folders will be served from, ending with /; the pack id and version are appended")
    parser.add_argument("--version", type=int, default=1)
    parser.add_argument("--dry-run", action="store_true", help="count characters and clips; call nothing")
    args = parser.parse_args()

    if args.list_voices:
        voices = requests.get(f"{API}/voices", headers={"xi-api-key": api_key()}, timeout=60).json()["voices"]
        for v in sorted(voices, key=lambda v: v["name"]):
            labels = ", ".join(f"{k}={val}" for k, val in (v.get("labels") or {}).items())
            print(f"{v['voice_id']}  {v['name']:<22} {v.get('category', ''):<12} {labels}")
        return 0

    if args.sample:
        if not args.voice:
            parser.error("--sample needs at least one --voice")
        key = api_key()
        folder = DIST / "samples"
        folder.mkdir(parents=True, exist_ok=True)
        text = "Question 36. What are two Cabinet-level positions? Attorney General. Secretary of Agriculture. Secretary of Commerce."
        for voice in args.voice:
            target = folder / f"{voice}.mp3"
            synthesize(key, voice, text, target)
            print(f"sample -> {target}")
        return 0

    if not args.set:
        parser.error("--set is required (or use --list-voices / --sample)")
    clips = clips_for(args.set)
    characters = sum(len(spoken(c["text"])) for c in clips)
    print(f"set {args.set}: {len(clips)} clips, {characters:,} characters")
    if args.dry_run:
        return 0
    if len(args.voice) != 1 or not args.base_url:
        parser.error("--set needs exactly one --voice and a --base-url")
    if not args.base_url.startswith("https://") or not args.base_url.endswith("/"):
        parser.error("--base-url must start with https:// and end with /")

    key = api_key()
    voice_id = args.voice[0]
    voice_name = next((v["name"] for v in requests.get(f"{API}/voices", headers={"xi-api-key": key}, timeout=60).json()["voices"] if v["voice_id"] == voice_id), voice_id)
    pack_id = f"citiz-voice-{args.set}"
    folder = DIST / pack_id / f"v{args.version}"
    folder.mkdir(parents=True, exist_ok=True)

    for index, clip in enumerate(clips, 1):
        target = folder / clip["file"]
        if not target.exists():
            synthesize(key, voice_id, clip["text"], target)
            print(f"[{index}/{len(clips)}] {clip['id']}")
        clip.update(describe(target))

    manifest_clips = [{k: v for k, v in c.items() if k != "text"} for c in clips]
    titles = {
        "2025": ("Citiz voice · 2025 test", "The 128 questions of the 2025 test and their accepted answers, generated once from the official text"),
        "2008": ("Citiz voice · 2008 test", "The 100 questions of the 2008 test, generated once from the official text (the official recordings carry the answers)"),
        "words": ("Citiz voice · vocabulary words", "The reading and writing words of the English test, generated once from the official lists"),
    }
    title, description = titles[args.set]
    sources = [{"authority": "USCIS", "title": "Text: the verified content in this repository (content/exams, content/english)", "url": "https://github.com/peopleworks/Citiz/tree/main/content", "verifiedOn": dt.date.today().isoformat(), "license": "Public domain (U.S. Government work, 17 U.S.C. § 105)"}]
    pack = {
        "id": pack_id,
        "kind": "synthetic",
        "title": title,
        "description": description,
        "versionId": None if args.set == "words" else args.set,
        "version": args.version,
        "baseUrl": f"{args.base_url}{pack_id}/v{args.version}/",
        "sizeBytes": sum(c["bytes"] for c in manifest_clips),
        "license": ELEVENLABS_LICENSE,
        "voice": f"ElevenLabs · {voice_name} ({MODEL}, speed {VOICE_SETTINGS['speed']})",
        "generatedOn": dt.date.today().isoformat(),
        "reviewStatus": "needs-review",
        "sources": sources,
        "clips": manifest_clips,
    }
    save_pack(pack)
    print(f"{pack_id} v{args.version}: {len(clips)} clips, {pack['sizeBytes'] / 1e6:.1f} MB -> {folder}; content/audio/packs.json updated (reviewStatus needs-review until someone listens)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
