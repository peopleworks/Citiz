# ADR-0002: Interface, study and help languages are three independent choices

**Status:** Accepted · **Date:** 2026-07-28 (design document), implemented 2026-08-25

## Context

A learner may navigate in Spanish, practise the official examination in English, and want
explanations in Vietnamese. A single "current culture" cannot represent that, and if the interface
language drove content, changing a menu could silently change what counts as an official answer.

## Decision

`LanguageProfile` holds three codes: `InterfaceCulture`, `StudyCulture`, `HelpCulture`. They are
set independently (Settings) and persisted together. The interface language selects the translation
pack; the study language selects what is practised (English for the naturalization interview); the
help language is reserved for explanations and future AI feedback.

Official questions and accepted answers are never generated from a translation pack. Translation
packs contain interface strings only; any educational translation of official content will be a
separate, labelled content type with its own review status.

Every language pack carries a review status (`Source`, `Reviewed`, `Draft`, `MachineDraft`) in
`SupportedLanguages`, shown in Settings, so a learner knows whether a fluent speaker has read it.

## Consequences

- `SupportedLanguages.All` is the single list of languages; the pack files, the validator, the tests
  and the menu all derive from it. Adding a language is one line plus one file.
- Right-to-left is a property of the language definition and is applied to the document element,
  not to a wrapper div, so the whole page (scrollbars, dialogs, focus order) follows it.
- Study languages are a deliberately short list (English; Spanish only for the vocabulary of the
  process), because the interview is in English.
