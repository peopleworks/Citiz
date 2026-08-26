# Local versus cloud

Every feature in Citiz declares one execution class, and the interface discloses anything that is
not local at the moment the learner enables it. This is the policy; [PRIVACY.md](../../PRIVACY.md)
is the learner-facing summary.

## Execution classes

| Class | Meaning | Disclosure |
| --- | --- | --- |
| **Local** | Runs on the device. No network request beyond loading the app and its static content. | None needed; the footer pill says "Runs on your device" |
| **Browser service** | Handled by the browser itself, which may or may not use a network (speech synthesis, speech recognition). | The interface states which it is when it can detect it |
| **Optional cloud** | A remote service the learner opted into. Off by default. | Before enabling: what is sent, to what kind of service, what for, how long it may be kept, and the local alternative |
| **Requires account** | Needs identity (sync, groups, dashboards). | Not built; when built, never required for essential learning |

## Current features

| Feature | Class | Implementation |
| --- | --- | --- |
| Questions, answers, versions, rules | Local | Static JSON under `/content`, cached by the service worker |
| Vocabulary lists, capsules | Local | Same |
| Interface translations | Local | Static JSON under `/i18n` |
| Answer checking | Local | `AnswerMatcher` compiled into the WebAssembly bundle; `NoAiFallbackService` declares `AiExecutionClass.Local` |
| Progress, spaced review, settings | Local | `localStorage`; exportable and deletable in Settings |
| Reading text aloud | Browser service | Web Speech API; `SpeechService.IsLocalVoiceAsync` reports whether the chosen voice is on-device and the interface says so |
| Speech recognition | — | Not built. Will be a browser service where available, disclosed; cloud only as opt-in |
| AI explanations and conversation | — | Not built. `ICitizAiService.ExecutionClass` exists so every provider must declare itself; cloud providers are opt-in |
| Sync, groups, organization dashboards | — | Not built; requires account by nature; never required for essential learning |

## Rules for new features

1. Declare the class in the pull request. If it is not Local, explain what leaves the device.
2. Local by default. A cloud feature ships with its local alternative or with a clear statement that
   there is none.
3. Disclosure in the interface, in the learner's language, before the first use — not in a policy
   page only.
4. No feature may send the filing date, progress data, typed or spoken answers, or any settings to a
   service the learner did not explicitly choose.
5. The engines cannot make network calls (ADR-0001); a reviewer can verify a feature's class by
   reading project references and the host code alone.
