# Citiz content

Everything a learner can be told as a *fact* lives in this folder, as plain JSON that anyone can read,
diff and correct in a pull request. The application code never contains an official answer; it only
loads what is here.

```
content/
├── schemas/                    JSON Schemas for every file below (editor tooling and documentation)
├── exams/
│   ├── versions.json           The civics-test versions and their administration rules
│   ├── dynamic-answers.json    Answers that change with elections and appointments
│   ├── 2008/questions.json     Official 100-question bank (N-400 filed before 2025-10-20)
│   └── 2025/questions.json     Official 128-question bank (N-400 filed on or after 2025-10-20)
├── english/
│   ├── reading-vocabulary.json Official reading vocabulary for the English test
│   └── writing-vocabulary.json Official writing vocabulary for the English test
├── discovery/
│   └── topics.json             "Today in the United States" capsules
├── audio/
│   └── packs.json              Recordings the learner can download once: official USCIS tracks, the synthetic Citiz voice
└── sources/
    └── sources.json            Catalog of official sources the content worker watches for changes
```

Validate everything with:

```bash
dotnet run --project src/Citiz.Cli -- content validate
dotnet run --project src/Citiz.Cli -- content report      # what still needs a human to verify it
```

## The three rules

1. **Official text is transcribed, not paraphrased.** Questions and accepted answers keep the exact
   USCIS wording, including the parentheses USCIS uses for optional words: `"(U.S.) Constitution"`.
   The answer matcher understands that notation; a paraphrase would silently change what is accepted.
2. **Nothing is published without a source and a review status.** Every file and every entry that can
   stand alone carries `sources` and `reviewStatus`. The interface labels anything that is not
   `approved`. Marking content `approved` is a human act: open the cited source, compare, then change
   the status in the same pull request.
3. **Answers that depend on who holds an office are never written into the question bank.** The
   question carries a `dynamicAnswerKey`; the current officeholder lives in `dynamic-answers.json`,
   which is re-verified on its own schedule. When an office changes hands, one file changes.

## Review status

| Value | Meaning |
| --- | --- |
| `draft` | Written, not yet checked against its source. Hidden from official practice modes. |
| `needs-review` | Complete and sourced, waiting for a content maintainer to verify it. Shown with a label. |
| `approved` | Verified against the cited source by a content maintainer. |
| `outdated` | Was approved, but the source has changed since. Must be re-verified. |

See [`Docs/Editorial/EDR-0002-review-states.md`](../Docs/Editorial/EDR-0002-review-states.md).

## Common shapes

Dates are `YYYY-MM-DD`. Keys are camelCase. A source is:

```json
{
  "authority": "USCIS",
  "title": "128 Civics Questions and Answers (2025 version)",
  "url": "https://www.uscis.gov/citizenship-resource-center/naturalization-test-and-study-resources/2025-civics-test",
  "verifiedOn": null,
  "license": "Public domain (U.S. Government work, 17 U.S.C. § 105)"
}
```

`verifiedOn` is the date a maintainer last compared the content with that source; `null` means never.
The log of what was compared, and the decisions taken, is
[`exams/VERIFICATION.md`](exams/VERIFICATION.md); the comparison itself is reproducible with
[`tools/content-verify/`](../tools/content-verify/README.md).

## `exams/versions.json`

```json
{
  "$schema": "../schemas/exam-versions.schema.json",
  "versions": [
    {
      "id": "2025",
      "displayName": "2025 Civics Test",
      "filingFrom": "2025-10-20",
      "filingTo": null,
      "bankSize": 128,
      "standard": { "questionsAsked": 20, "passingAnswers": 12, "failingAnswers": 9 },
      "seniorConsideration": { "questionsAsked": 10, "passingAnswers": 6, "failingAnswers": 5 },
      "seniorQuestionNumbers": [],
      "reviewStatus": "needs-review",
      "sources": []
    }
  ]
}
```

- `filingFrom` / `filingTo` are inclusive N-400 filing dates; `null` means unbounded. Exactly one
  version must apply to any date, and the validator checks that.
