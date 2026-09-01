# Build History

A running account of how Citiz actually got built — the decisions, the real bugs, the reasoning
behind each one, written close to when it happened. Two audiences read this file: contributors who
want the "why" behind a design choice, and Pedro, mining it for the article/video series that shares
Citiz and teaches how it was built (see [`ROADMAP.md`](../ROADMAP.md) → "Tell the story").

Each entry: what shipped, the story worth telling, and — since this doubles as raw material for that
series — a one-line **content angle** naming the teachable hook.

Entries are grouped by day, in the order things actually happened that day.

---

## 2026-08-25 — The v0.3 rebuild

The professional foundation: both official civics banks, versioned exam rules, dynamic answers,
official vocabulary, discovery capsules; a deterministic exam session and answer matcher; a Blazor
WebAssembly PWA with five practice modes, dictation, discovery and one game; seven interface
languages; the `citiz` CLI tool; CI, CodeQL, GitHub Pages.

Two gotchas worth remembering, both still documented in `Components/LocalizedComponentBase.cs` and
`Citiz.Web.csproj`:

- **Content files linked from outside `wwwroot` served an empty body in the dev server** — `dotnet
  run` resolved a path that doesn't exist because the linked item's `ContentRoot` stayed `wwwroot`,
  even though `publish` worked fine. Fixed with a static-web-assets target after
  `ResolveProjectStaticWebAssets`.
- **Blazor doesn't re-render a child whose parameters didn't change** — a language switch left pages
  and badges in the old language, because nothing told them to re-render. `LocalizedComponentBase`
  subscribes every localized page/component to `LocalizationService.Changed`.

**Content angle:** two classic "it works until it doesn't" Blazor footguns — good explainer material
for anyone shipping a multilingual Blazor app.

---

## 2026-08-26 — The visual redesign

Citiz's UI still read as a prototype: emoji instead of real icons, no sense of progress, no visual
identity. A Copilot-authored prototype (React/Tailwind) existed as a reference. Rather than adopt it
wholesale, reviewed it for good ideas, then built a mockup with Claude's design canvas and got it
approved *before* touching any Razor or CSS — established as the standing workflow for UI work.

Shipped: a sidebar (desktop) / bottom-nav (mobile) layout, real SVG icons replacing every emoji, a
"Citiz passport" progress card fed by real data (not placeholders), a real streak and weekly-activity
grid computed from the learner's actual practice history, and a home page practice widget.

**Content angle:** "reviewing an AI-generated prototype without copying it wholesale" — the
mockup-first workflow itself is a reusable process worth explaining, not just the result.

---

## 2026-08-26 — Starting the mobile app: `Citiz.Hybrid`

The plan: a .NET MAUI Blazor Hybrid host reusing the same pattern already proven in a separate
project, `PeopleWorksMeeting` — a shared Razor Class Library between a web host and a native one.

1. Extracted `Citiz.SharedUI` out of `Citiz.Web`: every page, layout, component and app service
   moved there. `Citiz.Web` became a thin browser bootstrapper.
2. Hardened the platform seams `ISpeechService` and `IFileExporter` — the two things a native host
   can't do the browser's way (no `<a download>`, inconsistent Web Speech support across WebViews).
   `IContentStore`/`ITranslationCatalogLoader` were already interfaces from the 2026-08-25 build —
   they turned out to be exactly right for this, no rework needed.
3. Scaffolded `Citiz.Hybrid`, deliberately kept **out** of `Citiz.slnx` and CI — it lives in its own
   `Citiz.Hybrid.slnx`, so contributors who only want the web app never need the MAUI workloads.

**Content angle:** "designing for a second host before you build it" — the seams (interfaces) that
made this extraction painless were written a day earlier, for a web-only app, without knowing Hybrid
was coming. Good example of interfaces paying for themselves later.

---

## 2026-08-26 — The bug that looked like something else entirely

`Citiz.Hybrid` booted, rendered the sidebar and icons correctly — and then got stuck: every
translation showed as literal `[app.name]`-style bracketed keys, content never loaded, the app sat
on the loading spinner forever.

