# ADR-0003: The browser is the primary host; the server is optional

**Status:** Accepted · **Date:** 2026-08-25 · **Supersedes:** design document §10.1–10.2 (server-first topology)

## Context

The design document (v0.4) proposed a server-first topology: ASP.NET Core API, PostgreSQL or Azure
SQL, Redis, Blob Storage, SignalR, .NET Aspire, with Blazor and MAUI clients in front. The same
document also set the principles that matter more: essential learning without an account, local
processing by default, self-hosting by anyone, no dependency on a specific cloud for basic functions,
and "how many people could learn something useful without unnecessary barriers" as the guiding metric.

Those principles are easier to keep — and cheaper to run for a community project — if the product
does not need a server at all for its essential use.

## Decision

1. **`Citiz.Web` is a standalone Blazor WebAssembly PWA.** Content and translations are static files
   published next to the app. Progress lives in the browser. The app works offline after the first
   visit (service worker) and is hosted as static files (GitHub Pages, nginx, any static host).
2. **`Citiz.Api` is optional** and holds no learner data. It exposes the same public content and the
   same evaluator over HTTP for integrations and organizations that want a shared instance.
3. **No relational database in the MVP.** The content repository is the database: JSON in git,
   validated in CI, with review history in commits. A database can appear later for the community
   features that genuinely need identity, without touching the engines.
4. **Cloud services, when they come (speech, AI), are providers behind interfaces** (ADR-0001),
   opt-in, and disclosed at the moment of use.

## Consequences

- Zero infrastructure cost to run the official instance; a fork can be online in minutes.
- Content updates are a pull request and a deploy; no migration, no cache invalidation beyond the
  service worker's versioned cache.
- The hybrid apps (roadmap 0.7) reuse the same components and the same static content packaging.
- Features that need a server (sync, groups, dashboards) are explicitly *later* and explicitly
  optional; the design document's topology remains the reference for that phase.
- Anything in the design document that assumed a server for essential learning (onboarding stored
  server-side, server-resolved dynamic answers) is implemented client-side instead.
