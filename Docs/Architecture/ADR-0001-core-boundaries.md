# ADR-0001: The engines do not depend on hosts, vendors or infrastructure

**Status:** Accepted · **Date:** 2026-07-28 (design document), reaffirmed 2026-08-25

## Context

The design document describes a product that must outlive any particular UI framework, cloud, database
or AI model: exam versions change, providers change, the team changes. It also describes several hosts
for the same logic — browser, hybrid app, CLI, API, worker, and later an MCP server.

## Decision

`Citiz.Core`, `Citiz.Content`, `Citiz.Learning`, `Citiz.Discovery`, `Citiz.Games`,
`Citiz.Localization` and `Citiz.AI` reference only each other and the base class library. They do not
reference Blazor, MAUI, ASP.NET Core, Entity Framework, any Azure or cloud SDK, or any AI vendor SDK.

Where an engine needs something from the outside world, it declares an interface and a host
implements it:

| Need | Interface | Implementations |
| --- | --- | --- |
| Read content files | `IContentStore` | `FileContentStore` (disk), `HttpContentStore` (browser) |
| Load a language pack | `ITranslationCatalogLoader` | `HttpTranslationCatalogLoader`; the CLI reads files directly |
| Judge an answer with help | `ICitizAiService` | `NoAiFallbackService`; future local and cloud providers |
| Persist progress | none — `ProgressSnapshot` is a value the host stores wherever it likes | `localStorage` in the browser |

Serialization in the engines uses source generation so the browser build stays trim-safe without the
engines knowing they run in a browser.

## Consequences

- The same `ExamSession` runs in the browser, the terminal and the API, and is tested once.
- Adding a host is adding implementations of the interfaces above, never editing an engine.
- An engine cannot make a network call, which is what makes the privacy promise in
  [Docs/Privacy/LOCAL_VS_CLOUD.md](../Privacy/LOCAL_VS_CLOUD.md) checkable by reading project references.
- Test projects reference engines only; a test that needs a browser is a sign the logic is in the
  wrong project.
