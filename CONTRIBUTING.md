# Contributing to Citiz

Thanks for being here. This project has one principle, and everything below follows from it:

> **Citiz may teach, explain, connect and personalize. It never invents an official answer or a
> historical fact.** Every fact shown carries its source and its verification status.

If a change makes the app more impressive but less verifiable, it is probably the wrong change.

## Getting set up

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download). Nothing else.

```bash
git clone https://github.com/peopleworks/Citiz.git
cd Citiz
dotnet test                                   # the whole suite
dotnet run --project src/Citiz.Web            # the app, at http://localhost:5000
dotnet run --project src/Citiz.Cli -- --help  # the maintainer's tool
```

`scripts/bootstrap.sh` (or `.ps1`) runs exactly what CI runs: format check, build, tests, content
validation, language-pack validation. Green there means green on the pull request.

## Ways to contribute

### 1. Verify content (no code)

The question banks were transcribed from the official USCIS lists and are marked `needs-review`
until a person has compared them, line by line, with the official document. That comparison is the
most valuable thing you can do today. [`content/exams/VERIFICATION.md`](content/exams/VERIFICATION.md)
lists exactly what to check and how to mark it done. Open the official PDF, open the JSON, compare,
and in the same pull request set `reviewStatus` to `approved` and `verifiedOn` to today's date for
what you verified. Partial verification is welcome: approve the questions you checked, leave the rest.

### 2. Report a wrong or outdated answer (no code)

Use the [content correction template](https://github.com/peopleworks/Citiz/issues/new/choose). Quote
the official source. A correction with a source link gets fixed fast; one without has to wait until
someone finds the source.

### 3. Translate or review a language (no code)

Interface strings are plain JSON files, one per language, in `src/Citiz.Web/wwwroot/i18n/`.
[`Docs/Localization/README.md`](Docs/Localization/README.md) explains how to add a language, how to
review one, and how the review state is shown to learners. Five of the seven packs are machine drafts
waiting for a fluent reader.

### 4. Write a capsule (little code)

"Today in the United States" capsules live in `content/discovery/topics.json`: a few sentences of
plain English, a simpler version for beginners, a handful of words, the questions it gives context
for, and a source from a public body. Facts only; the capsule explains, it never restates an official
answer as its own claim. See [`content/README.md`](content/README.md) for the format.

### 5. Code

The layout is in the [README](README.md#the-layout). The rules that matter:

- **Engines stay pure.** `Citiz.Core`, `Learning`, `Discovery`, `Games`, `Localization` and `AI` do
  not reference Blazor, a database, a cloud SDK or an AI vendor
  ([ADR-0001](Docs/Architecture/ADR-0001-core-boundaries.md)). If a feature needs I/O, it goes behind
  an interface the host implements (`IContentStore`, `ITranslationCatalogLoader`, `ICitizAiService`).
- **Nothing leaves the device without disclosure.** The web client makes no network request beyond
  loading itself and its content. A feature that sends anything anywhere must be opt-in and say what
  it sends, to whom, and what the local alternative is
  ([Docs/Privacy/LOCAL_VS_CLOUD.md](Docs/Privacy/LOCAL_VS_CLOUD.md)).
- **Warnings are errors.** Including NuGet audit warnings and code-style rules from `.editorconfig`.
  `dotnet format Citiz.slnx` fixes style; CI verifies it.
- **Public surface is documented.** Every public type and member in `src/` libraries has an XML doc
  comment that says what it is *for*, not just what it is called.
- **Every behaviour change lands with a test.** The exam rules in particular: a change to how a
  version is chosen or a sitting is scored needs a test that a reader can match against the USCIS
  rule it implements.
- **Accessibility is an acceptance criterion.** Keyboard only, screen reader (the live region is
  `#citiz-live`), 44px targets, nothing conveyed by colour alone, and it must work with the interface
  in Arabic (right-to-left; use logical CSS properties).

### 6. Design, documentation, accessibility review

Open an issue describing what you want to improve. Mock-ups, plain-language rewrites of the
interface, and accessibility audits are all welcome.

## Pull requests

- One idea per pull request. A content fix and a code change are two pull requests.
- Fill in the template. For content, the official source is the evidence a reviewer needs.
- Reviews follow [`.github/CODEOWNERS`](.github/CODEOWNERS): content maintainers for `content/`,
  technical maintainers for the rest ([GOVERNANCE.md](GOVERNANCE.md)).
- Commit messages say what changed and why, in a sentence a person would write.

## Decisions

Significant architecture decisions are recorded as ADRs in `Docs/Architecture/`; significant
editorial decisions as EDRs in `Docs/Editorial/`. If your change contradicts one, propose a new
record in the same pull request rather than working around the old one.

## What we do not accept

- Official answers that do not match the cited source, or that are paraphrased "to help".
- Officeholder names inside a question bank (they belong in `dynamic-answers.json`).
- Any feature that requires an account for essential learning.
- Personal immigration data, case numbers, recordings or documents — in issues, in tests, anywhere.
- Legal advice, in the app or in the repository. Citiz points to official sources; it does not
  interpret them for a person's case.
