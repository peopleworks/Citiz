#!/usr/bin/env python3
"""Compares the Citiz question banks, exam rules and vocabulary lists with the official USCIS documents.

    tools/content-verify/fetch.sh                       # download the official documents
    python tools/content-verify/verify.py               # compare content/ with them

The comparison is exact after a small normalisation (whitespace, typographic quotes), so any wording
difference shows up. Questions whose official answer is "Visit uscis.gov/citizenship/testupdates ..."
or "Answers will vary ..." are modelled with a dynamicAnswerKey in Citiz; they are listed under
"informational" together with editorial notes, and do not count as differences. The exit code is 1
when a real difference remains, so this can run in a maintainer's shell or a scheduled job.

Requires: pdfplumber, beautifulsoup4, lxml (see requirements.txt).
"""
from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

import pdfplumber
from bs4 import BeautifulSoup

QUOTES = {" ": " ", "’": "'", "‘": "'", "“": '"', "”": '"', "–": "-", "—": "-"}
NOTE_PREFIXES = ("Visit uscis.gov/citizenship/testupdates", "Answers will vary")


def norm(text: str | None) -> str:
    if text is None:
        return ""
    for a, b in QUOTES.items():
        text = text.replace(a, b)
    return re.sub(r"\s+", " ", text).strip()


def strip_star(prompt: str) -> tuple[str, bool]:
    prompt = norm(prompt)
    if prompt.endswith("*"):
        return prompt[:-1].rstrip(), True
    return prompt, False


def pdf_text(path: Path) -> str:
    with pdfplumber.open(path) as pdf:
        return "\n".join((page.extract_text() or "") for page in pdf.pages)


def parse_official_list(text: str, footer: re.Pattern[str], skip: tuple[str, ...] = ()) -> dict[int, dict]:
    """Parses the "N. prompt" / "• answer" layout both USCIS PDFs use. Wrapped lines are joined."""
    questions: dict[int, dict] = {}
    category = subcategory = None
    current = None
    started = False
    for raw in text.split("\n"):
        line = raw.strip()
        if not started:
            started = line == "AMERICAN GOVERNMENT"
            if not started:
                continue
        if not line or footer.match(line) or line.startswith(skip):
            continue
        if re.match(r"^[A-Z][A-Z ,&]+$", line):
            category = line
            continue
        if m := re.match(r"^([A-C]): (.+)$", line):
            subcategory = m.group(2).strip()
            continue
        if m := re.match(r"^(\d+)\. (.+)$", line):
            number = int(m.group(1))
            prompt, star = strip_star(m.group(2))
            current = {"number": number, "prompt": prompt, "star": star, "answers": [], "category": category, "subcategory": subcategory}
            questions[number] = current
            continue
        if current is None:
            continue
        if line == "*":
            current["star"] = True
        elif m := re.match(r"^[•▪] ?(.*)$", line):
            current["answers"].append(norm(m.group(1)))
        elif current["answers"]:
            current["answers"][-1] = norm(current["answers"][-1] + " " + line)
        else:
            current["prompt"], star = strip_star(current["prompt"] + " " + line)
            current["star"] = current["star"] or star
    return questions


def parse_2025_pdf(path: Path) -> dict[int, dict]:
    return parse_official_list(pdf_text(path), re.compile(r"^\d+ of \d+ uscis\.gov/citizenship$"))


def parse_2008_pdf(path: Path) -> dict[int, dict]:
    return parse_official_list(pdf_text(path), re.compile(r"^-\d+- www\.uscis\.gov$"), skip=("* If you are 65", "may study just the questions"))