The obvious suspect: JS interop timing. `BlazorWebView` only attaches its JavaScript bridge after
the first render commits, unlike WASM where it's available immediately — so a first hypothesis (a
real one, and the eventual fix for a *related* issue) was that `LearnerState.InitializeAsync()` was
running too early, before the bridge existed. Moving it to `OnAfterRenderAsync` didn't fully fix it.
Neither did a retry-with-timeout wrapper around every JS call. A raw, argument-free JS ping succeeded
instantly; the exact same call routed through the app's own services still hung. Confusing, because
every individual piece — tested in isolation — worked.

The real bug was one layer down, in code that looks nothing like JS interop: `LocalizationService`
awaited its catalog-loading calls with `.ConfigureAwait(false)`. In Blazor WebAssembly this is
invisible, because WASM is single-threaded — `ConfigureAwait(false)` never actually changes which
thread you're on there. In `BlazorWebView`'s real multi-threaded renderer, though, the continuation
after that await (including the `Changed?.Invoke()` that calls `StateHasChanged()`) resumed on a
thread-pool thread instead of the render dispatcher's thread — and `StateHasChanged()` off that
thread throws `InvalidOperationException`. Nothing was catching it, so Blazor's renderer swallowed
the exception silently: `IsInitialized` never flipped to `true`, and the UI just... waited, forever,
looking exactly like a hang.

Found by adding a temporary `try`/`catch` around the suspect call and logging the real stack trace to
a file — which pointed straight at the `.ConfigureAwait(false)` line. Fix: remove it from every await
on the path to that event.

**Content angle:** the strongest story from today. "I chased the wrong bug for a while, and here's
how the debugging actually converged" is honest, useful, and specific — a real case study in why
`ConfigureAwait(false)` is a genuinely different decision in a UI-facing service versus a library.

---

## 2026-08-26 — A joke that became three features

Pedro, testing the freshly-fixed Hybrid app, joked that a hardcoded "Welcome Pedro" greeting would be
wrong for every other user — there was no way for anyone to tell Citiz their name. That became three
Settings additions, mockup-approved before implementation like the earlier redesign:

- **Profile**: an optional name, used only to personalize the Home greeting ("Hi, Pedro!" instead of
  the generic tagline) — nothing else changes if it's unset.
- **Appearance**: a Light/Dark/Automatic toggle that actually overrides the system preference (most
  apps only ever *follow* the system) — applied before Blazor even boots, via a synchronous inline
  script, so there's no flash of the wrong theme.
- **Interview date**: deliberately a separate field from the N-400 filing date. The filing date picks
  the exam version; a learner's actual interview date only powers a "days left" countdown on Home —
  two different questions that were being conflated by having only one date field.

Building the theme toggle surfaced a real, easy-to-miss Blazor bug: `aria-pressed="@(someBool)"`
renders as an HTML *boolean* attribute (present or absent) instead of the ARIA string `"true"`/
`"false"` the spec requires — Blazor special-cases any attribute bound to a C# `bool`, regardless of
what the attribute actually means. The CSS that highlights the active button never matched, and it
was only caught by checking `document.querySelector(...).getAttribute('aria-pressed')` in a real
browser. The fix, already used correctly elsewhere in the codebase: `expr ? "true" : "false"`.

**Content angle:** "a throwaway joke turned into a real feature in one sitting" is a nice concrete
demo of mockup → approve → ship, and the `aria-pressed` bug is a sharp, two-minute "did you know"
explainer.

---

## 2026-08-26 — When the docs say "just call `SaveAsync`" and it doesn't

Building the native `IFileExporter` (so "Download my progress" works outside a browser): added
`CommunityToolkit.Maui.Storage`, wired its `IFileSaver` in, called `SaveAsync`. On Windows, every
single call threw a `COMException` with no message.

The library's own Windows implementation initializes its file picker with
`Process.GetCurrentProcess().MainWindowHandle` — a well-known unreliable API for WinUI3 apps (it can
return an invalid handle). Wrote a corrected version using the proper way to get a window handle.
Still threw — this time silently, no message at all.

