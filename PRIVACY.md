# Privacy

Preparing for citizenship should not require handing over sensitive immigration information. Citiz
is built so that it cannot ask for it.

## What Citiz stores, and where

Everything is in your browser's `localStorage`, on your device:

| Key | What | Why |
| --- | --- | --- |
| `citiz.profile` | Your interface, study and help languages | So the app opens in your language |
| `citiz.exam` | The date you filed Form N-400 (optional), an explicit version choice (optional), whether you use the 65/20 consideration | So the app practices the right version with the right rules |
| `citiz.progress` | Which questions and words you practiced, how many times, how well, and when they come back | Spaced review |

You can download all of it as a file (Settings → *Download my progress*) and delete all of it
(Settings → *Delete everything*). Nothing is sent to a server. There is no account. There are no
cookies, no analytics, no crash reporting.

## What Citiz never asks

- Social Security number
- Alien Registration Number (A-Number)
- USCIS online account credentials or receipt numbers
- Copies of a green card, passport, N-400 or any document
- Your name, address, date of birth, or where you live

The filing date is asked because the test version depends on it; it is optional, and you can pick the
version directly instead.

## Where each feature runs

| Feature | Where it runs |
| --- | --- |
| Questions, answers, versions, rules, vocabulary, capsules | On your device (static files downloaded with the app, cached for offline use) |
| Answer checking | On your device (deterministic matcher; no AI) |
| Progress and spaced review | On your device |
| Interface translations | On your device |
| Reading text aloud | Your browser's speech synthesis. Most browsers synthesize on the device; some use a network voice, and the interface tells you which one it is using |
| AI conversation and explanations | Not built yet. When they exist they will be optional, off by default, and will say what is sent and to whom before you turn them on |

The full policy is [`Docs/Privacy/LOCAL_VS_CLOUD.md`](Docs/Privacy/LOCAL_VS_CLOUD.md): every
feature declares an execution class, and anything remote is disclosed at the moment you enable it.

## Self-hosting

Organizations that host their own copy get the same properties: the app is static files, and the
optional API holds no learner data. Nothing in this repository phones home to the Citiz project.
