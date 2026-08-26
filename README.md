# Citiz

> Prepare · Communicate · Discover · Participate

[![CI](https://img.shields.io/github/actions/workflow/status/peopleworks/Citiz/ci.yml?branch=main&label=CI&logo=github)](https://github.com/peopleworks/Citiz/actions/workflows/ci.yml)
[![CodeQL](https://img.shields.io/github/actions/workflow/status/peopleworks/Citiz/codeql.yml?branch=main&label=CodeQL&logo=github)](https://github.com/peopleworks/Citiz/actions/workflows/codeql.yml)
[![License: MIT](https://img.shields.io/badge/code-MIT-blue)](LICENSE)
[![Content: CC BY 4.0](https://img.shields.io/badge/content-CC%20BY%204.0-blue)](content/README.md#licensing)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Blazor WebAssembly](https://img.shields.io/badge/Blazor-WASM-512BD4?logo=blazor&logoColor=white)](https://learn.microsoft.com/aspnet/core/blazor/)

*¿Prefieres leer en español? → [README.es.md](README.es.md)*

**Citiz** is a free, open-source, multilingual, privacy-first companion for people preparing for
United States citizenship. It practices the official civics questions the way an officer asks them,
drills the English test vocabulary, and teaches something about the country every day — and it does
all of it in your browser.

> 🔒 **Citiz runs entirely on your device.** There is no account, no server that sees what you
> study, no analytics. Your progress is saved in your browser and you can download or delete it at any
> time. The only thing Citiz ever asks you is *when you filed Form N-400*, so it can pick the right
> version of the test — and even that is optional.

Citiz is an **independent educational tool**. It is not affiliated with USCIS or any government
agency, it does not give legal advice, and it cannot guarantee the outcome of an interview or
application.

---

## What it does today (v0.3)

| Pillar | Built | How |
| --- | --- | --- |
| **Prepare** | Both official civics banks: **2008** (100 questions) and **2025** (128 questions) | Flashcards with spaced review, multiple choice, type-the-answer with a deterministic checker, a **practice test scored exactly like the real one** (stops the moment the outcome is decided), and a browsable bank with sources |
| **Communicate** | The official **reading** and **writing** vocabulary lists | Tap a word to hear it (browser voice, on-device where the browser allows), dictation practice |
| **Discover** | Twelve "Today in the United States" capsules | Short sourced pieces linked to the questions they give context for |
| **Play & Learn** | *Civics challenge* | Ten multiple-choice rounds where every option is a real official answer; results count as practice |
| **Languages** | 7 interface languages | English, Spanish, Chinese (Simplified and Traditional), Filipino, Vietnamese, Arabic (right-to-left) — interface, study and help language are independent |

Also: a `citiz` command-line tool that validates every content file and language pack (the same
checks CI runs), resolves which test applies to a filing date, and runs a practice sitting in the
terminal; an optional API; a worker that watches the official sources for changes; a Dockerfile; a
GitHub Pages deployment.

**Not built yet** (designed, on the [roadmap](ROADMAP.md)): speech recognition and interview
simulation, AI explanations, the .NET MAUI hybrid apps, community features, the remaining games.

## Which test do I take?

USCIS administers two versions of the civics test, depending on when Form N-400 was filed. Citiz
models this as data, not code, in [`content/exams/versions.json`](content/exams/versions.json):

| N-400 filed | Version | Bank | Asked | Pass | Test ends at |
| --- | --- | --- | --- | --- | --- |
| **Before October 20, 2025** | 2008 Civics Test | 100 | up to 10 | 6 correct | 5 incorrect |
| **On or after October 20, 2025** | 2025 Civics Test | 128 | up to 20 | 12 correct | 9 incorrect |

Applicants who are 65 or older with 20 or more years as permanent residents study a designated subset
of 20 questions and are asked up to 10 (the *65/20 special consideration*). Citiz has the 2008 list;
the 2025 list is still to be copied from the official document, so that mode is disabled for 2025
until it is — Citiz does not guess.

## Content you can trust, because you can check it

Everything a learner is told as a fact lives in [`content/`](content/README.md) as plain JSON, with
three rules that the validator and the tests enforce:

1. **Official text is transcribed, not paraphrased**, including the parentheses USCIS uses for
   optional words: `"(U.S.) Constitution"`. The answer checker understands that notation.
2. **Nothing is published without a source and a review status.** The interface labels anything that
   is not `approved`. Marking content approved is a human act.
3. **Answers that depend on who holds an office** (President, Speaker, your governor…) are never
   written into the bank. They live in `dynamic-answers.json` and are re-verified on their own
   schedule.

```
$ dotnet run --project src/Citiz.Cli -- content report

  File                                 Total Approved  Pending   By status
  exams/versions.json                      2        0        2   needs-review 2
  exams/2008/questions.json              100        0      100   needs-review 100
  exams/2025/questions.json              128        0      128   needs-review 128
  ...
```

That report is honest about where the project stands: the banks were transcribed from the official
lists and still need a maintainer to compare them line by line with the USCIS documents before they are
marked approved. [`content/exams/VERIFICATION.md`](content/exams/VERIFICATION.md) is the checklist.
Doing that comparison is the single most valuable contribution right now — and it is also, not by
accident, a very good way to study.

## Run it

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download). Nothing else.

```bash
git clone https://github.com/peopleworks/Citiz.git
cd Citiz
dotnet run --project src/Citiz.Web            # the app, at http://localhost:5000
```

The maintainer's tool:

```bash
dotnet run --project src/Citiz.Cli -- content validate          # every content file, every rule
dotnet run --project src/Citiz.Cli -- content report            # what still needs a human
dotnet run --project src/Citiz.Cli -- localization validate     # every language pack against en.json
dotnet run --project src/Citiz.Cli -- exam resolve 2025-11-03   # which test applies to that filing date
dotnet run --project src/Citiz.Cli -- exam simulate --version 2025   # a practice sitting in the terminal
```

Everything CI runs, in one go: `scripts/bootstrap.sh` (or `.ps1`). Docker: `docker build -t citiz . && docker run --rm -p 8080:80 citiz`.

## The layout

| Project | What it is |
| --- | --- |
| `src/Citiz.Core` | The domain: versioned test rules, questions with provenance, the deterministic exam session and answer matcher. No I/O, no UI, no AI. |
| `src/Citiz.Content` | Loads, maps and validates `content/` from disk or over HTTP. |
| `src/Citiz.Learning` | Progress, mastery and spaced review. Storage-agnostic. |
| `src/Citiz.Discovery` | The daily capsule and the connections between capsules and questions. |
| `src/Citiz.Games` | The game catalog, adaptive difficulty, multiple-choice building, the civics challenge. |
| `src/Citiz.Localization` | The three-language profile, supported languages, translation catalogs and their validator. |
| `src/Citiz.AI` | The AI contract and the no-AI fallback. Providers plug in here; the product works without one. |
| `src/Citiz.Web` | Host: the Blazor WebAssembly PWA. |
| `src/Citiz.Cli` | Host: `citiz`, the maintainer's tool. |
| `src/Citiz.Api` | Host: optional server exposing the content and evaluator over HTTP. |
| `src/Citiz.ContentWorker` | Host: polls the official sources and reports changes for human review. |
| `content/` | The open content repository: banks, rules, dynamic answers, vocabulary, capsules, source catalog, schemas. |
| `tests/` | xUnit. The content that ships must validate; the language packs must agree with English. |
| `Docs/` | Architecture decisions (ADR), editorial decisions (EDR), localization and privacy guides, the founding design document. |

The engines never depend on Blazor, a database, a cloud or an AI vendor
([ADR-0001](Docs/Architecture/ADR-0001-core-boundaries.md)); the browser is the primary host and
the server is optional ([ADR-0003](Docs/Architecture/ADR-0003-local-first-client.md)).

## Contributing

The most useful contributions need no C# at all:

- **Verify content** against the official documents and mark it approved — see
  [`content/exams/VERIFICATION.md`](content/exams/VERIFICATION.md).
- **Review a language pack** if you speak the language — see
  [`Docs/Localization/README.md`](Docs/Localization/README.md). Five of the seven packs are machine
  drafts waiting for a fluent reader.
- **Report a wrong or outdated answer** with the
  [content correction template](https://github.com/peopleworks/Citiz/issues/new/choose).

Code, accessibility, design and documentation contributions are equally welcome; start with
[CONTRIBUTING.md](CONTRIBUTING.md). Everyone here follows the [Code of Conduct](CODE_OF_CONDUCT.md).

## License and notice

Code is [MIT](LICENSE). Editorial content written for Citiz is CC BY 4.0. USCIS material is a work of
the United States Government and in the public domain; every other source keeps its own license,
recorded on the entry ([details](content/README.md#licensing)).

Citiz is built with .NET 10 and Blazor WebAssembly by **Pedro Hernández (PeopleWorks)**, Microsoft
MVP for .NET, with the community — *por y para la comunidad*. It exists because preparing for
citizenship should not depend on the ability to pay, on speaking English already, or on handing your
data to anyone.
