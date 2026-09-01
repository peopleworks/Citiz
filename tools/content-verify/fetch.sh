#!/usr/bin/env bash
# Downloads the official documents the question banks, rules and vocabulary are verified against.
# Usage: tools/content-verify/fetch.sh [output-dir]   (default: tools/content-verify/official)
#
# uscis.gov answers 403 to non-browser user agents, so a browser-like one is sent. Nothing here is
# scraped for publication: the files are compared locally and never committed (see .gitignore).
set -euo pipefail

out="${1:-$(dirname "$0")/official}"
mkdir -p "$out"
ua="Mozilla/5.0 (Macintosh; Intel Mac OS X 14_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0 Safari/537.36"

fetch() {
  local name="$1" url="$2" code
  code=$(curl -sSL -A "$ua" --max-time 120 -o "$out/$name" -w "%{http_code}" "$url")
  printf '%s  %-28s %s\n' "$code" "$name" "$url"
}

fetch 128q-2025.pdf        "https://www.uscis.gov/sites/default/files/document/questions-and-answers/2025-Civics-Test-128-Questions-and-Answers.pdf"
fetch 2008-page.html       "https://www.uscis.gov/citizenship/find-study-materials-and-resources/study-for-the-test/100-civics-questions-and-answers-with-mp3-audio-english-version"
fetch 100q.pdf             "https://www.uscis.gov/sites/default/files/document/questions-and-answers/100q.pdf"
fetch reading_vocab.pdf    "https://www.uscis.gov/sites/default/files/document/guides/reading_vocab.pdf"
fetch writing_vocab.pdf    "https://www.uscis.gov/sites/default/files/document/guides/writing_vocab.pdf"
fetch test-updates.html    "https://www.uscis.gov/citizenship/find-study-materials-and-resources/check-for-test-updates"
fetch interview-and-test.html "https://www.uscis.gov/citizenship/learn-about-citizenship/the-naturalization-interview-and-test"
fetch 2025-civics-test.html "https://www.uscis.gov/citizenship-resource-center/naturalization-test-and-study-resources/2025-civics-test"

echo "Downloaded to $out"
