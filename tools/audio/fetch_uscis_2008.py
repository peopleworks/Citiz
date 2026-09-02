#!/usr/bin/env python3
"""Builds the official audio pack for the 2008 civics test from USCIS's own MP3 tracks.

    python tools/audio/fetch_uscis_2008.py --base-url https://your.host/citiz-audio/uscis-2008/v1/

USCIS publishes one track per question (question and all answers, read by a person) on the
"100 Civics Questions and Answers with MP3 Audio" page; works of the U.S. Government are public
domain. The tool downloads the 100 tracks into tools/audio/dist/uscis-2008/v1/ (the folder to
upload to the host, manifest included), measures them, and writes the pack into
content/audio/packs.json. Idempotent: existing files are kept.

uscis.gov answers 403 to non-browser user agents, hence the browser-like one below.
"""
from __future__ import annotations

import argparse
import concurrent.futures
import datetime as dt
import re
import sys
from pathlib import Path

import requests

from packs_common import DIST, PUBLIC_DOMAIN, describe, load_content, save_pack

PAGE = "https://www.uscis.gov/citizenship/find-study-materials-and-resources/study-for-the-test/100-civics-questions-and-answers-with-mp3-audio-english-version"
UA = {"User-Agent": "Mozilla/5.0 (Macintosh; Intel Mac OS X 14_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0 Safari/537.36"}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--base-url", required=True, help="where the pack folder will be served from (must end with /)")
    parser.add_argument("--version", type=int, default=1, help="pack version; bump when the files change")
    args = parser.parse_args()
    if not args.base_url.startswith("https://") or not args.base_url.endswith("/"):
        parser.error("--base-url must start with https:// and end with /")

    folder = DIST / "uscis-2008" / f"v{args.version}"
    folder.mkdir(parents=True, exist_ok=True)

    html = requests.get(PAGE, headers=UA, timeout=60).text
    links = sorted(set(re.findall(r'href="(/sites/default/files/document/audio/Track[^"]*)"', html)))
    if len(links) != 100:
        print(f"expected 100 track links on the USCIS page, found {len(links)}", file=sys.stderr)
        return 1

    def fetch(link: str) -> tuple[int, Path]:
        number = int(re.search(r"Track(?:%20|_| )?(\d+)", link).group(1))
        target = folder / f"q{number:03}.mp3"
        if not target.exists():
            response = requests.get("https://www.uscis.gov" + link, headers=UA, timeout=120)
            response.raise_for_status()
            target.write_bytes(response.content)
        return number, target

    with concurrent.futures.ThreadPoolExecutor(6) as pool:
        tracks = sorted(pool.map(fetch, links))

    bank = {q["number"]: q for q in load_content("exams/2008/questions.json")["questions"]}
    clips = []
    for number, path in tracks:
        clips.append({"id": f"r-2008-{number:03}", "role": "recording", "file": path.name, **describe(path), "questionId": bank[number]["id"]})

    pack = {
        "id": "uscis-2008",
        "kind": "official",
        "title": "Official USCIS recordings · 2008 test",
        "description": "The 100 questions of the 2008 test with their answers, read by USCIS",
        "versionId": "2008",
        "version": args.version,
        "baseUrl": args.base_url,
        "sizeBytes": sum(c["bytes"] for c in clips),
        "license": PUBLIC_DOMAIN,
        "voice": None,
        "generatedOn": None,
        "reviewStatus": "approved",
        "sources": [
            {
                "authority": "USCIS",
                "title": "100 Civics Questions and Answers for the 2008 Test with MP3 Audio (English version)",
                "url": PAGE,
                "verifiedOn": dt.date.today().isoformat(),
                "license": PUBLIC_DOMAIN,
            }
        ],
        "clips": clips,
    }
    save_pack(pack)
    total_mb = pack["sizeBytes"] / 1e6
    minutes = sum(c["seconds"] for c in clips) / 60
    print(f"uscis-2008 v{args.version}: 100 clips, {total_mb:.1f} MB, {minutes:.1f} minutes -> {folder} and content/audio/packs.json")
    return 0


if __name__ == "__main__":
    sys.exit(main())
