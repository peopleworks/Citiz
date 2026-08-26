# Security Policy

## Reporting a vulnerability

Use GitHub's private reporting:
**[Report a vulnerability](https://github.com/peopleworks/Citiz/security/advisories/new)**
(Security → Advisories → Report a vulnerability). It stays private until we publish it together.

Please do not open a public issue for a vulnerability. If GitHub advisories do not work for you,
email `peopleworks@gmail.com` with `SECURITY` in the subject.

Expect a first reply within a week. This is a community project maintained alongside a day job, so
a fix may take longer than an acknowledgement; you will hear where it stands either way. If you would
like credit in the advisory, say so; if you would rather stay anonymous, that is fine too.

## Supported versions

The deployed web app and the latest release. There are no long-term support branches.

## What the attack surface looks like

Most of Citiz has no server to attack, which changes what "vulnerability" means here.

| Component | Where it runs | Notes |
| --- | --- | --- |
| Web app (`Citiz.Web`) | Your browser, WebAssembly | Static hosting, no backend, no accounts, no cookies, no analytics. Progress is in `localStorage`. |
| `Citiz.Cli`, `Citiz.ContentWorker`, the libraries | Your machine | Local processing. The worker fetches public official pages and hashes them. |
| `Citiz.Api` | A server, optional | Serves the same public content over HTTP. Holds no learner data. |

We are especially interested in reports about:

- **Anything that causes learner data to leave the device from the web app.** The central promise
  of the project is that it does not; a bug there is the most serious thing you could find.
- **Content integrity.** A way to make the app show an answer that is not in the content files, or
  to bypass the review-status label, matters here: people study from this.
- **The service worker and offline cache**, if it could serve stale content after a correction was
  published without any way to refresh.
- **Anything in the optional API**: injection, resource exhaustion, path traversal into the content
  folder (the store rejects paths outside its root; tell us if it does not).
- **Supply chain.** Dependencies are pinned centrally in `Directory.Packages.props`, audited on every
  build (NuGet audit warnings are errors), and kept current by Dependabot.

## What is not a vulnerability

- **A wrong or outdated answer.** That is a content defect, and a serious one, but it is fixed through
  the [content correction template](https://github.com/peopleworks/Citiz/issues/new/choose), in the
  open, with the source.
- **Being able to see the answers before answering.** Citiz is a study tool, not a proctored test.
- **Browser voice services.** When you tap "Listen", your browser synthesizes speech; some browsers
  use a network voice. The interface says which. That is disclosed behaviour, not a leak.
