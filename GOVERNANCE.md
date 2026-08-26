# Governance

Citiz is a small open-source project with an outsized responsibility: people study for a real
interview from what it shows. Governance exists to keep the official content correct and the
project's promises intact, not to add ceremony.

## Roles

| Role | Responsible for | Today |
| --- | --- | --- |
| **Technical maintainers** | Architecture, code, security, releases, CI | Pedro Hernández (PeopleWorks) |
| **Content maintainers** | Accuracy of official content: comparing banks with USCIS documents, keeping dynamic answers current, approving content changes | Pedro Hernández; volunteers welcome |
| **Language maintainers** | One per language: glossary, review of the pack, marking it *reviewed* | Open for every language except English |
| **Community moderators** | Issues, discussions, conduct | Technical maintainers, until the community grows |
| **Educational advisory group** | Citizenship instructors, ESL teachers, librarians, people who went through naturalization; advice on what actually helps | To be formed; see ROADMAP.md |

Roles are earned by doing the work. A contributor who has verified content or reviewed a language a
few times will be offered the role, with commit rights to the relevant folders via `CODEOWNERS`.

## How decisions are made

- **Day to day:** by pull request review. The `CODEOWNERS` file routes content changes to content
  maintainers and everything else to technical maintainers.
- **Architecture:** by an ADR in `Docs/Architecture/` (numbered, short, with the decision and its
  consequences). A pull request that contradicts an accepted ADR proposes a new one.
- **Editorial policy:** by an EDR in `Docs/Editorial/`. EDR-0001 (official content) and EDR-0002
  (review states) are the two everything else builds on.
- **Disagreement:** discussed in the pull request or issue; the maintainer of the affected area
  decides; the decision and its reasoning stay in the thread.

## What no one can decide

Some things are not up for a vote, because they are the reason the project exists:

1. Official answers come only from official sources, transcribed verbatim, with a review status.
2. Essential learning never requires an account.
3. Nothing leaves the device without disclosure and consent.
4. No sponsor, donor or partner gains authority over official answers, educational results or
   editorial policy (see *Sustainability*).

## Content approval

Marking content `approved` is a human act by a content maintainer who opened the cited source and
compared. It is recorded in the content file itself (`reviewStatus`, `verifiedOn`) and in git history.
When the content worker reports that a source changed, the dependent content goes back to
`needs-review` (or `outdated` if it was approved) until someone re-verifies it.

## Sustainability

Basic access stays free. Donations, sponsorships, grants or paid support for organizations may fund
the project; none of them buys influence over content or policy, and learner data is never a product.
