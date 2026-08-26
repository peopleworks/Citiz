## What this changes

<!-- One or two sentences. If it fixes an issue, link it: Fixes #123 -->

## Why

<!-- The reasoning, not the diff. For a content change, the official source is the evidence a
     reviewer needs; quote it. -->

## Checklist

- [ ] `dotnet test` passes with no new warnings (warnings are errors here)
- [ ] `dotnet format Citiz.slnx --verify-no-changes` is clean
- [ ] One idea per pull request

If you touched **content** (`content/**`):

- [ ] `dotnet run --project src/Citiz.Cli -- content validate` is clean
- [ ] Official text is verbatim from the source, including USCIS's parentheses
- [ ] Every changed entry has a source and a review status; anything I verified myself against the source is marked `approved` with today's `verifiedOn`
- [ ] No officeholder name went into a question bank (they live in `dynamic-answers.json`)

If you touched **language packs** (`src/Citiz.Web/wwwroot/i18n/*.json`):

- [ ] `dotnet run --project src/Citiz.Cli -- localization validate` is clean
- [ ] I am a fluent speaker of this language, or I have marked the pack's status accordingly in `SupportedLanguages`

If you touched the **web client**:

- [ ] Works with keyboard only, and with the interface in Arabic (right-to-left)
- [ ] No new network request; nothing leaves the device without disclosure (see `Docs/Privacy/LOCAL_VS_CLOUD.md`)
