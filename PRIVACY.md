# Citiz Privacy Policy

**Effective date:** October 8, 2026 · **Applies to:** the Citiz web app at
<https://peopleworks.github.io/Citiz/>, the Citiz apps for Android, iOS and Windows, and the `citiz`
command-line tool.

**Who is responsible:** Citiz is an open-source project published by PeopleWorks (the maintainers of
<https://github.com/peopleworks/Citiz>). Privacy questions and requests: **peopleworks@gmail.com**, or
a GitHub issue at <https://github.com/peopleworks/Citiz/issues>.

Preparing for citizenship should not require handing over sensitive immigration information. Citiz is
built so that it cannot ask for it: there is no account, no server that sees what you study, no
analytics, no advertising, no crash reporting, and no third-party SDK.

## The short version

- **Nothing you enter leaves your device.** Citiz has no account and no back end. It never sends your
  name, your dates, your settings or your progress anywhere.
- **Citiz collects no data about you.** We cannot see who uses the app or how.
- **Two kinds of downloads happen**, both started by you: the app itself (from GitHub Pages on the
  web, from the app store on mobile) and, if you choose, an audio pack from our audio host. Those
  servers see an ordinary download request, not what you study.
- **You can export or delete everything** from *Settings › Your data* at any time.

## What Citiz stores on your device

Everything below is optional, is entered by you, and is kept only on the device you use.

| What | Why | Where it is kept |
| --- | --- | --- |
| A first name or nickname (optional) | The greeting on Home | `citiz.name` |
| Interface, study and help languages | So the app opens in your languages | `citiz.profile` |
| Light or dark theme | Appearance | `citiz.theme` |
| The date you filed Form N-400 (optional) | To pick the 2008 or 2025 test; you can choose the version directly instead | `citiz.exam` |
| The test version you chose (optional) | Same | `citiz.exam` |
| Whether you use the 65/20 special consideration | To practice the 65/20 list and rules | `citiz.exam` |
| Your interview date (optional) | To show how many days are left | `citiz.exam` |
| Which questions and words you practiced, how many times, how well, and when they come back | Spaced review and your progress | `citiz.progress` |
| Whether you dismissed the one-time offer to download audio | So the offer is not repeated | `citiz.audio.offer` |
| Audio packs you downloaded (optional) | So questions and words play without a network | Browser Cache Storage on the web; the app's own data folder in the mobile and desktop apps |

**Where "on your device" means:** in the web app, your browser's `localStorage` and Cache Storage for
the site `peopleworks.github.io`. In the Android, iOS and Windows apps, the app's private storage (the
WebView's local storage and the app's data folder), which other apps cannot read.

**Operating-system backups.** Android Auto Backup and iCloud/iTunes backups may include an app's
private storage, according to the settings of your device and your Google or Apple account. The Citiz
apps do not yet exclude their data from those backups, so a device backup may contain the items in the
table above, including any audio pack you downloaded. A backup is encrypted and handled by Google or
Apple under their own policies; Citiz never receives it.

## What Citiz never asks for

- Social Security number
- Alien Registration Number (A-Number)
- USCIS online account credentials, receipt numbers or case numbers
- Copies of a green card, passport, Form N-400 or any document
- Your legal name, address, date of birth, place of birth, or where you live
- Your location, contacts, photos, microphone or camera

The name field accepts anything you like, including nothing, and is used only for the greeting.

## Network connections

Citiz makes no connection on its own. The only network traffic is:

1. **Loading the app and its content.** On the web, the app, the official questions, the vocabulary,
   the capsules and the translations are static files served by GitHub Pages. GitHub may keep standard
   web-server logs (IP address, time, file requested) under the
   [GitHub Privacy Statement](https://docs.github.com/site-policy/privacy-policies/github-general-privacy-statement).
   In the mobile and desktop apps, all of that is bundled inside the app, so nothing is fetched to
   study.
2. **Audio packs, only if you download one.** From *Settings › Audio* or the one-time offer, you can
   download the official USCIS recordings or the synthetic Citiz voice. The files come from
   `peopleworksservices.com`, a server operated for this project. It sees the download request (your IP
   address, the time and the files requested) and may keep standard web-server logs for operational
   purposes; it never learns which question you study or how you answer, because playback is local
   after the download. Nothing is sent back to it.
3. **Reading aloud with your device's voice.** On Android, iOS and Windows the phone's or computer's
   own speech engine speaks on the device. In a browser, speech synthesis is usually on the device too;
   some browsers use a network voice, and the app tells you when that is the case (*Settings › What
   runs where*).

There are no cookies, no analytics, no advertising identifiers, no crash reporting and no
third-party SDKs in any version of Citiz. The optional self-hosted API in this repository holds no
learner data.

## Your choices

- **Export:** *Settings › Your data › Download my progress* saves your progress ledger as a JSON file.
- **Delete:** *Settings › Your data › Delete everything* removes every item in the table above,
  including downloaded audio packs, from this device. Uninstalling the app, or clearing the site's
  data in your browser, does the same.
- **Audio packs** can be deleted one at a time from *Settings › Audio*.

Because Citiz holds no data about you, there is nothing for us to access, correct or delete on your
behalf: it is all, and only, on your device.

## Children

Citiz is a study aid for adults preparing a naturalization application. It collects no personal
information from anyone, of any age.

## AI features

None are built yet. When Citiz offers an AI conversation or explanations, they will be optional, off by
default, and the app will say exactly what would be sent and to whom before you turn them on. The
design rule is in [`Docs/Privacy/LOCAL_VS_CLOUD.md`](Docs/Privacy/LOCAL_VS_CLOUD.md): every feature
declares where it runs, and anything remote is disclosed at the moment you enable it.

## Self-hosting

Organizations that host their own copy get the same properties: the app is static files, and the
optional API holds no learner data. Nothing in this repository phones home to the Citiz project.

## Changes to this policy

Changes are made in the public repository, where the history of this file can be inspected. A change
that affects what the app stores or sends will also be noted in [`CHANGELOG.md`](CHANGELOG.md) and
the effective date above will be updated.
