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
3. ⏳ **Host interfaces are half-implemented.** `IContentStore` and `ITranslationCatalogLoader`
   were already interfaces (`Citiz.Web`'s `HttpContentStore`/`HttpTranslationCatalogLoader` are one
   implementation), and `ISpeechService`/`IFileExporter` were carved out the same way — but
   `MauiProgram.cs` currently registers the **same HTTP-based implementations Citiz.Web uses**,
   which have nothing to fetch: no `content/` or `i18n/` files are bundled into this host's package
   yet. Content and language loading are expected to fail until these are replaced:
   - A file-system-backed `IContentStore` and `ITranslationCatalogLoader`, bundling the repository's
     `content/` and `Citiz.Web/wwwroot/i18n/` files as `MauiAsset` items (or an MSBuild copy step
     into `Resources/Raw/`), read back with `FileSystem.OpenAppPackageFileAsync`.
   - `ISpeechService`: the current `WebSpeechService` (Web Speech API via JS interop) is registered
     as-is and may simply work inside a `BlazorWebView` — untested on a real device. If it doesn't,
     write a `NativeSpeechService` using platform text-to-speech.
   - `IFileExporter`: `BrowserFileExporter`'s `<a download>` trick doesn't exist natively; needs a
     `CommunityToolkit.Maui.Storage`-based implementation for "Download my progress" in Settings.
4. ⏳ **Not yet built for a real device or emulator** — only smoke-tested that it compiles and boots
   (see below). No app icon/splash beyond the template defaults, no store provisioning, no CI job.

## Try it

```bash
dotnet build Citiz.Hybrid.slnx -f net10.0-windows10.0.19041.0
dotnet run --project src/Citiz.Hybrid -f net10.0-windows10.0.19041.0
```

Swap the `-f` target for `net10.0-android` / `net10.0-ios` / `net10.0-maccatalyst` once you have the
matching emulator or device set up (`dotnet workload list` should show the platform installed).

Nothing in the engines changes for any of this ([ADR-0001](../../Docs/Architecture/ADR-0001-core-boundaries.md)).
