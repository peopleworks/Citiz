# Verifying the exam content

The banks, rules, dynamic answers and vocabulary lists in this folder were transcribed from the
official USCIS lists and are marked `needs-review`. Nothing here is marked `approved` until a person
has opened the official document and compared. This file says what to compare, what the transcribers
flagged as uncertain, and how to record the result.

**Doing this is the most valuable contribution to Citiz right now.** It is also a very good way to
study: you will read every question and every accepted answer, slowly, against the source.

## How to record a verification

For each entry you verified against the official document:

1. Set `"reviewStatus": "approved"` on the entry (a question, a version, a dynamic answer, a
   vocabulary file). A question without its own `reviewStatus` inherits the file's, so approving a
   whole bank is changing the file-level value.
2. Set `"verifiedOn"` on the source you used to today's date (`YYYY-MM-DD`).
3. Run `dotnet run --project src/Citiz.Cli -- content validate` and `dotnet test`.
4. Open a pull request titled like *Verify 2008 bank, questions 1–57 against USCIS PDF*.

If you find a difference, fix the content to match the source verbatim, and say so in the pull
request. If you are not sure, leave the entry `needs-review` and open a content correction issue
with what you saw.

## 2008 Civics Test (`2008/questions.json`)

Official source: USCIS, *100 Civics Questions and Answers (2008 version)* —
https://www.uscis.gov/citizenship/find-study-materials-and-resources/study-for-the-test/100-civics-questions-and-answers-with-mp3-audio-english-version
(the PDF linked from that page is the authoritative text).

Check all 100 prompts and accepted answers. The transcriber flagged these in particular:

- **Q36** (Cabinet-level positions): membership and official order of the list.
- **Q39** (justices on the Supreme Court): recorded as `nine (9)` with the "check testupdates"
  note; the current USCIS page shows only the instruction to check for updates. Decide whether to
  keep the number or make it dynamic.
- **Q40** (Chief Justice): recorded as dynamic (`chief-justice`), because USCIS's current answer is
  "visit uscis.gov/citizenship/testupdates". Confirm.
- **Q87** (American Indian tribes): membership and order of the 22 names.
- **Q92** (states bordering Canada): official order.
- **Q100** (national holidays): includes *Juneteenth*, which USCIS added in 2022; a pre-2022 PDF will
  not have it.
- **Q12, Q48, Q65, Q86**: sentence-style answers; check exact punctuation.
- **Q20, Q23, Q43, Q44**: the bracketed D.C./territory remarks in `note` were reproduced from memory.
- **65/20 list** (`versions.json` → `seniorQuestionNumbers`): confirm the 20 asterisked numbers
  `[6, 11, 13, 17, 20, 27, 28, 44, 45, 49, 54, 56, 70, 75, 78, 85, 94, 95, 97, 99]`.

## 2025 Civics Test (`2025/questions.json`)

Official source: USCIS, *2025 Civics Test* —
https://www.uscis.gov/citizenship-resource-center/naturalization-test-and-study-resources/2025-civics-test
(the *128 Civics Questions and Answers (2025 version)* PDF).

The 2025 test is based on the 2020 civics test with updates. The bank was transcribed from the 2020
wording, with the known 2025 changes applied where the transcriber was confident. **Diff the whole
file against the 2025 PDF**; in particular:

- **Q31** ("Who does a U.S. senator represent?"): recorded with the 2020 answer *Citizens of their
  state*. Confirm the 2025 wording.
- **Q126** (national holidays): *Juneteenth* was added. Confirm position and wording.
- **Q120** (Statue of Liberty): USCIS's remark "[Also acceptable are New Jersey, near New York City,
  and on the Hudson (River).]" is in `note`, not in `acceptedAnswers`. Decide whether the matcher
  should accept those (move them into `acceptedAnswers`) — the transcriber left it as a note.
- **Q23, Q29, Q61, Q62**: bracketed D.C./territory remarks in `note` are from the 2020 wording.
- **65/20 list**: `seniorQuestionNumbers` for 2025 is **empty on purpose**. Copy the asterisked
  question numbers from the 2025 PDF into `versions.json`; that enables the 65/20 practice mode for
  2025. The validator requires at least 10.

## Dynamic answers (`dynamic-answers.json`)

Federal entries (`president`, `vice-president`, `speaker-of-the-house`, `chief-justice`,
`president-party`) name the officeholders as of the transcription. Verify each against its source
(whitehouse.gov/administration, speaker.gov, supremecourt.gov) **on the day you verify**, set
`verifiedOn`, and set `approved`. These go stale with every election and appointment; the content
worker watches the source pages and reports changes.

State and district entries have no holder by design; they only need their `lookupHint` URLs checked.

## Vocabulary (`../english/*.json`)

Sources: the USCIS reading and writing vocabulary PDFs linked from
https://www.uscis.gov/citizenship/find-study-materials-and-resources/study-for-the-test.

Check every word and its official heading. Flagged: the slash forms (`state/states`), the
third-person verb forms (`meets`, `elects`), and the exact punctuation of `Washington, D.C.` in the
writing list.

## Exam rules (`versions.json`)

Source: https://www.uscis.gov/citizenship/learn-about-citizenship/the-naturalization-interview-and-test
and the 2025 test page. Confirm: filing-date boundary (October 20, 2025), bank sizes (100 / 128),
questions asked (10 / 20), passing answers (6 / 12), and the 65/20 rules (10 asked, 6 to pass) for
both versions.
