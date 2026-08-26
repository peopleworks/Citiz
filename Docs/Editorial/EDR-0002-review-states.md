# EDR-0002: Review states

**Status:** Accepted · **Date:** 2026-08-25

## The states

Every content entry (exam version, question, dynamic answer, vocabulary list, capsule) carries one
`reviewStatus`:

| State | Meaning | Shown to learners as |
| --- | --- | --- |
| `draft` | Written, not yet checked against its source. | "Editorial draft" |
| `needs-review` | Complete and sourced, waiting for a content maintainer to verify it against the cited source. | "Not yet verified" |
| `approved` | Verified against the cited source by a content maintainer, on `verifiedOn`. | (no label) |
| `outdated` | Was approved; the source changed since. Must be re-verified. | "Outdated" |

## Transitions

```
draft ──(sources attached, complete)──▶ needs-review ──(human compared with source)──▶ approved
                                              ▲                                          │
                                              └──────(source changed: worker or report)──┘ → outdated
```

- `needs-review → approved`: a content maintainer opens the cited source, compares, sets
  `reviewStatus: approved` and `verifiedOn: <today>` in the same pull request. Partial approval is
  fine — per question, per entry.
- `approved → outdated`: when the content worker reports that a monitored source changed, or a
  correction is reported with a source, the dependent entries are set `outdated` until re-verified.
  They keep showing, labelled, because a labelled answer is better than a missing one; if the change
  is known to be substantive, the maintainer edits the content immediately and goes through review
  again.
- Nothing moves to `approved` by automation. Ever.

## What the states control

- **The interface** labels everything that is not `approved` (`ReviewBadge`). The label is a title
  attribute and text, not just a colour.
- **Discovery** shows capsules from `draft` up, labelled; it never shows `outdated`.
- **`citiz content report`** counts entries per state per file; the number of pending entries is the
  project's honest "how much of this is verified" figure.
- **CI** fails on structural errors, not on `needs-review` — unverified content is allowed to exist
  so it can be verified in the open, but it is never allowed to pretend.

## File-level defaults

A question bank has a file-level `reviewStatus` that each question inherits unless it sets its own,
so a bank can be approved question by question without editing 128 entries at once.