The real, deeper cause: the picker type itself (the classic WinRT `Windows.Storage.Pickers`) needs
package identity to work at all, and Citiz.Hybrid runs unpackaged (`WindowsPackageType=None`) — no
window handle, however correctly obtained, fixes that. Confirmed a newer release of the same library
fixes this by switching to a different, newer picker API (`Microsoft.Windows.Storage.Pickers`, from
Windows App SDK 1.8+) built specifically to also work in unpackaged apps — but that release needs a
newer MAUI version than this repo's workload currently has. Wrote a small Windows-specific file using
that same modern API directly, registered only for Windows via `#if WINDOWS`.

Verified for real, not just "no exception thrown": raced the call against a timeout to prove a picker
dialog actually opened (rather than failing before it could), screenshotted the real native "Save As"
dialog to confirm it looked right, and clicked through it to confirm the file landed on disk with the
exact content passed in.

**Content angle:** three fix attempts before the real one, each disproven with evidence, not
guesswork — a genuinely good "how do you actually debug native interop" case study, plus a small
reusable lesson: never trust "it didn't throw" as proof a native picker call worked.

---

## 2026-08-26 — Trying to automate clicks on a native app (and giving up, for a good reason)

While verifying the Settings features above inside the native `Citiz.Hybrid` window (not just the
browser), tried the obvious thing: screenshot the window, simulate mouse clicks at pixel coordinates.
Hit real, compounding trouble — DPI virtualization made screen coordinates from an unaware process
not match reality; fixing that made the window's own maximize/restore state behave strangely across
monitors; and Windows UI Automation, which *did* walk the native window chrome fine, found nothing
inside the WebView2 control at all — its accessibility tree wasn't exposed to an external client here.

Abandoned pixel-chasing and switched to a method that had already worked well earlier in the day:
call the exact same code path a button click would trigger, directly, from a temporary diagnostic in
the component's lifecycle — then verify the *actual effect* (the DOM attribute that really changed,
the file that really landed on disk) rather than the UI interaction itself. Faster, more precise, and
immune to DPI or window-manager quirks. (The one place pixel automation *did* work cleanly: the
native "Save As" dialog above — a real Win32/WinRT common dialog, not WebView2 content.)

**Content angle:** "when not to automate" — a short, honest post about picking the right verification
tool for the actual question being asked, instead of reaching for the most literal one.

---

## 2026-09-01 — Verified content: the 2025 bank was wrong in 13 places

The banks had shipped as `needs-review`, honestly labelled, and people were already using the live
site. Time to earn the "Verified" badge. First obstacle, unrelated to content: this Mac had .NET 6
through 9 but not 10, so the SDK went into `~/.dotnet` with the official install script (no sudo,
no system change), and `Citiz.slnx` built clean on the first try.

Verification was done as a diff, not a reading exercise. A small Python tool
(`tools/content-verify/`) downloads the official documents — uscis.gov answers 403 to anything
that does not look like a browser, so it sends a browser user agent — extracts the text (pdfplumber
for the PDFs, BeautifulSoup for the 2008 page, which is the current source: it has Juneteenth, the
2019 PDF does not) and compares every prompt, every answer in order, every section heading and every
asterisk with the JSON. Then every reported difference was read against the document by hand.

