# Changelog

All notable changes to Citiz. The format follows [Keep a Changelog](https://keepachangelog.com/);
versions follow [Semantic Versioning](https://semver.org/).

## [0.4.0] — 2026-09-01

Verified content. Every official fact Citiz shows was compared with its official document on
2026-09-01; `citiz content report` answers "Every entry is approved" and the "Not yet verified" labels
are gone from the interface because the content earned it. The log of what was compared, what was
wrong and the decisions taken is `content/exams/VERIFICATION.md`.

### Fixed

- **2025 bank**: 13 questions differed from USCIS form M-1778 (09/25), because the bank had been
  transcribed from the 2020 test wording. Corrected verbatim: the answers to questions 3, 13, 31, 33,
  41, 60, 68, 93, 115 and 118; the prompt of question 97; the Cabinet-level list of question 48
  ("Secretary of War (Defense)", "Vice-President", and six more positions); the official closing line
  of question 117 kept as its note.
- **2008 bank, question 39** (number of Supreme Court justices) is now a dynamic answer
  (`supreme-court-justices`), as USCIS moved it to its *Check for Test Updates* page.
- **Statue of Liberty** (2008 Q95, 2025 Q120): the answers USCIS marks "also acceptable" — New Jersey,
  near New York City, on the Hudson (River) — are accepted by the matcher, with the official remark
  kept as the note.
- **Dynamic answers** list exactly the forms USCIS accepts; the Chief Justice is recorded as
  "John G. Roberts, Jr." as the Court writes it.
- **Discovery capsules**: the Grand Canyon capsule claimed the canyon's rocks are among the oldest
  exposed on Earth; the National Park Service says otherwise (about two billion years, against four
  billion for the oldest known). Reworded, and every capsule's facts now cite the page that states them.

### Added

- The 65/20 question list for the 2025 test (20 questions), which enables the 65/20 practice mode for
  applicants who filed on or after October 20, 2025.
- `tools/content-verify/`: downloads the official USCIS documents and compares every prompt, answer,
  heading, asterisk and vocabulary word with `content/`; exit code 1 on any difference.
- `verifiedOn` dates on every source; the official PDFs and speaker.gov added to the monitored
  sources catalog.

### Changed

- `content/exams/VERIFICATION.md` is now a dated verification log with the transcription decisions,
  instead of a to-do list.
- READMEs, CONTRIBUTING and the roadmap describe the verified state.

## [0.3.0] — 2026-08-25

The professional foundation. The scaffold from 0.2.0 did not compile; this release replaces every
source file and turns the design into a working, tested product.

### Added

- **Content repository** (`content/`): both official civics banks (2008: 100 questions; 2025: 128
  questions) transcribed verbatim with sources; versioned administration rules including the 65/20
  special consideration; dynamic answers for officeholders kept apart from the banks; the official
  reading and writing vocabulary; twelve discovery capsules; a catalog of monitored official sources;
  JSON Schemas for every file; a written format spec with the three content rules.
- **Review status** on every entry (`draft`, `needs-review`, `approved`, `outdated`), labelled in
  the interface, summarized by `citiz content report`.
- **`Citiz.Core`**: `ExamVersion` with data-driven rules, `ExamPolicy` (version by filing date, pass
  and fail thresholds), `ExamSession` (a sitting that stops the moment the outcome is decided,
  reproducible with a seed), `AnswerMatcher` (deterministic evaluator that understands USCIS's
  parenthesis notation), `QuestionBank`, `DynamicAnswer`, `SourceReference`.
- **`Citiz.Content`**: `IContentStore` (disk and HTTP implementations), source-generated JSON
  loading, `ContentMapper` with contributor-readable errors, `ContentRepository` with per-file
  caching, `ContentValidator` with cross-file checks (contiguous numbering, bank size, overlapping
  versions, dynamic keys, 65/20 numbers, capsule references, feeds).
- **`Citiz.Learning`**: `ProgressLedger` with streak-based spaced review and a versioned, exportable
  snapshot.
- **`Citiz.Discovery`**: date-based daily pick (no profiling), related capsules and questions.
- **`Citiz.Games`**: game catalog with honest status, `MultipleChoiceBuilder` whose distractors are
  real official answers, the *Civics challenge*.
- **`Citiz.Localization`**: `SupportedLanguages` as the single source of truth with a review status
  per pack, browser-language normalization, `LocalizationService` decoupled from HTTP, a pack
  validator (parity, empty values, placeholders).
- **`Citiz.AI`**: `ICitizAiService` with an execution class for disclosure; `NoAiFallbackService`.
- **`Citiz.Web`**: rebuilt as a real app — home with the daily capsule and live progress; Prepare
  with flashcards, multiple choice, typed answers, a practice test and a browsable bank; Communicate
  with the vocabulary lists read aloud and dictation; Discover with capsule pages; Play & Learn;
  Settings with language profile, exam settings, export and delete; About. Accessible layout (skip
  link, live region, focus, 44px targets), dark mode, right-to-left, offline-first service worker,
  PWA icons.
- **`citiz` CLI**: `content validate`, `content report`, `localization validate`,
  `localization status`, `exam resolve`, `exam simulate` (a practice sitting in the terminal).
- **`Citiz.Api`** rewritten over the content repository; **`Citiz.ContentWorker`** now hashes the
  monitored sources and reports changes.
- **Tests**: 127 across Core, Content, Learning, Games and Localization, including "the shipped
  content validates" and "the language packs agree with English".
- **Repository**: central package management with audit-as-error, reproducible builds, XML docs on
  every public member, `.editorconfig` enforced in build and CI, CI/CodeQL/Pages/Dependabot
  workflows, CODEOWNERS, issue templates for bugs, content corrections and translations, bilingual
  README, CONTRIBUTING, SECURITY, PRIVACY, GOVERNANCE, ROADMAP, ADR-0003, EDR-0002, the founding
  design document with corrected numbering, a content verification checklist.

### Changed

- Interface language packs expanded from 39 to ~200 keys; five packs are labelled machine drafts.
- The 65/20 designation moved from a per-question flag to a per-version list, so an unrecorded list
  disables the mode instead of silently degrading it.

### Removed

- Hard-coded exam versions in code, the fake progress numbers in the interface, and the two-question
  sample bank.

## [0.2.0] — 2026-08-25

Multilingual local-first scaffold (generated; did not compile).

## [0.1.0] — 2026-08-24

Initial scaffold.
