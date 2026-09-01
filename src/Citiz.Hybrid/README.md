# Citiz.Hybrid

The .NET MAUI Blazor Hybrid host for Android, iOS, macOS and Windows. Deliberately **not** in
`Citiz.slnx`: the MAUI workloads are large and platform-specific, and CI — and every contributor who
only wants the web app — should be able to build the rest of the repository with the .NET SDK alone.
Build and run it through [`Citiz.Hybrid.slnx`](../../Citiz.Hybrid.slnx) instead.

## Where it stands

1. ✅ **The UI moved out of `Citiz.Web`** into [`Citiz.SharedUI`](../Citiz.SharedUI), a Razor class
   library: every page, the layout, the components, and the application services (`LearnerState`,
   `StudyService`, storage, speech). `Citiz.Web` is now a thin browser bootstrapper; this project is
   the second one.
2. ✅ **Scaffolded** with `dotnet new maui-blazor -n Citiz.Hybrid`, referencing `Citiz.SharedUI`.
   `MainPage.xaml` mounts `Citiz.SharedUI.App` directly — the same router, the same pages, no
   Hybrid-specific UI at all.
3. ✅ **Content and translations load from the app package.** `AppPackageContentStore` and
   `AppPackageTranslationCatalogLoader` (`Services/`) read `content/**` and `i18n/*.json`, bundled as
   `MauiAsset` items straight from the repository's `content/` and `Citiz.Web/wwwroot/i18n/`, via
   `FileSystem.OpenAppPackageFileAsync` — no HTTP server behind a `BlazorWebView` to fetch them from.
   Verified with repeated clean launches on Windows: real translated text and content, no stale
   `[key]` fallbacks.
4. ✅ **First-launch initialization is reliable.** `MainLayout` defers anything JS-interop-dependent
   (`LearnerState.InitializeAsync()`, which touches `BrowserStorage`) to `OnAfterRenderAsync`, since
   `BlazorWebView` only attaches its JS bridge after the first render commits — calling it from
   `OnInitializedAsync` would run before that bridge exists. A related, easy-to-misdiagnose bug lived
   one layer down: `LocalizationService` used `.ConfigureAwait(false)` on the path to its `Changed`
   event, so `StateHasChanged()` fired off the render dispatcher's thread and threw silently in
   `BlazorWebView`'s real multi-threaded dispatcher (invisible in WASM, which is single-threaded) —
   fixed by dropping `.ConfigureAwait(false)` from that call path.
5. ✅ **`IFileExporter` (Settings → "Download my progress") has a native implementation.**
   `Services/MauiFileExporter.cs` wraps `CommunityToolkit.Maui.Storage`'s `IFileSaver`, registered for
   Android/iOS/MacCatalyst. Windows is a deliberate exception: `Platforms/Windows/WindowsFileExporter.cs`
   calls the Windows App SDK 1.8+ pickers (`Microsoft.Windows.Storage.Pickers`) directly, because
   CommunityToolkit.Maui.Storage 13.0.0's own Windows `FileSaver` throws a `COMException` in this
   unpackaged app every time (confirmed live) — its classic WinRT `Windows.Storage.Pickers` picker
   needs package identity that an unpackaged (`WindowsPackageType=None`) app doesn't have, no matter
   how correctly its owner window is initialized. A later CommunityToolkit.Maui release (14.2.2+)
   fixes this the same way internally, but requires a newer `Microsoft.Maui.Controls` than this repo's
   MAUI workload currently provides (`$(MauiVersion)` = 10.0.20; that release needs ≥ 10.0.30) — worth
   revisiting once the workload updates, to drop the Windows-specific file. Verified end-to-end on
   Windows: the native "Save As" dialog opens with the right filename/type/location, and the picked
   file is written with the exact content passed in.
