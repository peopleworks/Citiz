# Content verification tool

Compares what ships in `content/` with the official USCIS documents, so a maintainer can re-verify
the banks in minutes instead of reading 228 questions by eye — and so the comparison itself is
reproducible by anyone who doubts it.

```bash
python3 -m venv .venv && .venv/bin/pip install -r tools/content-verify/requirements.txt
tools/content-verify/fetch.sh            # downloads the PDFs and pages into tools/content-verify/official/
.venv/bin/python tools/content-verify/verify.py
```

What it checks:

| Content | Official document | Compared |
| --- | --- | --- |
| `exams/2025/questions.json`, 2025 rules | *128 Civics Questions and Answers (2025 version)*, M-1778 PDF | every prompt, every accepted answer in order, section headings, the asterisked 65/20 list |
| `exams/2008/questions.json`, 2008 rules | the uscis.gov *100 Civics Questions and Answers* page (current: it has Juneteenth; the 2019 `100q.pdf` does not), cross-checked with the PDF | same |
| `english/*-vocabulary.json` | the reading and writing vocabulary PDFs | every word |

The comparison is exact after normalising whitespace and typographic quotes. Two kinds of entries
are reported as *info* rather than differences, because Citiz models them deliberately:

- questions whose official answer is "Visit uscis.gov/citizenship/testupdates …" or "Answers will
  vary …" carry a `dynamicAnswerKey`; the current officeholder lives in `exams/dynamic-answers.json`
  and is verified against the *Check for Test Updates* page and the office's own site;
- bracketed remarks USCIS attaches to an answer (`[Also acceptable are New Jersey, …]`) are kept in
  `note`, and the answers they name are listed as accepted answers of their own.

Dynamic answers and the discovery capsules are verified by hand against the pages cited on each
entry; the tool does not cover them. The downloaded documents are not committed (`official/` is
ignored): they are public-domain, but the repository should carry the comparison, not copies.

The verification log — what was compared, when, and the decisions taken — is in
[`content/exams/VERIFICATION.md`](../../content/exams/VERIFICATION.md).
