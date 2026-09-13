# ADR-0004: The first AI provider is a small model on the device

**Status:** Proposed · **Date:** 2026-09-12 · **Relates to:** [ROADMAP](../../ROADMAP.md) 0.5 and 0.6,
[ADR-0001](ADR-0001-core-boundaries.md), [ADR-0003](ADR-0003-local-first-client.md),
[LOCAL_VS_CLOUD](../Privacy/LOCAL_VS_CLOUD.md)

## Context

The roadmap already asks for this, in two places: *"Speech-to-text for spoken answers, on-device where the
browser supports it; disclosed when not"* (0.5), and *"`ICitizAiService` providers: a local model (Foundry
Local / on-device) first, cloud second; both opt-in, both restricted to approved content"*, with
*"Ambiguous-answer evaluation as a second stage after the deterministic matcher"* (0.6).

The contract is ready for it. `AnswerEvaluationRequest` hands a provider the official `AcceptedAnswers` and
says it may only accept those; feedback comes back as a translation key, so no provider writes prose a learner
reads unreviewed; `AiExecutionClass.Local` exists. The only provider today is `NoAiFallbackService`.

A model now fits the job on a phone. **Gemma 4** (Google, announced 2026-04-02) is released under **Apache
2.0**. Its E2B size (2.3B effective parameters, 5.1B in total) takes audio natively — speech recognition and
speech translation, clips of up to 30 seconds at 16 kHz — and ships as a LiteRT-LM build of about 2.6 GB for
Android. It runs on the maintainer's Android phone in Google's AI Edge Gallery app. Gemma 3n does the same job
but is governed by the Gemma Terms of Use and its Prohibited Use Policy, which an MIT project should not
inherit when an Apache 2.0 model exists.

The runtimes that can host it inside an app, as of 2026-09-12:

| Runtime | Language APIs | Gemma 4 audio | Fit for Citiz |
| --- | --- | --- | --- |
| [LiteRT-LM](https://github.com/google-ai-edge/LiteRT-LM) (Apache 2.0) | Python, Kotlin and C++ stable; Swift and JavaScript early preview; Flutter community | yes; on 2026-09-12 its Python package (0.17.0) ran `gemma-4-E2B-it.litertlm`, byte-identical (SHA-256) to the file AI Edge Gallery 1.0.19 downloads on Android, on a Windows laptop CPU, taking 0.21–0.34 s per second of audio | Android through a .NET for Android binding; iOS waits for Swift. The desktop Python package can test prompts and accents before any app code exists |
| [ONNX Runtime GenAI](https://github.com/microsoft/onnxruntime-genai) | C#, C++, Python | the runtime supports it since v0.15.0 (2026-07-30), but no Gemma 4 model in its format was published as of 2026-09-12: the `onnx-community` exports follow the Transformers.js layout, so a model would have to be built from the full weights with its model builder | pure C#, once such a model exists; untested on a phone with audio |
| [llama.cpp](https://github.com/ggml-org/llama.cpp) | C/C++ | since April 2026 (`mtmd`) | native interop only |

## Decision (proposed)

1. **The first `ICitizAiService` provider is local**: Gemma 4 E2B on the device, `AiExecutionClass.Local`,
   opt-in, and disclosed at the moment of use. It lives in `Citiz.Hybrid`; ADR-0001 keeps model SDKs out of the
   engines.
2. **It does two jobs, in this order.** First, it transcribes a spoken answer into
   `AnswerEvaluationRequest.Response`, and the deterministic matcher judges it as it judges a typed one. Second,
   only when the matcher cannot settle a response, it chooses one of `AcceptedAnswers` or none. It never
   produces text a learner reads.
3. **Android first.** iOS and Mac Catalyst follow when LiteRT-LM's Swift API leaves preview, or earlier if
   the C# runtime proves itself on a phone. The browser comes last.
4. **Nothing reaches learners until the gates below pass**, and the product stays whole without it:
   `NoAiFallbackService` answers whenever the device cannot run the model or the learner declines.

## Gates before any learner sees it

1. **The spike.** In `Citiz.Hybrid`, on a real Android phone: load Gemma-4-E2B and transcribe one WAV file.
   Record load time, peak memory, seconds per answer, the phone model, the runtime version, and the model
   file's SHA-256: the same file name has been published with different contents, and a repository commit
   alone does not say which one a device holds. If the first runtime route fails, try the other (LiteRT-LM through a binding, ONNX Runtime GenAI
   from C#). If both fail, this ADR is rejected.
2. **Accent.** Most learners are not native English speakers, and a recogniser that mishears an accented
   answer marks a right answer wrong. Measure the word error rate on official accepted answers spoken by
   volunteers with the accents of Citiz's help languages, per language. Use volunteers only: CONTRIBUTING
   forbids learners' recordings anywhere. An informal test on one Spanish-accented voice already showed a
   spoken *v* written as *b*.
3. **False acceptance.** Do not put `AcceptedAnswers` into the transcription prompt. A model told what to
   expect tends to hear it, and a wrong spoken answer comes back as the right one, telling a learner they are
   ready when they are not. If a later design wants that hint, measure first how many wrong answers it turns
   into accepted ones. This is not hypothetical: in a test on 2026-09-12, a hint naming one word in the
   transcription prompt turned both control clips that said a similar-sounding word into the hinted word (2 of 2,
   synthetic voices).
4. **Nothing leaves the device.** Confirm that the runtime library itself sends no telemetry
   (LOCAL_VS_CLOUD rule 4).

## Consequences

- **Microphone permissions:** Android needs `RECORD_AUDIO`, and iOS and Mac Catalyst need
  `NSMicrophoneUsageDescription`. Neither is declared today.
- **A model store:** the model needs its own store. `AppDataAudioPackStore` reads each file whole into memory
  under a 10-minute timeout, which cannot fetch 2.6 GB. The store must stream to disk, resume, compute the
  SHA-256 while streaming, and check free space and device memory before it starts. Where the model is hosted
  is still open: size and bandwidth decide it. The audio packs live on peopleworksservices.com.
- **License:** the model weights keep their Apache 2.0 license, and its notice ships with the model pack.
- **CI:** CI does not build `Citiz.Hybrid` today, so the provider is verified only locally until it does.
- **If rejected:** ROADMAP 0.6 goes back to open. A cloud provider stays second and opt-in (ADR-0003 §4).
