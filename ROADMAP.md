# Roadmap

The founding vision is the [design document](Docs/Design/Citiz-Design-Document-v0.4.es.md) (Spanish).
This file is the short, current list of what comes next, in order. Dates are deliberately absent:
this is a community project, and each item ships when it is right.

## 0.3 — Professional foundation *(this release)*

Both official banks, versioned rules, dynamic answers, official vocabulary, capsules; a deterministic
exam session and answer matcher; a Blazor WebAssembly PWA with five practice modes, dictation, discovery
and one game; seven interface languages; the `citiz` tool; CI, CodeQL, Pages; the documents in this
folder.

## 0.4 — Verified content

The release where "not yet verified" labels start disappearing.

- [ ] Compare the 2008 bank line by line with the official USCIS document; mark approved
- [ ] Compare the 2025 bank with the official 2025 document; mark approved
- [ ] Record the 2025 65/20 question list and enable the mode
- [ ] Re-verify dynamic answers (officeholders) and vocabulary lists; record `verifiedOn`
- [ ] Spanish pack reviewed by a second fluent speaker → `Reviewed`
- [x] Enable GitHub Pages and put the live link in the README (https://peopleworks.github.io/Citiz/)
- [ ] First `good first issue` batch: one capsule per state, capsule review, language review

## 0.5 — Listen and speak

- [ ] Official audio: play the question the way it is asked (browser voice today; recorded human
  audio if licensing allows)
- [ ] Speech-to-text for spoken answers, on-device where the browser supports it; disclosed when not
- [ ] Interview simulation: greeting, N-400 vocabulary questions, reading, writing, civics — the full
  sequence, scored per skill
- [ ] Study plan: a daily "next thing" from the ledger (due reviews, weakest areas, a capsule)

## 0.6 — Explain

- [ ] `ICitizAiService` providers: a local model (Foundry Local / on-device) first, cloud second; both
  opt-in, both restricted to approved content ([design §12](Docs/Design/Citiz-Design-Document-v0.4.es.md))
- [ ] Explanations and mnemonics generated only from approved content, labelled as such
- [ ] Ambiguous-answer evaluation as a second stage after the deterministic matcher

## 0.7 — Everywhere

- [ ] `Citiz.Hybrid`: .NET MAUI Blazor Hybrid host for Android and Windows, sharing the Razor
  components (a `Citiz.UI` Razor class library carved out of `Citiz.Web`)
- [ ] Signed offline content packages with a manifest and a delta sync
- [ ] NuGet packages for the engines; `citiz` as a global tool

## Tell the story

Citiz is free, useful, and mostly unknown. A content track, alongside the engineering one: articles,
videos and shorts — not just showing what Citiz does, but teaching how it was built. Raw material
lives in [`Docs/BuildHistory.md`](Docs/BuildHistory.md), kept as a running log while building rather
than reconstructed later. Not yet scoped: channels/formats, primary audience, and whether it starts
English-first or leans into the same multilingual angle Citiz itself has.

## Later

- More games (lightning map, who am I, order the story, listen and find), "50 states, 50 stories",
  the virtual passport
- Community: moderated study groups, organization dashboards, volunteer instructors — only with a
  moderation model in place first
- Editorial pipeline: the content worker opens the review, not just the log line
- More languages, prioritized by demand from community organizations
- An MCP server exposing the engines to assistants, with the same content and privacy rules

## Principles that do not move

Whatever the version: essential learning without an account; official answers only from official
sources with a visible review status; nothing leaves the device without disclosure; the product is
useful without any AI provider.
