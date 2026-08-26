# Translating Citiz

The interface of Citiz is translated by whoever speaks the language. You do not need to know C#,
.NET or Blazor. If you speak a language Citiz does not, you can add it; if you speak one it has, you
can review it and make its "machine draft" label go away.

## Three languages, on purpose

Citiz keeps three language choices separate ([ADR-0002](../Architecture/ADR-0002-language-separation.md)):

1. **Interface language** — navigation, buttons, notices. This is what you translate here.
2. **Study language** — the language being practised. The naturalization interview is in English, so
   official questions and answers stay in English no matter what the interface says.
3. **Help language** — explanations and support. Reserved for future explanatory content.

Translating the interface never touches official content. That is by design.

## What you edit

```
src/Citiz.Web/wwwroot/i18n/
├── en.json       ← English, the reference
├── es.json       ← Spanish
├── zh-Hans.json  ← Chinese (Simplified)
├── zh-Hant.json  ← Chinese (Traditional)
├── fil.json      ← Filipino
├── vi.json       ← Vietnamese
└── ar.json       ← Arabic
```

Each file is a flat map of **key → text**. Keys never appear on screen; only the text does.

```json
{
  "nav.prepare": "Prepare",
  "home.progressSeen": "questions practiced",
  "exam.rules": "Up to {0} questions. {1} correct answers pass; {2} incorrect answers end the test."
}
```

`{0}`, `{1}`… are placeholders the app fills in. Keep every placeholder the English text has, in
the position that reads naturally in your language.

## Review a language (most wanted)

Five packs were produced by machine translation and are labelled **Machine draft** in Settings until
a fluent speaker reviews them. To review one:

1. Open the pack and `en.json` side by side. Fix anything that reads wrong, is too formal, or uses a
   term the community would not use. Plain, warm language for adult learners; short sentences.
2. Run the validator: `dotnet run --project src/Citiz.Cli -- localization validate`. It reports
   missing keys, extra keys, empty values and placeholder mismatches by name.
3. In `src/Citiz.Localization/SupportedLanguages.cs`, change the pack's status from
   `TranslationReviewStatus.MachineDraft` to `Draft` (you reviewed it) or `Reviewed` (a second fluent
   speaker also reviewed it). That is what learners see in Settings.
4. Open a pull request. Say which language and that you are a fluent speaker.

## Add a language

Say you want Korean (`ko`).

1. Copy `en.json` to `ko.json` and translate the values. Never change the keys.
2. Add one line to `SupportedLanguages.All` in `src/Citiz.Localization/SupportedLanguages.cs`:
   ```csharp
   new("ko", "Korean", "한국어", TextDirection.LeftToRight, TranslationReviewStatus.Draft),
   ```
   Use `TextDirection.RightToLeft` for right-to-left scripts; the whole page follows.
3. If browsers report your language with a tag Citiz should map (like `tl` → `fil`), add it to
   `SupportedLanguages.NormalizeBrowserLanguage`.
4. Run `dotnet run --project src/Citiz.Cli -- localization validate` and `dotnet test`. The tests
   check that every supported language has a pack and every pack is supported.
5. Open a pull request.

## Terms

See [GLOSSARY.md](GLOSSARY.md) for the terms that must be translated consistently — and the ones that
must not be translated at all (Citiz, USCIS, N-400, 65/20, the official test names).

## Machine translation

Machine translation (including AI) may produce a first draft. It must be marked `MachineDraft` in
`SupportedLanguages` until a fluent speaker has reviewed it, and it must never be used for official
content.
