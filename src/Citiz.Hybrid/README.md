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
6. ⏳ **Still open:**
   - `ISpeechService`: the current `WebSpeechService` (Web Speech API via JS interop) is registered
     as-is and may simply work inside a `BlazorWebView` — untested on a real device. If it doesn't,
     write a `NativeSpeechService` using platform text-to-speech.
   - Not yet built for a real device or emulator — only smoke-tested that it compiles and boots on
     Windows desktop (see below). No app icon/splash beyond the template defaults, no store
     provisioning, no CI job.

## Try it

```bash
dotnet build Citiz.Hybrid.slnx -f net10.0-windows10.0.19041.0
dotnet run --project src/Citiz.Hybrid -f net10.0-windows10.0.19041.0
```

Swap the `-f` target for `net10.0-android` / `net10.0-ios` / `net10.0-maccatalyst` once you have the
matching emulator or device set up (`dotnet workload list` should show the platform installed).

Nothing in the engines changes for any of this ([ADR-0001](../../Docs/Architecture/ADR-0001-core-boundaries.md)).
