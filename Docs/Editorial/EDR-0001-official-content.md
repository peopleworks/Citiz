# EDR-0001: Official content policy

**Status:** Accepted · **Date:** 2026-07-28 (design document), implemented 2026-08-25

## The rule

The guiding principle of the design document, made operational:

> Software may teach, explain, connect ideas and personalize. It never invents an official answer or
> a historical fact.

1. **Only official sources define official content.** Civics questions, accepted answers, exam
   rules, the 65/20 designation and the vocabulary lists come from USCIS documents and nowhere else.
   Officeholders come from the office's own site (whitehouse.gov, speaker.gov, supremecourt.gov).
2. **Transcribed, not paraphrased.** Wording is copied verbatim, including USCIS's parentheses for
   optional words. No accepted answer is added because it "would also be accepted"; no answer is
   dropped because it "is outdated" — the source is updated first, then the content.
3. **Sourced and dated.** Every file and every standalone entry carries `sources` (authority, title,
   URL, license, `verifiedOn`) and a `reviewStatus` (EDR-0002).
4. **Dynamic answers live apart.** A question whose answer depends on who holds an office carries a
   `dynamicAnswerKey`; the name lives in `dynamic-answers.json`. State- and district-dependent answers
   are never resolved by Citiz: the learner is pointed to the official lookup, because Citiz does not
   ask where anyone lives.
5. **Editorial content is labelled editorial.** Capsules, explanations, notes and translations are
   written for Citiz, carry their own status, and never restate an official answer as their own claim.
6. **AI and community contributions cannot create or change official content.** A provider receives
   the accepted answers and returns a judgement about them; a contributor's correction goes through a
   pull request with the source quoted.
7. **Publication is a human act.** `approved` is set by a content maintainer who opened the source
   and compared, in the same pull request, with `verifiedOn` set to that day.

## Why it is this strict

People will sit in front of an officer with what this app taught them. A helpful paraphrase that the
officer does not accept is worse than no app at all. Verbatim text, visible sources and visible review
status let a learner — or their instructor — check anything in seconds.
