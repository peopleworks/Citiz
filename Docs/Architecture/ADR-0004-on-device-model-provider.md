# ADR-0004: Spoken answers are transcribed on the device; a model is added where it earns its size

**Status:** Proposed · **Date:** 2026-09-12 · **Revised:** 2026-09-13, after review · **Relates to:**
[ROADMAP](../../ROADMAP.md) 0.5 and 0.6, [ADR-0001](ADR-0001-core-boundaries.md),
[ADR-0003](ADR-0003-local-first-client.md), [LOCAL_VS_CLOUD](../Privacy/LOCAL_VS_CLOUD.md)

## Context

The roadmap already asks for this, in two places: *"Speech-to-text for spoken answers, on-device where the
browser supports it; disclosed when not"* (0.5), and *"`ICitizAiService` providers: a local model (Foundry
Local / on-device) first, cloud second; both opt-in, both restricted to approved content"*, with
*"Ambiguous-answer evaluation as a second stage after the deterministic matcher"* (0.6).

The contract is ready for it. `AnswerEvaluationRequest` hands a provider the official `AcceptedAnswers` and
says it may only accept those; feedback comes back as a translation key, so no provider writes prose a learner
reads unreviewed; `AiExecutionClass.Local` exists. The only provider today is `NoAiFallbackService`. The
contract judges text, so a spoken answer needs a recogniser in front of it, the way `ISpeechService` sits
behind "Listen".