def parse_2008_page(path: Path) -> dict[int, dict]:
    """The uscis.gov page: <p><strong>N. prompt</strong> Question N Audio</p><ul><li>answer</li>...</ul>."""
    soup = BeautifulSoup(path.read_text(encoding="utf-8", errors="replace"), "lxml")
    questions: dict[int, dict] = {}
    category = subcategory = None
    for element in (soup.find("main") or soup.body).find_all(["p", "h2", "h3", "h4", "ul"]):
        text = norm(element.get_text(" "))
        if element.name == "ul":
            if questions and not questions[max(questions)]["answers"]:
                questions[max(questions)]["answers"] = [norm(li.get_text(" ")) for li in element.find_all("li")]
            continue
        if re.match(r"^[A-Z][A-Z ,&]+$", text):
            category = text
        elif m := re.match(r"^([A-C]): (.+)$", text):
            subcategory = m.group(2).strip()
        elif m := re.match(r"^(\d+)\. (.+?)\s*Question\s*\d+ Audio$", text):
            number = int(m.group(1))
            prompt, star = strip_star(m.group(2))
            prompt = prompt.replace(" .", ".").replace(" ?", "?")  # emphasis tags leave a space before punctuation
            questions[number] = {"number": number, "prompt": prompt, "star": star, "answers": [], "category": category, "subcategory": subcategory}
    return questions


def parse_vocabulary_pdf(path: Path) -> dict[str, list[str]]:
    """The one-page vocabulary sheets are columns; words are clustered by their x position."""
    with pdfplumber.open(path) as pdf:
        words = pdf.pages[0].extract_words(keep_blank_chars=True, x_tolerance=1.5, y_tolerance=2)
    columns: list[list[dict]] = []
    for word in sorted(words, key=lambda w: w["x0"]):
        if columns and word["x0"] - columns[-1][-1]["x0"] <= 25:
            columns[-1].append(word)
        else:
            columns.append([word])
    result: dict[str, list[str]] = {}
    for column in columns:
        entries = [w["text"].strip() for w in sorted(column, key=lambda w: w["top"])]
        heading = [e for e in entries if re.fullmatch(r"[A-Z][A-Z ]+|\([A-Z]+\)", e)]
        body = [e for e in entries if e not in heading and not e.startswith("(rev.") and "Vocabulary for" not in e]
        if heading and body:
            result[" ".join(heading).title().replace("(Function)", "(Function)").replace("(Content)", "(Content)")] = body
        elif heading:
            result.setdefault(" ".join(heading).title(), [])
        elif body:
            # the CIVICS heading sits in its own column; the words are in the column before it
            result.setdefault("__unlabelled__", []).extend(body)
    return result


def compare_bank(label: str, official: dict[int, dict], bank_path: Path, versions: dict, version_id: str) -> int:
    bank = json.loads(bank_path.read_text(encoding="utf-8"))
    by_number = {q["number"]: q for q in bank["questions"]}
    print(f"\n== {label}: {len(official)} official, {len(by_number)} in {bank_path.name}")
    differences = 0
    informational: list[str] = []
    for number in sorted(set(official) | set(by_number)):
        o, b = official.get(number), by_number.get(number)
        if o is None or b is None:
            print(f"  DIFFERENCE Q{number}: {'missing from bank' if b is None else 'not in the official document'}")
            differences += 1
            continue
        if norm(b["prompt"]) != o["prompt"]:
            print(f"  DIFFERENCE Q{number} prompt\n    official: {o['prompt']}\n    bank    : {norm(b['prompt'])}")
            differences += 1
        for field in ("category", "subcategory"):
            if o[field] and norm(b[field]).lower() != o[field].lower():
                print(f"  DIFFERENCE Q{number} {field}: official '{o[field]}', bank '{b[field]}'")
                differences += 1
        answers = [norm(a) for a in b["acceptedAnswers"]]
        if b.get("dynamicAnswerKey"):
            if any(o["answers"][0].startswith(p) for p in NOTE_PREFIXES) if o["answers"] else False:
                informational.append(f"Q{number}: dynamicAnswerKey={b['dynamicAnswerKey']} (official: {o['answers'][0][:60]}...)")
            else:
                print(f"  DIFFERENCE Q{number}: bank is dynamic but the official answers are {o['answers']}")
                differences += 1
        elif answers != o["answers"]:
            # An official answer may carry a bracketed remark ("[Also acceptable are ...]") or a trailing
            # sentence; the bank keeps those in `note` and lists the remark's answers separately.
            stripped = [re.sub(r"\s*\[.*\]$", "", a) for a in o["answers"]]
            stripped = [re.sub(r"\s+For a complete list of tribes.*$", "", a) for a in stripped]
            if answers[: len(stripped)] == stripped and b.get("note"):
                informational.append(f"Q{number}: official remark kept in note: {norm(b['note'])[:90]}")
            else:
                print(f"  DIFFERENCE Q{number} answers\n    official: {o['answers']}\n    bank    : {answers}")
                differences += 1
        elif b.get("note"):
            informational.append(f"Q{number}: note: {norm(b['note'])[:90]}")
    stars = [n for n, q in sorted(official.items()) if q["star"]]
    version = next(v for v in versions["versions"] if v["id"] == version_id)
    if stars != version["seniorQuestionNumbers"]:
        print(f"  DIFFERENCE 65/20 list\n    official: {stars}\n    versions.json: {version['seniorQuestionNumbers']}")
        differences += 1
    else:
        print(f"  65/20 list matches ({len(stars)} questions)")
    for line in informational:
        print(f"  info {line}")
    print(f"  {differences} difference(s)")
    return differences