The 2008 bank matched. The 2025 bank did not: it had been transcribed from the 2020 test wording
with the 2025 changes applied from memory, and USCIS form M-1778 differs in 13 questions — some
tiny ("obey" vs "follow", "or to the people"), some not (question 97's prompt is a different
sentence; question 48's Cabinet list has "Secretary of War (Defense)" and six positions the 2020
list never had; question 68's answers are restructured). Nobody would have caught the small ones by
eye, and the small ones are exactly what a typed-answer checker trips on. The 65/20 list for 2025
was also missing entirely, so that practice mode had been disabled for everyone filing after
October 20, 2025 — recorded from the asterisks, enabled.

Two judgement calls the documents leave open, written down so the next verifier does not re-decide
them silently: the number of Supreme Court justices became a dynamic answer for 2008 (USCIS moved it
to its updates page, exactly like the officeholders), and the "[Also acceptable are New Jersey, …]"
remark on the Statue of Liberty became real accepted answers, because the matcher should accept
what the officer accepts.

Then the twelve discovery capsules, each fact against the page it cites. Most held. One did not: the
Grand Canyon capsule said its rocks are "among the oldest exposed rocks on Earth", and the National
Park Service FAQ answers that exact question with "No" — about two billion years, against four
billion for the oldest known. Several capsules cited a park's landing page, which says nothing; each
now cites the page that actually states the fact. Along the way the NPS Mississippi page turned out
to say "Gulf of America" now, so the capsule says both names.

Closed with a browser smoke test (Playwright against the local build): every page, no "Not yet
verified", no "Editorial draft", no untranslated `[key]`, and fresh screenshots for the README.

**Content angle:** "I trusted my transcription and the diff said no" — a concrete, slightly
embarrassing story about why verbatim official content needs a mechanical check, not a careful
reader; plus the smaller lesson that a source you cite has to be the page that says the thing.

---

## 2026-09-01 — First run on iOS, from a Mac that had never built Citiz

Two ways to test "mobile", and both earned their place. For the web app: the iPhone 17 simulator's
own Safari (real WebKit, real safe areas, `xcrun simctl openurl` and `simctl io screenshot` for
proof) plus Playwright's WebKit engine with the iPhone 15 device profile for anything that needs a
tap — every page, a multiple-choice answer, the Settings flow that resolves the 2025 test and shows
the 65/20 checkbox enabled now that the list exists. For the native app: `dotnet workload install
maui` into the user-level SDK, `dotnet build -f net10.0-ios`, `simctl install` + `launch`. It booted
first time, with content and translations from the app package and no managed exceptions in the
system log — the seams built on 2026-08-26 for Windows carried over to iOS without a change.

The WebKit screenshot raised a flag that turned out to be half real: the mode tabs on Prepare
ghosted through the sticky top bar. The bar is surface colour at 92% plus `backdrop-filter:
blur(10px)`. Measured properly (three variants, pixel-sampled at the device scale factor), the
Chromium screenshot blurs what is behind the bar as designed, while Playwright's headless WebKit
reports the blur as applied but does not render it — so the 8% of content that bleeds through
arrives sharp. Real Safari blurs; but iOS 15–17 (and the WKWebView the Hybrid app runs in there)
only honour the `-webkit-` prefixed property, which the stylesheet did not declare. Fix: declare
both forms and make the bar 96% opaque, so it reads cleanly with or without the blur. The lesson
was as much about the measurement as the bug: the first pixel count sampled the wrong region
because the screenshot is 3× the CSS pixels, and said "0 differences" for every variant.

Also from reading the Spanish pack as a Spanish speaker rather than validating it as JSON: "Racha
de 1 días", "Faltan 1 días" — the packs had no singular forms, and neither did English ("1 days").
Added `home.streak.day` and `home.interview.tomorrow` in all seven languages and a three-way switch
in Home; changed the weekday letters from Spain's L M X J V S D to Lu Ma Mi Ju Vi Sá Do, which reads
naturally on both sides of the Atlantic; and fixed the manifesto's "Citiz es libre" (free as in
freedom) where the English means free of charge. Pedro counted that reading as the second review
the localisation guide asks for, so the pack is now `Reviewed` in Settings.

**Content angle:** "test on the engine your users actually have" — a concrete, visual example of a
bug that Chromium-only testing cannot find, plus the small localisation lesson that a validator
proves parity, not grammar.

---

<!--
  Adding an entry: date heading (`## YYYY-MM-DD — short title`), then what shipped, the story, and a
  one-line **Content angle**. Write it close to when it happened, while the reasoning is still fresh
  — that's the whole value of this file over reconstructing it later from commit messages.
-->
