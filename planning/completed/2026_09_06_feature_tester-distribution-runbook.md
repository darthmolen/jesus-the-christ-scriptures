# Tester distribution runbook

Date: 2026-09-06
Branch: `docs/tester-distribution`

## Objective

Answer a concrete question: *can I generate one link and hand it out, or do I have to add every
tester by hand?* The repo had nothing on tester lists, opt-in URLs, or Google Groups, and the
one number it did record was stale.

## Outcome

Yes — with a caveat worth writing down. A **Google Group as the tester list** removes the
publisher's side of the friction entirely: you add the group address once and never open the
Console again. Setting the group to *"Anyone on the web can join"* makes membership
self-service. The tester still takes **two clicks** (join the group, then opt in), because on a
closed track membership must precede opt-in and being listed is not the same as being opted in.

The larger finding is that closed testing may not be needed at all. The 12-testers-for-14-days
gate applies only to **personal accounts created after 2023-11-13**; organization accounts are
exempt, and an exempt account can use **open testing**, where a single link works for anyone with
no group at all. Since the account type is not recorded anywhere and could not be verified from
the repo, the runbook opens by having the reader check it and then branches.

## Files

| File | Change |
|---|---|
| `docs/android/HOW-TO-DISTRIBUTE-TESTS.md` | new — account-type triage, Path A (open testing), Path B (closed + Google Group), bilingual handout, review prerequisites |
| `docs/android/HOW-TO-DEPLOY.md` | corrected the tester count; cross-linked the new doc |

## Outcome

Merged as [PR #60](https://github.com/darthmolen/jesus-the-christ-scriptures/pull/60), then
corrected by [PR #62](https://github.com/darthmolen/jesus-the-christ-scriptures/pull/62) within
hours — the first real use of the runbook disproved one of its own claims.

### What the 1.0.8 promotion taught

The runbook said closed-testing releases after the first are "quick". Promoting 1.0.8 from
`internal` to Closed testing - Alpha sat *In review* for over an hour. **Every** closed-testing
release is reviewed, not just the first; ~24h is the honest budget, up to ~3 days on a newer
account. Minutes happens, but it is luck, not the rule.

The more useful thing the episode surfaced was never in the doc at all: **promotion is the reviewed
step, and the CI upload to `internal` is not.** The same `versionCode` stays installable from
`internal` for the entire time a promotion sits in review, so checking a fix on your own device
never has to wait on Google. PR #62 adds that as its own section, along with the diagnostic tell —
in *Publishing overview → Submission activity*, every row is a track you promoted to, and internal
uploads never appear there, which is why that page can look as though CI publishes straight to
Alpha.

## Deviations from plan

None to the approach. Two things the plan did not anticipate:

- The tester count in `HOW-TO-DEPLOY.md` was **15**, and the older completed planning doc says
  **20**. The current figure is **12** (reduced from 20 in December 2024). Only the live doc was
  corrected; `planning/completed/google-play-publishing_2026-06-10.md` is a historical record and
  was deliberately left alone.
- Google now also verifies that testers genuinely *used* the app, not merely that they opted in.
  That changes who is worth recruiting, so it is called out in the runbook.

## Not done

- `release.yml` untouched, by decision. It stays on `internal` for tagged builds; moving a build to
  a testing track is a Console promotion from the app library, which needs no rebuild.

  Review caught that the first draft overstated this as "don't edit the workflow to change tracks",
  which contradicted `HOW-TO-DEPLOY.md`. Both docs now draw the same distinction: promotion moves
  one build, whereas `PLAY_TRACK` is a standing change to where every future tagged release lands.
- The Console steps are written from Play's current documentation but **have not been clicked
  through** — the account type is still unknown. `HOW-TO-DEPLOY.md` already carries a scar from
  Play moving "API access" out from under it, so treat these steps as provisional and correct them
  on first use.
- The listing prerequisites (contact email, website, feature graphic, screenshots) remain open;
  they are now a checklist in the runbook rather than a surprise at review time.