def compare_vocabulary(label: str, official: dict[str, list[str]], path: Path) -> int:
    data = json.loads(path.read_text(encoding="utf-8"))
    official_words = sorted(norm(w) for words in official.values() for w in words)
    bank_words = sorted(norm(w) for g in data["groups"] for w in g["words"])
    print(f"\n== {label}: {len(official_words)} official words, {len(bank_words)} in {path.name}")
    missing = sorted(set(official_words) - set(bank_words))
    extra = sorted(set(bank_words) - set(official_words))
    for w in missing:
        print(f"  DIFFERENCE missing from bank: {w}")
    for w in extra:
        print(f"  DIFFERENCE not in the official list: {w}")
    print(f"  {len(missing) + len(extra)} difference(s)")
    return len(missing) + len(extra)


def main() -> int:
    root = Path(__file__).resolve().parents[2]
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--official", type=Path, default=Path(__file__).resolve().parent / "official", help="folder written by fetch.sh")
    parser.add_argument("--content", type=Path, default=root / "content", help="the content folder to check")
    args = parser.parse_args()

    versions = json.loads((args.content / "exams" / "versions.json").read_text(encoding="utf-8"))
    total = 0
    total += compare_bank("2025 Civics Test (M-1778 PDF)", parse_2025_pdf(args.official / "128q-2025.pdf"), args.content / "exams" / "2025" / "questions.json", versions, "2025")
    total += compare_bank("2008 Civics Test (uscis.gov page)", parse_2008_page(args.official / "2008-page.html"), args.content / "exams" / "2008" / "questions.json", versions, "2008")
    pdf_2008 = parse_2008_pdf(args.official / "100q.pdf")
    page_2008 = parse_2008_page(args.official / "2008-page.html")
    print(f"\n== 2008 cross-check: uscis.gov page vs 100q.pdf (rev. 01/19)")
    for number in sorted(page_2008):
        a, b = page_2008[number], pdf_2008.get(number)
        if b and (a["prompt"], a["answers"], a["star"]) != (b["prompt"], b["answers"], b["star"]):
            print(f"  info Q{number} differs between the page and the 2019 PDF (the page is current)")
    total += compare_vocabulary("Reading vocabulary", parse_vocabulary_pdf(args.official / "reading_vocab.pdf"), args.content / "english" / "reading-vocabulary.json")
    total += compare_vocabulary("Writing vocabulary", parse_vocabulary_pdf(args.official / "writing_vocab.pdf"), args.content / "english" / "writing-vocabulary.json")
    print(f"\n{'No differences with the official documents.' if total == 0 else f'{total} difference(s) to review.'}")
    return 1 if total else 0


if __name__ == "__main__":
    sys.exit(main())
