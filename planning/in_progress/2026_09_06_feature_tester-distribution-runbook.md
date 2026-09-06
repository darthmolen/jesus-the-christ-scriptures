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
