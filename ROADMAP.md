# Roadmap

The founding vision is the [design document](Docs/Design/Citiz-Design-Document-v0.4.es.md) (Spanish).
This file is the short, current list of what comes next, in order. Dates are deliberately absent:
this is a community project, and each item ships when it is right.

## 0.3 — Professional foundation

Both official banks, versioned rules, dynamic answers, official vocabulary, capsules; a deterministic
exam session and answer matcher; a Blazor WebAssembly PWA with five practice modes, dictation, discovery
and one game; seven interface languages; the `citiz` tool; CI, CodeQL, Pages; the documents in this
folder.

## 0.4 — Verified content *(this release)*

The release where the "not yet verified" labels disappeared — by verification, not by hiding them.

- [x] Compare the 2008 bank line by line with the official USCIS document; mark approved
- [x] Compare the 2025 bank with the official 2025 document (M-1778); 13 questions corrected; mark approved
- [x] Record the 2025 65/20 question list and enable the mode
- [x] Re-verify dynamic answers (officeholders) and vocabulary lists; record `verifiedOn`
- [x] Fact-check the twelve discovery capsules against their sources; mark approved
- [x] A reproducible comparison tool (`tools/content-verify/`) and a dated verification log
- [x] Enable GitHub Pages and put the live link in the README (https://peopleworks.github.io/Citiz/)
- [x] Spanish pack reviewed by a second fluent speaker → `Reviewed`
- [ ] First `good first issue` batch: one capsule per state, capsule review, language review

## 0.5 — Listen and speak

- [x] Official audio: USCIS's own recordings of the 2008 questions, downloaded once as a pack (public
  domain); a synthetic "Citiz voice" pack for the 2025 test and the vocabulary, generated once by the
  maintainer and labelled as such; the device voice as the fallback everywhere. Built and verified
  end to end on 2026-09-01; rolling out in this order:
  - [x] Official pack uploaded to `https://peopleworksservices.com/citiz-audio/uscis-2008/v1/` and
    verified byte for byte against the catalog
  - [x] CORS on the host: the `<location path="citiz-audio">` block in the site root `web.config`
    (`tools/audio/README.md`), verified with curl on 2026-09-13; `manifest.json` re-uploaded with
    the real `baseUrl` and verified byte for byte; on the live site a browser downloaded all 100
    clips (18 s) and played one
  - [x] ElevenLabs key stored on the Mac (`tools/audio/set-elevenlabs-key.sh`), restricted to Text
    to Speech and Voices; six voices sampled on 2026-09-13 (`tools/audio/dist/samples/index.html`)
  - [x] A voice chosen by ear (Sarah), then `--set 2025` (533 clips) and `--set words` (98) generated.
    A local Whisper model transcribed every clip with no hint of its text (`tools/audio/check_clips.py`)
    and caught numbers read twice, a mispronounced name and seven clips cut off at the end, all
    regenerated. Uploaded on 2026-09-13 and verified byte for byte; both packs download and play in
    the app
  - [x] One-time offers where each pack helps, so learners find the voice without opening Settings:
    on Prepare the official recordings of their test first, then the Citiz voice for it; on
    Communicate the Citiz voice for the vocabulary. One pack per visit, each labelled like its clips;
    checked in a browser on 2026-09-13 for a 2025 and a 2008 learner
  - [ ] Listen to the clips the check lists and to its random sample, set the packs `approved`, and
    commit `content/audio/packs.json` (not before: the app offers every pack listed there), then push
  - [x] The 2008 questions in the Citiz voice: `--set 2008` (100 prompts, 2.5 MB) generated on
    2026-09-13; the clip check lists none of them
  - [ ] Upload `citiz-voice-2008/v1` and verify it byte for byte, like the other packs
- [ ] Speech-to-text for spoken answers, on-device where the browser supports it; disclosed when not
  ([ADR-0004](Docs/Architecture/ADR-0004-on-device-model-provider.md), proposed)
- [ ] Interview simulation: greeting, N-400 vocabulary questions, reading, writing, civics — the full
  sequence, scored per skill
- [ ] Study plan: a daily "next thing" from the ledger (due reviews, weakest areas, a capsule)

## 0.6 — Explain

- [ ] `ICitizAiService` providers: a local model (Foundry Local / on-device) first, cloud second; both
  opt-in, both restricted to approved content ([design §12](Docs/Design/Citiz-Design-Document-v0.4.es.md))
- [ ] Explanations and mnemonics generated only from approved content, labelled as such
- [ ] Ambiguous-answer evaluation as a second stage after the deterministic matcher
  (its own ADR, once there is data on how often the matcher cannot settle; see ADR-0004)

## 0.7 — Everywhere

- [x] `Citiz.Hybrid`: .NET MAUI Blazor Hybrid host sharing the Razor components through
  `Citiz.SharedUI`; runs on iOS, Android and Windows with each platform's own speech and the audio
  packs ([`src/Citiz.Hybrid`](src/Citiz.Hybrid/README.md))
- [ ] **Citiz on Google Play and the App Store**, through PeopleWorks organization accounts on both
  stores. Next once the Citiz voice packs are approved (maintainer's decision, 2026-09-13). Checked
  on 2026-09-13:
  - [x] Organization accounts, so the closed test with 12 testers for 14 days that Google Play
    requires of new personal accounts does not apply
    ([Play Console Help](https://support.google.com/googleplay/android-developer/answer/14151465))
  - [x] The Android build targets API 36, which Google Play requires of new apps and updates since
    2026-08-31 ([target API level](https://support.google.com/googleplay/android-developer/answer/11926878))
  - [x] Xcode 26.6 on the maintainer's Mac: App Store Connect accepts only builds made with Xcode 26
    and the iOS 26 SDK since 2026-04-28 ([Apple](https://developer.apple.com/news/upcoming-requirements/))
  - [ ] A Citiz app icon and splash screen; the app still shows the .NET MAUI template's purple
  - [ ] Android: a signed release App Bundle with Play App Signing; decide who keeps the upload key,
    and where
  - [ ] iOS: `PrivacyInfo.xcprivacy` with the three required-reason API categories every .NET app
    uses, file timestamp (C617.1), system boot time (35F9.1) and disk space (E174.1)
    ([Microsoft Learn](https://learn.microsoft.com/dotnet/maui/ios/privacy-manifest));
    `ITSAppUsesNonExemptEncryption` set to false (HTTPS only); a distribution certificate and profile
  - [ ] Store listings in the interface languages: screenshots of the app on phones, and a
    description that names the USCIS sources and says Citiz is not affiliated with any government
    ([Google Play](https://support.google.com/googleplay/android-developer/answer/9514050); Apple's
    [App Review Guidelines](https://developer.apple.com/app-store/review/guidelines/) on misleading
    metadata); the privacy policy link ([PRIVACY.md](PRIVACY.md))
  - [ ] Privacy declarations: Google Play's Data safety form and Apple's App Privacy details. Citiz
    sends nothing about the learner; the audio host sees a download, so decide how its server logs
    are declared. The content and age rating questionnaires
  - [ ] CI builds `Citiz.Hybrid` for Android and iOS, so any store build can be reproduced
  - [ ] Internal testing on Google Play and TestFlight with volunteers, then production; store
    badges in the README
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
