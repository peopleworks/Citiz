# Verifying the exam content

Every official fact in this folder — the two question banks, the exam rules, the dynamic answers and
the vocabulary lists — carries a `reviewStatus` and a `verifiedOn` date. This file is the log of what
was compared with which official document, the decisions taken where a document leaves room for
one, and the procedure for the next round. Re-verification is due whenever the content worker reports
that a monitored source changed (`content/sources/sources.json`), after every election or
appointment, and at least once a year.

## Verification log

### 2026-09-01 — full verification, everything approved

Done by the maintainer with the comparison tool in [`tools/content-verify/`](../../tools/content-verify/README.md):
the official documents were downloaded that day, parsed, and compared entry by entry with the JSON;
every reported difference was then read against the document by hand before a decision.

| Content | Compared with | Result |
| --- | --- | --- |
| `2025/questions.json` (128) | *128 Civics Questions and Answers (2025 version)*, form M-1778 (09/25), the PDF linked from the USCIS 2025 Civics Test page | 13 questions corrected (see below); 65/20 list recorded; approved |
| `2008/questions.json` (100) | the uscis.gov *100 Civics Questions and Answers for the 2008 Test* page (last updated 01/26/2024), cross-checked with `100q.pdf` (rev. 01/19) | wording matched; Q39 made dynamic; Q95 remark split out; approved |
| `versions.json` | *The Naturalization Interview and Test* (updated 10/31/2025), the *2025 Civics Test* page (09/17/2025), M-1778, *Check for Test Updates* (09/18/2025) | filing boundary Oct 20, 2025; 10/6/5 and 20/12/9; 65/20 10/6 for both; approved |
| `dynamic-answers.json` | *Check for Test Updates* (the forms USCIS accepts), whitehouse.gov/administration, speaker.gov, supremecourt.gov/about/biographies.aspx, all read that day | holders confirmed; accepted forms aligned with USCIS; approved |
| `../english/reading-vocabulary.json` (64 words) | *Reading Vocabulary for the Naturalization Test* (rev. 08/08) | identical; approved |
| `../english/writing-vocabulary.json` (75 words) | *Writing Vocabulary for the Naturalization Test* (rev. 08/08) | identical; approved |

**What was wrong in the 2025 bank.** It had been transcribed from the 2020 test wording with the
2025 changes applied from memory. The official 2025 document differs in:

| Q | Was | Official 2025 text |
| --- | --- | --- |
| 3 | "Defines the parts of **the** government" | "Defines the parts of government" |
| 13 | "Government must **follow** the law." | "Government must obey the law." |
| 31 | one answer | adds "People of their state" |
| 33 | two answers | adds "People from their (congressional) district", "People in their district" |
| 41 | five answers | adds "Appoints federal judges" |
| 48 | the 2020 Cabinet list | "Secretary of War (Defense)", "Vice-President", and six more Cabinet-level positions (EPA, SBA, CIA, OMB, DNI, USTR), in the official order |
| 60 | "…to the states or the people." | "…to the states or **to** the people." |
| 68 | "Naturalize", "Derive citizenship", "Be born in the United States" | "Be born in the United States, under the conditions set by the 14th Amendment", "Naturalize", "Derive citizenship (under conditions set by Congress)" |
| 93 | "Lincoln assassinated" | "Lincoln was assassinated." |
| 97 | "What amendment gives citizenship to all persons born in the United States?" | "What amendment says all persons born or naturalized in the United States, and subject to the jurisdiction thereof, are U.S. citizens?" |
| 115 | "…in a field near Shanksville, Pennsylvania" | "…in a field in Pennsylvania" |
| 117 | — | the official closing line "For a complete list of tribes, please visit bia.gov." kept as the note |
| 118 | "Automobile (cars, combustible engine)" | "Automobile (cars, internal combustion engine)" |

The 65/20 list for 2025 (the asterisked questions in M-1778) is
`[2, 7, 12, 20, 30, 36, 38, 39, 44, 52, 61, 66, 74, 78, 86, 94, 113, 115, 121, 126]`; recording it
enabled the 65/20 practice mode for 2025. The 2008 list was confirmed unchanged.

**Decisions taken** (the document leaves these to the transcriber; they are recorded here so the
next verifier does not re-decide them silently):

1. **Q39 (2008), number of justices.** The official answer is now "Visit
   uscis.gov/citizenship/testupdates for the number of justices on the Supreme Court." It is modelled
   like the officeholders: `dynamicAnswerKey: supreme-court-justices`, with `nine (9)` recorded in
   `dynamic-answers.json` from the *Check for Test Updates* page. The 2025 document states the number
   directly (Q53, "Nine (9)"), so that one stays in the bank.
2. **Q95 (2008) and Q120 (2025), the Statue of Liberty.** USCIS prints "Liberty Island [Also
   acceptable are New Jersey, near New York City, and on the Hudson (River).]". The remark is kept in
   `note` verbatim, and the three answers it names are listed as accepted answers after the two main
   ones, so the matcher accepts what the officer accepts. The current 2008 page has dropped the
   remark; the 2019 PDF and the 2025 document both carry it.
3. **Bracketed remarks** ("Answers will vary. [District of Columbia residents…]") are reproduced in
   `note` with USCIS's square brackets, in both banks.
4. **Typography.** Curly quotes and apostrophes in the PDFs are written as straight ASCII quotes.
   The answer matcher ignores the difference, and so does the comparison tool.
5. **Dynamic answers list exactly the forms USCIS accepts** on *Check for Test Updates* (for example
   "JD Vance", "Vance"; "Republican (Party)"), not additional variants. The matcher already accepts
   "J.D. Vance" for "JD Vance" because punctuation is ignored.

### Earlier

Transcribed 2026-08-25 from the official lists (2025 from the 2020 wording) and marked
`needs-review`; nothing approved.

## How to run the next verification

1. `tools/content-verify/fetch.sh`, then `python tools/content-verify/verify.py` (see its README for
   the one-time setup). Read every `DIFFERENCE` against the document; `info` lines are the modelling
   decisions above.
2. If USCIS changed the text, change the content to match **verbatim** (rule 1 in
   [`content/README.md`](../README.md)); if the change is substantive, say so in the pull request.
3. Open *Check for Test Updates*, whitehouse.gov/administration, speaker.gov and
   supremecourt.gov/about/biographies.aspx; update `holder`, `acceptedAnswers`, `since` in
   `dynamic-answers.json` if an office changed hands.
4. Set `verifiedOn` to today's date on every source you actually opened, keep `reviewStatus` at
   `approved` (or set it back to `needs-review` for anything you could not confirm), and run
   `dotnet run --project src/Citiz.Cli -- content validate` and `dotnet test`.
5. Add a dated entry to the log above and open a pull request titled like
   *Re-verify 2025 bank against M-1778 (rev. …)*.

If you find a difference and are not sure, leave the entry as it is and open a
[content correction issue](https://github.com/peopleworks/Citiz/issues/new/choose) quoting what you saw.
