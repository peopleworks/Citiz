# Citiz.Hybrid

The .NET MAUI Blazor Hybrid host for Android, iOS, Windows and macOS is on the roadmap (0.7). It is
not in the solution yet, deliberately: the MAUI workloads are large and platform-specific, and every
contributor should be able to build the repository with the .NET SDK alone.

When it lands, the plan is:

1. Carve the pages, components and services out of `src/Citiz.Web` into a `Citiz.UI` Razor class
   library, leaving `Citiz.Web` as the browser host (index.html, icons, service worker).
2. Create the MAUI host with `dotnet new maui-blazor -n Citiz.Hybrid`, reference `Citiz.UI`, and
   implement the same host interfaces the web host does (`IContentStore` over bundled files,
   `ITranslationCatalogLoader`, storage, speech).
3. Keep it in its own solution file (`Citiz.Hybrid.slnx`) with its own CI job on the platforms it
   targets, the way SignsofAI keeps its desktop app separate.

Nothing in the engines changes for this ([ADR-0001](../../Docs/Architecture/ADR-0001-core-boundaries.md)).
