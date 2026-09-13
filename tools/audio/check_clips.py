#!/usr/bin/env python3
"""Checks a generated voice pack before anyone approves it: a local Whisper model transcribes every clip,
the transcript is compared with the text the voice was given, and a page lists the clips worth a listen.

    .venv/bin/pip install -r tools/audio/requirements-check.txt       # once; the model downloads on first use
    .venv/bin/python tools/audio/check_clips.py --set 2025 --set words
    open tools/audio/dist/review/index.html

The recogniser gets no hint of the expected text (ADR-0004, gate 3), so a wrong clip cannot be heard as the
right one. A clip is listed when its transcript still differs from its text after numbers, ordinals,
apostrophes and a few same-sounding words are normalised, or when its length is out of proportion to its
text. The page adds a random sample of the other clips for a general listen. Everything runs on this machine.
"""
from __future__ import annotations

import argparse
import difflib
import html
import json
import random
import re
import statistics
import sys
import time

from mutagen.mp3 import MP3

from generate_elevenlabs import clips_for, spoken
from packs_common import DIST

ORDINALS = {
    "first": "one", "second": "two", "third": "three", "fourth": "four", "fifth": "five", "sixth": "six",
    "seventh": "seven", "eighth": "eight", "ninth": "nine", "tenth": "ten", "eleventh": "eleven",
    "twelfth": "twelve", "thirteenth": "thirteen", "fourteenth": "fourteen", "fifteenth": "fifteen",
    "sixteenth": "sixteen", "seventeenth": "seventeen", "eighteenth": "eighteen", "nineteenth": "nineteen",
    "twentieth": "twenty",
}
# A transcript cannot tell these apart, so they count as the same word; Whisper also writes some words as letters.
SAME_SOUND = {"to": "two", "too": "two", "for": "four", "sue": "soo", "r": "are", "b": "be"}


def normalised(text: str) -> list[str]:
    from num2words import num2words

    text = text.lower().replace("u.s.", "us").replace("d.c.", "dc").replace("'", "").replace("’", "")
    text = re.sub(r"(?<=\d),(?=\d{3})", "", text)
    text = re.sub(r"(\d+)(?:st|nd|rd|th)\b", r"\1", text)
    text = re.sub(r"\d+", lambda m: f" {num2words(int(m.group()))} ", text)
    words = []
    for word in re.sub(r"[^a-z]+", " ", text).split():
        word = re.sub(r"(.)\1{2,}", r"\1", word)
        word = SAME_SOUND.get(ORDINALS.get(word, word), ORDINALS.get(word, word))
        if word != "and":
            words.append(word)
    return words


def page(rows: list[dict], flagged: list[dict], sample: list[dict], model: str) -> str:
    def item(row: dict) -> str:
        note = f' <small>{html.escape(", ".join(row["flags"]))}</small>' if row["flags"] else ""
        return (
            f'<li><h3>{html.escape(row["id"])}{note}</h3>'
            f'<p><b>Text:</b> {html.escape(row["text"])}<br><b>Heard:</b> {html.escape(row.get("heard", "—"))}</p>'
            f'<audio controls preload="none" src="{html.escape(row["src"])}"></audio></li>'
        )

    return (
        '<!doctype html><meta charset="utf-8"><title>Citiz voice · clips to hear</title>'
        "<style>body{font:16px system-ui;margin:2rem auto;max-width:46rem;padding:0 1rem}ul{padding:0}"
        "li{list-style:none;margin:0 0 1.5rem}h3{font-size:1rem;margin:0}small{color:#a33;font-weight:400}"
        "p{margin:.3rem 0 .5rem}audio{width:100%}</style>"
        f"<h1>Citiz voice · clips to hear</h1><p>{len(rows)} clips transcribed on this machine with Whisper "
        f"{html.escape(model)}, given no hints.</p>"
        f"<h2>Worth a listen ({len(flagged)})</h2><ul>{''.join(map(item, flagged)) or '<li>None.</li>'}</ul>"
        f"<h2>A random sample ({len(sample)})</h2><ul>{''.join(map(item, sample))}</ul>"
    )


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--set", action="append", choices=["2025", "2008", "words"], required=True, help="pack to check (repeatable)")
    parser.add_argument("--version", type=int, default=1)
    parser.add_argument("--model", default="small.en", help="faster-whisper model (default: small.en)")
    parser.add_argument("--threshold", type=float, default=0.85, help="lowest similarity between the word sequences that passes")
    parser.add_argument("--sample", type=int, default=12, help="how many other clips to pick at random for a general listen")
    args = parser.parse_args()

    from faster_whisper import WhisperModel

    started = time.time()
    model = WhisperModel(args.model, device="cpu", compute_type="int8")
    rows: list[dict] = []
    for set_name in args.set:
        pack = f"citiz-voice-{set_name}"
        folder = DIST / pack / f"v{args.version}"
        for clip in clips_for(set_name):
            path = folder / clip["file"]
            row = {"pack": pack, "id": clip["id"], "src": f"../{pack}/v{args.version}/{clip['file']}",
                   "text": clip["text"], "said": spoken(clip["text"]), "flags": []}
            rows.append(row)
            if not path.exists():
                row["flags"].append("missing")
                continue
            segments, _ = model.transcribe(str(path), language="en", beam_size=5, condition_on_previous_text=False)
            row["heard"] = " ".join(segment.text.strip() for segment in segments).strip()
            row["similarity"] = round(difflib.SequenceMatcher(None, normalised(row["said"]), normalised(row["heard"])).ratio(), 3)
            row["seconds"] = round(MP3(path).info.length, 2)
            if row["similarity"] < args.threshold:
                row["flags"].append("differs")

    long_enough = [r for r in rows if "seconds" in r and len(r["said"]) >= 12]
    typical = statistics.median(r["seconds"] / len(r["said"]) for r in long_enough) if long_enough else None
    for row in rows:
        if "seconds" not in row:
            continue
        if len(row["said"]) >= 12 and typical:
            if not 0.4 * typical <= row["seconds"] / len(row["said"]) <= 2.5 * typical:
                row["flags"].append("length")
        elif row["seconds"] > 4.0:
            row["flags"].append("length")

    flagged = [r for r in rows if r["flags"]]
    sample = random.Random(0).sample([r for r in rows if not r["flags"]], min(args.sample, len(rows) - len(flagged)))
    review = DIST / "review"
    review.mkdir(parents=True, exist_ok=True)
    (review / "report.json").write_text(json.dumps(rows, indent=1, ensure_ascii=False), encoding="utf-8")
    (review / "index.html").write_text(page(rows, flagged, sample, args.model), encoding="utf-8")

    print(f"{len(rows)} clips checked in {time.time() - started:.0f}s: {len(flagged)} worth a listen")
    for r in flagged:
        print(f"  {r['id']:<18} {','.join(r['flags']):<14} {r.get('similarity', '')!s:<6} said={r['said'][:48]!r} heard={r.get('heard', '')[:48]!r}")
    print(f"listen: open {review / 'index.html'}")
    return 1 if any("missing" in r["flags"] for r in rows) else 0


if __name__ == "__main__":
    sys.exit(main())