- `passingAnswers + failingAnswers` must equal `questionsAsked + 1`: the officer stops the moment the
  outcome is decided, so this is the only shape a real rule set can have.
- `seniorQuestionNumbers` are the official numbers USCIS marks with an asterisk for applicants who are
  65 or older and have been permanent residents for 20 or more years. Both versions have their list
  recorded from the official document. If a future version's list is not yet known, leave the array
  **empty**: an empty list disables the 65/20 mode for that version, which is safer than guessing.

## `exams/<version>/questions.json`

```json
{
  "$schema": "../../schemas/questions.schema.json",
  "versionId": "2025",
  "reviewStatus": "needs-review",
  "sources": [],
  "questions": [
    {
      "id": "2025-002",
      "number": 2,
      "category": "American Government",
      "subcategory": "Principles of American Government",
      "prompt": "What is the supreme law of the land?",
      "acceptedAnswers": ["(U.S.) Constitution"]
    },
    {
      "id": "2025-038",
      "number": 38,
      "category": "American Government",
      "subcategory": "System of Government",
      "prompt": "What is the name of the President of the United States now?",
      "acceptedAnswers": [],
      "dynamicAnswerKey": "president",
      "note": "Visit uscis.gov/citizenship/testupdates for the name of the President of the United States."
    }
  ]
}
```

- `id` is `<versionId>-<number padded to 3 digits>`. Numbers are contiguous from 1 to `bankSize`.
- `category` and `subcategory` are the official section headings, in title case.
- `acceptedAnswers` has one element per official accepted answer, in official order. Keep USCIS's
  parentheses; do not add answers USCIS does not list. When USCIS attaches a bracketed remark to an
  answer (`Liberty Island [Also acceptable are New Jersey, …]`), the remark goes in `note` verbatim and
  the answers it names are listed after the main ones, so the matcher accepts what the officer accepts.
- Curly quotes and apostrophes in the official PDFs are written as straight ASCII quotes; the matcher
  and the verification tool ignore the difference.
- `dynamicAnswerKey`, `note` and a per-question `reviewStatus` are optional. A question without its own
  `reviewStatus` inherits the file's.
- The keys a question may reference are exactly those defined in `dynamic-answers.json`. A question
  whose official answer is "Visit uscis.gov/citizenship/testupdates …" or "Answers will vary …" is
  always dynamic — including the number of Supreme Court justices in the 2008 test, which USCIS moved
  to its updates page.

## `exams/dynamic-answers.json`

```json
{
  "$schema": "../schemas/dynamic-answers.schema.json",
  "answers": [
    {
      "key": "president",
      "office": "President of the United States",
      "scope": "federal",
      "holder": "Donald J. Trump",
      "acceptedAnswers": ["Donald J. Trump", "Donald Trump", "Trump"],
      "since": "2025-01-20",
      "verifiedOn": null,
      "lookupHint": null,
      "reviewStatus": "needs-review",
      "sources": []
    },
    {
      "key": "state-governor",
      "office": "Governor of your state",
      "scope": "state",
      "holder": null,
      "acceptedAnswers": [],
      "since": null,
      "verifiedOn": null,
      "lookupHint": "Find your governor at usa.gov/state-governor.",
      "reviewStatus": "approved",
      "sources": []
    }
  ]
}
```

`scope` is `federal`, `state` or `district`. Only federal entries carry a `holder`; state and district
entries carry a `lookupHint` telling the learner where to find their own answer, because Citiz does not
ask where you live. `acceptedAnswers` lists exactly the forms USCIS accepts on its *Check for Test
Updates* page; the office's own site confirms the holder.

## `english/*-vocabulary.json`

```json
{
  "$schema": "../schemas/vocabulary.schema.json",
  "kind": "reading",
  "reviewStatus": "needs-review",
  "sources": [],
  "groups": [
    { "category": "People", "words": ["Abraham Lincoln", "George Washington"] }
  ]
}
```

## `discovery/topics.json`