**A model now fits the job on a phone.** **Gemma 4** (Google, announced 2026-04-02) is released under
**Apache 2.0**. Its E2B size (2.3B effective parameters, 5.1B in total) takes audio natively — speech
recognition and speech translation, clips of up to 30 seconds at 16 kHz — and ships as a
[LiteRT-LM build](https://huggingface.co/litert-community/gemma-4-E2B-it-litert-lm) of 2,583 MB. Its model
card measures 1,733 MB of CPU memory on an S26 Ultra with the CPU backend (676 MB with the GPU backend) and
607 MB on an iPhone 17 Pro (1,450 MB with the GPU backend); those benchmarks are for text, and the card
publishes no audio figures for phones. It runs on the maintainer's Android phone in Google's AI Edge Gallery
app. Gemma 3n does the same job but is governed by the Gemma Terms of Use and its Prohibited Use Policy,
which an MIT project should not inherit when an Apache 2.0 model exists.

**The devices already recognise speech without a download from Citiz.** Android has
[`SpeechRecognizer.CreateOnDeviceSpeechRecognizer`](https://learn.microsoft.com/dotnet/api/android.speech.speechrecognizer.createondevicespeechrecognizer)
and [`IsOnDeviceRecognitionAvailable`](https://learn.microsoft.com/dotnet/api/android.speech.speechrecognizer.isondevicerecognitionavailable)
(API 31); iOS and Mac Catalyst have
[`SFSpeechRecognitionRequest.RequiresOnDeviceRecognition`](https://learn.microsoft.com/dotnet/api/speech.sfspeechrecognitionrequest.requiresondevicerecognition)
and [`SFSpeechRecognizer.SupportsOnDeviceRecognition`](https://learn.microsoft.com/dotnet/api/speech.sfspeechrecognizer.supportsondevicerecognition).
All four are bound in .NET, like the platform voices behind `AndroidSpeechService` and `AppleSpeechService`.
Apple's newer [`SpeechTranscriber`](https://developer.apple.com/documentation/speech/speechtranscriber)
(iOS 26) is declared in Swift only, so .NET would reach it through a small native wrapper. Google's
[ML Kit GenAI Speech Recognition](https://developers.google.com/ml-kit/genai/speech-recognition/android) is
on-device too, but alpha, and its better mode runs only on Pixel 10 and 11.

**The browser can too, though not everywhere.** The Web Speech API gained
[`processLocally`](https://developer.mozilla.org/docs/Web/API/SpeechRecognition/processLocally), with
`SpeechRecognition.available()` and `install()` for language packs; MDN marks it experimental. Without it,
[recognition in Chrome is server-based](https://developer.mozilla.org/docs/Web/API/SpeechRecognition): the
audio goes to a web service, which LOCAL_VS_CLOUD rule 4 forbids for spoken answers unless the learner
explicitly chose that service. The local mode is recent and uneven: for example,
[Chromium issue 444393111](https://issues.chromium.org/issues/444393111) reports `available()` broken on macOS.

**Size is a barrier.** Citiz's guiding metric is how many people can learn without unnecessary barriers
(ADR-0003). A 2.6 GB download and more than a gigabyte of memory are such a barrier on an inexpensive phone
or a limited data plan; a platform recogniser costs the learner no download from Citiz.

Routes that can transcribe a spoken answer, as of 2026-09-13:

| Route | What the learner downloads | License | Reach from .NET |
| --- | --- | --- | --- |
| Platform recogniser, on-device mode | nothing from Citiz; the system may fetch its own language data | the platform's | bound: Android API 31+, iOS, Mac Catalyst |
| Browser, `processLocally` | a language pack the browser installs | the browser's | JavaScript interop in the web app |
| [Whisper.net](https://github.com/sandrohanea/whisper.net) with a [whisper.cpp](https://github.com/ggml-org/whisper.cpp) model | tiny 75 MiB (about 273 MB of memory), base 142 MiB (388 MB), small 466 MiB (852 MB) | MIT | NuGet: Android, iOS, Mac Catalyst, WebAssembly |
| Gemma 4 E2B, LiteRT-LM | 2,583 MB | Apache 2.0 | a .NET for Android binding; iOS waits for LiteRT-LM's Swift API (below) |

The runtimes that can host a model like Gemma 4 inside an app, as of 2026-09-12:

| Runtime | Language APIs | Gemma 4 audio | Fit for Citiz |
| --- | --- | --- | --- |
| [LiteRT-LM](https://github.com/google-ai-edge/LiteRT-LM) (Apache 2.0) | Python, Kotlin and C++ stable; Swift and JavaScript early preview; Flutter community | yes; on 2026-09-12 its Python package (0.17.0) ran `gemma-4-E2B-it.litertlm`, byte-identical (SHA-256) to the file AI Edge Gallery 1.0.19 downloads on Android, on a Windows laptop CPU, taking 0.21–0.34 s per second of audio | Android through a .NET for Android binding; iOS waits for Swift. The desktop Python package can test prompts and accents before any app code exists |
| [ONNX Runtime GenAI](https://github.com/microsoft/onnxruntime-genai) | C#, C++, Python | the runtime supports it since v0.15.0 (2026-07-30), but no Gemma 4 model in its format was published as of 2026-09-12: the `onnx-community` exports follow the Transformers.js layout, so a model would have to be built from the full weights with its model builder | pure C#, once such a model exists; untested on a phone with audio |
| [llama.cpp](https://github.com/ggml-org/llama.cpp) | C/C++ | since April 2026 (`mtmd`) | native interop only |

## Decision (proposed)

1. **Transcription is a device service, not an `ICitizAiService` provider.** A recogniser turns a spoken
   answer into `AnswerEvaluationRequest.Response`, and the deterministic matcher judges it as it judges a
   typed one. It sits beside `ISpeechService`: implemented in `Citiz.Hybrid` for Android and Apple, and
   through the Web Speech API in the browser. It is opt-in and disclosed at the moment of use. ADR-0001 keeps
   SDKs out of the engines.
2. **On each platform, the lightest route that passes the gates wins.** In order of what a learner pays in
   download and memory: the platform's on-device recogniser, Whisper.net with a small model, Gemma 4 E2B. The
   spike runs every route a platform has on the same clips, and a heavier route is chosen only where it does
   clearly better on the accent and false-acceptance gates.
3. **No platform waits for another, and the browser is not last.** The browser is Citiz's primary host
   (ADR-0003). Where it can recognise locally (`processLocally`, checked with `available()`), the web app uses
   that. Where it cannot, spoken answers stay off, or are offered only as the explicit, disclosed choice that
   rule 4 requires. Audio is never sent anywhere silently.
4. **The second-stage judge is its own decision.** Choosing one of `AcceptedAnswers`, or none, when the
   matcher cannot settle a response needs data first: how often the matcher cannot settle real answers. It
   gets its own ADR when that data exists. Gemma 4 E2B is the leading candidate for it: Apache 2.0, on the
   device, and able to transcribe as well if its route wins here. It never produces text a learner reads.
5. **Nothing reaches learners until the gates below pass**, and the product stays whole without it: typing an
   answer always works, and `NoAiFallbackService` answers whenever a device cannot run a route or the learner
   declines.

## Gates before any learner sees it

1. **The spike.** On real devices: the maintainer's Android phone, an inexpensive Android phone with 4 GB of
   memory or less, and an iPhone. For every route, record the download size, load time, peak memory, seconds
   per answer, the device model and OS version, and the runtime version. For a model file, also record its
   SHA-256: the same file name has been published with different contents, and a repository commit alone
   does not say which one a device holds. For a platform recogniser, record that on-device mode was confirmed
   (`IsOnDeviceRecognitionAvailable`, `SupportsOnDeviceRecognition`) and that it still worked in airplane
   mode. If a model's first runtime route fails, try the other (LiteRT-LM through a binding, ONNX Runtime
   GenAI from C#). A platform where no route passes keeps spoken answers off.
2. **Accent.** Most learners are not native English speakers, and a recogniser that mishears an accented
   answer marks a right answer wrong. Measure the word error rate on official accepted answers spoken by
   volunteers with the accents of Citiz's help languages, per language and per route, on the same clips. Use
   volunteers only: CONTRIBUTING forbids learners' recordings anywhere. An informal test on one
   Spanish-accented voice already showed a spoken *v* written as *b*.
3. **False acceptance.** Do not put `AcceptedAnswers` into the transcription prompt. A model told what to
   expect tends to hear it, and a wrong spoken answer comes back as the right one, telling a learner they are
   ready when they are not. This is not hypothetical: in a test on 2026-09-12, a hint naming one word in the
   transcription prompt turned both control clips that said a similar-sounding word into the hinted word (2
   of 2, synthetic voices). The platforms offer the same hint under other names,
   [`contextualStrings`](https://learn.microsoft.com/dotnet/api/speech.sfspeechrecognitionrequest.contextualstrings)
   on Apple and [`EXTRA_BIASING_STRINGS`](https://learn.microsoft.com/dotnet/api/android.speech.recognizerintent.extrabiasingstrings)
   on Android (API 33), and the rule holds there too. If a later design wants a hint, or lets the matcher try
   a recogniser's alternative transcriptions after the first, measure first how many wrong answers that turns
   into accepted ones.
4. **Nothing leaves the device.** Confirm that the runtime library itself sends no telemetry (LOCAL_VS_CLOUD
   rule 4). For platform and browser recognisers, the airplane-mode run in gate 1 is the check; in the
   browser, only `processLocally` counts as local.

## Consequences

- **Permissions:** Android needs `RECORD_AUDIO`; iOS and Mac Catalyst need `NSMicrophoneUsageDescription`,
  and `NSSpeechRecognitionUsageDescription` for Apple's recogniser. None is declared today. The browser asks
  for the microphone itself.
- **A model store, only if a model route wins:** `AppDataAudioPackStore` reads each file whole into memory
  under a 10-minute timeout, which cannot fetch 2.6 GB. The store must stream to disk, resume, compute the
  SHA-256 while streaming, and check free space and device memory before it starts.
- **Hosting a model:** 2.6 GB per learner is heavy for peopleworksservices.com, where the audio packs live. A
  third-party host such as Hugging Face sees every download, so it would be named in LOCAL_VS_CLOUD before the
  first one.
- **Licenses:** Gemma weights keep their Apache 2.0 license, and its notice ships with the model pack.
  Whisper.net, whisper.cpp and the Whisper models are MIT.
- **CI:** CI does not build `Citiz.Hybrid` today, so the native routes are verified only locally until it
  does.
- **If rejected:** spoken answers stay off and typing keeps working. The second-stage judge still gets its
  own ADR, and a cloud provider stays second and opt-in (ADR-0003 §4).

## History

- 2026-09-12: proposed, with Gemma 4 E2B as the first provider for both jobs, Android first.
- 2026-09-13: revised after review. The platforms' on-device recognisers, Whisper.net and the browser's local
  mode became candidates beside Gemma 4 E2B; an inexpensive phone and an airplane-mode check joined the
  spike; the platforms' hint lists joined the false-acceptance gate; the second-stage judge moved to its own
  decision.