6. ✅ **`ISpeechService` works as-is on Windows.** `WebSpeechService` (Web Speech API via JS interop)
   needed no changes: `window.speechSynthesis` is present, 5 voices are available immediately (no
   `onvoiceschanged` delay to work around), `IsAvailableAsync()`/`IsLocalVoiceAsync()` both return
   `true` (an on-device voice, not a network one — required by `Docs/Privacy/LOCAL_VS_CLOUD.md`), and
   a real `SpeakAsync()` call set `speechSynthesis.speaking` immediately and kept it `true` while the
   utterance played. WebView2 is Chromium, so this isn't too surprising — but it was unverified until
   checked live, the same way as everything else on this list.
7. ✅ **iOS: builds and runs on the simulator.** From a Mac with Xcode 26.6 and the `maui` workload,
   `dotnet build src/Citiz.Hybrid -f net10.0-ios` produced `Citiz.Hybrid.app` (iossimulator-arm64) on
   the first try; installed and launched on an iPhone 17 simulator (iOS 26.5), the app shows real
   translated text and content from the app package, the bottom tab bar and the safe-area padding,
   and the system log has no managed exceptions. Verified 2026-09-01 with `xcrun simctl`.
8. ✅ **Android: builds, runs and speaks on the emulator.** Same Mac, Android 16 (API 36) arm64
   emulator, deployed with the `Install` target (see below for why not `adb install`). Content and
   translations load from the package; the tab bar works; native text-to-speech reads words and
   questions aloud through Google TTS with an on-device voice (`en-us-x-tpf-local`, confirmed in
   logcat); "Download my progress" opens the system *Save* picker and writes `citiz-progress.json`
   where the learner chooses (confirmed by reading the file back). Verified 2026-09-01.