```json
{
  "$schema": "../schemas/discovery-topics.schema.json",
  "topics": [
    {
      "id": "grand-canyon",
      "category": "geography",
      "title": "The Grand Canyon",
      "summary": "Two or three sentences, plain English, facts only.",
      "simpleEnglish": "The same idea in shorter sentences for beginners.",
      "estimatedMinutes": 3,
      "difficulty": "beginner",
      "vocabulary": ["canyon", "river", "national park"],
      "relatedQuestionIds": ["2025-090"],
      "relatedPlaces": ["Arizona"],
      "reviewStatus": "draft",
      "sources": []
    }
  ]
}
```

`category` is one of `history`, `geography`, `people`, `institutions`, `culture`, `innovation`,
`nature`. `difficulty` is `beginner`, `intermediate` or `advanced`. Every `relatedQuestionIds` entry
must exist in a question bank; the validator checks it. Capsules are editorial content: they explain,
they never restate an official answer as their own fact.

## `audio/packs.json`

The catalog of recordings a learner can download as a pack and keep on the device. The MP3 files
are not in this repository: they live at each pack's `baseUrl` (any HTTPS host with CORS), and
[`tools/audio/`](../tools/audio/README.md) builds both the files and this catalog.

```json
{
  "$schema": "../schemas/audio-packs.schema.json",
  "packs": [
    {
      "id": "uscis-2008",
      "kind": "official",
      "title": "Official USCIS recordings · 2008 test",
      "description": "The 100 questions of the 2008 test with their answers, read by USCIS",
      "versionId": "2008",
      "version": 1,
      "baseUrl": "https://example.org/citiz-audio/uscis-2008/v1/",
      "sizeBytes": 30541211,
      "license": "Public domain (U.S. Government work, 17 U.S.C. § 105)",
      "voice": null,
      "generatedOn": null,
      "reviewStatus": "approved",
      "sources": [],
      "clips": [
        { "id": "r-2008-036", "role": "recording", "file": "q036.mp3", "bytes": 1065146, "seconds": 66.6, "sha256": "…", "questionId": "2008-036" }
      ]
    }
  ]
}
```

- `kind` is `official` (recorded by the authority; public domain) or `synthetic` (generated once from
  the verified text by the maintainer; `voice` and `generatedOn` say how). The interface labels every
  clip accordingly, and a synthetic clip is never presented as official.
- `role` says what a clip contains and therefore where it may play: `recording` (an official track
  that reads the question **and** its answers, so it is offered only after the answer is revealed and
  in Browse), `prompt` (question only, behind "Listen"), `answer` (one accepted answer, with
  `answerIndex`), `word` (one vocabulary word). Official packs hold recordings only; synthetic packs
  never do. Answers of dynamic questions are never recorded.
- The validator checks that every clip points at a real question, answer index or word, that the
  bytes add up to `sizeBytes`, and that digests are 64-digit hex. `version` bumps when files change;
  the app caches by id and version.

## `sources/sources.json`

The catalog the content worker polls. Each entry names an official page or document, how often to
check it (ISO 8601 duration), and which content files depend on it, so a change can be routed to the
right review.

```json
{
  "$schema": "../schemas/sources.schema.json",
  "sources": [
    {
      "id": "uscis-2025-civics-test",
      "authority": "USCIS",
      "title": "2025 Civics Test",
      "url": "https://www.uscis.gov/citizenship-resource-center/naturalization-test-and-study-resources/2025-civics-test",
      "format": "html",
      "checkEvery": "P7D",
      "monitor": true,
      "requiresHumanReview": true,
      "feeds": ["exams/2025/questions.json", "exams/versions.json"],
      "lastHash": null,
      "lastCheckedOn": null
    }
  ]
}
```

## Licensing

Works of the United States Government are in the public domain in the United States
(17 U.S.C. § 105); that covers the USCIS question banks and vocabulary lists. Editorial content written
for Citiz (summaries, capsules, notes) is offered under CC BY 4.0. Anything taken from a third party
keeps its own license, recorded on the entry. Nothing is added without a `license` field.