9. ✅ **Speech is native on Android and iOS, with the best installed voice.** Android's system
   WebView exposes no speech voices at all — with the browser implementation the Communicate page
   correctly said "Your browser cannot read text aloud" — and MAUI's own `TextToSpeech` can pick a
   language but not a voice, and hands the rate straight to Apple's 0–1 scale. So the hosts speak
   through the platform engines directly: `Services/AndroidSpeechService.cs`
   (`Android.Speech.Tts.TextToSpeech`, best local voice by quality, the engine's default among
   equals, never a network voice) and `Services/AppleSpeechService.cs` (`AVSpeechSynthesizer`, best
   voice by quality — Premium > Enhanced > Default — falling back to the system voice, never a
   novelty one; rate mapped to Apple's scale). Windows keeps WebView2's verified Web Speech voices.
   Both log the voices they see and the one they choose (`adb logcat -s Citiz:*`,
   `xcrun simctl launch --console-pty`). The "What runs where" table in Settings asks the service
   whether the voice is local instead of hard-coding "your browser's voice service".

   **Why it still sounds like a computer on the simulators:** they only ship the compact voices.
   The iPhone 17 simulator lists 68 voices, every en-US one "Default" quality (Samantha plus the
   novelty set); the Android 16 emulator has nine Google en-US voices, all the same "High" tier.
   On a real iPhone, download an Enhanced or Premium voice (Settings → Accessibility → Spoken
   Content → Voices → English → e.g. Ava, Zoe, Evan) and Citiz picks it up on the next launch; on a
   real Android phone, Settings → System → Languages → Text-to-speech output → Google → Install
   voice data does the same. A switch of library would not change this: every on-device TTS library
   ends in the same two engines. The genuinely better voice for the questions is USCIS's own
   recorded audio (public domain, 100 tracks for the 2008 test, roughly 25 MB) — roadmap 0.5.
10. ✅ **Safe areas on Android.** Android 15+ draws edge to edge and gives the WebView no
    `env(safe-area-inset-*)`, so the status bar sat on Citiz's top bar and the gesture bar on the tab
    bar. `MainPage.xaml` sets `SafeAreaEdges="All"` on the page; iOS was already inset by default.
11. ⏳ **Still open:**
    - On iOS, speech was exercised by hand on the simulator (it spoke, at first far too fast — the
      rate mapping in `AppleSpeechService` fixed that). `IFileExporter` on iOS still has not been
      exercised — it needs a tap inside the WebView, which `simctl` cannot script; the file saver is
      CommunityToolkit's, so the risk is low but the proof is missing. macOS (Mac Catalyst) is
      untested, including `AppleSpeechService`'s novelty-voice guard, which matters most there.
    - When testing sound on the Android emulator, start it *without* `-no-audio` — the first session
      here did, and "nothing plays" was the flag, not the Mac.
    - No app icon/splash beyond the template defaults, no store provisioning, no CI job for
      `Citiz.Hybrid.slnx`.

## Try it

```bash
dotnet build Citiz.Hybrid.slnx -f net10.0-windows10.0.19041.0
dotnet run --project src/Citiz.Hybrid -f net10.0-windows10.0.19041.0
```

Swap the `-f` target for `net10.0-android` / `net10.0-ios` / `net10.0-maccatalyst` once you have the
matching emulator or device set up (`dotnet workload list` should show the platform installed).

On a Mac (Xcode installed, `dotnet workload install maui` run once):

```bash
dotnet build src/Citiz.Hybrid -f net10.0-ios                        # -> bin/Debug/net10.0-ios/iossimulator-arm64/Citiz.Hybrid.app
xcrun simctl list devices available | grep iPhone                    # pick a simulator UDID
xcrun simctl boot <UDID> && open -a Simulator
xcrun simctl install booted src/Citiz.Hybrid/bin/Debug/net10.0-ios/iossimulator-arm64/Citiz.Hybrid.app
xcrun simctl launch booted com.peopleworks.citiz
xcrun simctl io booted screenshot citiz-ios.png                      # proof for the pull request
```

`dotnet build -t:Run -f net10.0-ios -p:_DeviceName=:v2:udid=<UDID>` does the install-and-launch in one
step.

Android on the same Mac (no Android Studio needed):

```bash
brew install openjdk@17 android-commandlinetools               # JDK 17 and sdkmanager/avdmanager
export JAVA_HOME=/opt/homebrew/opt/openjdk@17/libexec/openjdk.jdk/Contents/Home
export ANDROID_HOME=$HOME/Library/Android/sdk
yes | sdkmanager --sdk_root=$ANDROID_HOME "cmdline-tools;latest" "platform-tools" && yes | sdkmanager --sdk_root=$ANDROID_HOME --licenses
dotnet build src/Citiz.Hybrid -f net10.0-android -t:InstallAndroidDependencies \
  -p:AndroidSdkDirectory=$ANDROID_HOME -p:JavaSdkDirectory=$JAVA_HOME -p:AcceptAndroidSDKLicenses=True
yes | sdkmanager --sdk_root=$ANDROID_HOME "emulator" "system-images;android-36;google_apis;arm64-v8a"
$ANDROID_HOME/cmdline-tools/latest/bin/avdmanager create avd -n citiz -k "system-images;android-36;google_apis;arm64-v8a" -d pixel_7
$ANDROID_HOME/emulator/emulator -avd citiz &                   # boots in a window; `adb devices` shows emulator-5554
dotnet build src/Citiz.Hybrid -f net10.0-android -t:Install -p:AndroidSdkDirectory=$ANDROID_HOME -p:JavaSdkDirectory=$JAVA_HOME
$ANDROID_HOME/platform-tools/adb shell am start -n com.peopleworks.citiz/crc64dee97667ff9d55d1.MainActivity
$ANDROID_HOME/platform-tools/adb exec-out screencap -p > citiz-android.png
```

Two things that cost an hour if you do not know them: use the SDK's own `avdmanager`, not
Homebrew's wrapper (the wrapper looks in `/opt/homebrew/share/android-commandlinetools` and will not
see the system image); and deploy Debug builds with the `Install` target, never `adb install` of the
APK — a Debug APK relies on *Fast Deployment*, which pushes the assemblies separately, so an
`adb install`ed one aborts on launch with "No assemblies found … Assuming this is part of Fast
Deployment". (`-p:EmbedAssembliesIntoApk=true`, or a Release build, produces a self-contained APK.) The web app can be checked in the same simulator's Safari against `dotnet run --project
src/Citiz.Web --urls http://localhost:5050` (port 5000 is taken by macOS AirPlay Receiver, which
answers 403).

Nothing in the engines changes for any of this ([ADR-0001](../../Docs/Architecture/ADR-0001-core-boundaries.md)).
