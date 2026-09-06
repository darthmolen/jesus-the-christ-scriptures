# How to get the app onto testers' phones

[HOW-TO-DEPLOY.md](HOW-TO-DEPLOY.md) covers *building and uploading* a bundle. This covers the
other half: handing someone a link that installs it.

The goal is a link you can post once — not the three-step round trip of "check your phone for
your Google account → send it to me → wait for an invite." How close you can get depends
entirely on one fact, so establish it first.

## Step 0 — check your account type

**Play Console → Settings → Developer account → Account details.**

Google requires a closed test of **12 testers opted in continuously for 14 days** before granting
production access — but *only for personal accounts created after 2023-11-13*. **Organization
accounts are exempt.** ([Play policy][testing-req])

| Account type | What you need | Go to |
|---|---|---|
| Organization | nothing — exempt | [Path A](#path-a--open-testing-lowest-friction) |
| Personal, created before 2023-11-13 | nothing — exempt | [Path A](#path-a--open-testing-lowest-friction) |
| Personal, created after 2023-11-13 | 12 testers × 14 days | [Path B](#path-b--closed-testing-with-a-google-group) |

> The number has moved. It was 20 at launch and dropped to 12 in December 2024. Older notes in
> this repo (`planning/completed/google-play-publishing_2026-06-10.md`) say 20; that file is a
> historical record and is deliberately not edited. **12** is current.

## Path A — open testing (lowest friction)

If you're exempt, use **open testing** and skip everything below it. One link, anyone installs,
no group membership, no opt-in dance.

1. Play Console → **Testing → Open testing → Create new release**.
2. **Add from library** — promote the bundle already uploaded to `internal` rather than
   rebuilding. Play serves a tester the highest `versionCode` on any track they can see.
3. Roll out, then copy the link from the track's **Testers** tab.
4. Post the link anywhere. That's it.

Open testing is reviewed and the listing is publicly discoverable, so the
[prerequisites](#prerequisites-that-block-review) below still apply.

## Path B — closed testing with a Google Group

A closed track gates installs on a tester list. That list can be individual emails or a **Google
Group**, and the difference is the whole point:

- **Email list** — you collect each address and paste it into the Console. Every new tester is a
  round trip through you. This is the friction you're trying to avoid.
- **Google Group** — you add the group address **once** and never open the Console again. Anyone
  who joins the group is eligible.

### Set it up

1. **Open the group up.** Google Groups → your group → **Settings → Members → Who can join
   group → "Anyone on the web can join."** Without this you're back to approving every request
   by hand, and the self-service flow doesn't work.
2. **Point the track at it.** Play Console → **Testing → Closed testing** → your track →
   **Testers** tab → **Google Groups** → enter `yourgroup@googlegroups.com`.
3. **Put a build on the track.** *Create new release → Add from library* — promote the existing
   `versionCode`. No rebuild, no new tag.
4. **Copy the opt-in URL** from the Testers tab. It looks like:
   `https://play.google.com/apps/testing/com.vozloop.jesusthechristscriptures.full`

### What the tester does

Two self-service clicks, and **zero round trips through you**:

1. Click the group's join link → instantly a member.
2. Open the opt-in URL → **Become a tester** → install from Play.

**These cannot be collapsed into one link on a closed track.** Group membership has to come
*before* opt-in, and being on the list is not the same as being opted in — every tester must open
the opt-in URL themselves and press the button, or they don't count. ([Play docs][set-up-test])

### The handout

> **English** — Two quick steps to install *Scriptures: Jesus the Christ*:
> 1. Join the testers group: `<group join link>`
> 2. Open `<opt-in link>`, tap **Become a tester**, then install from Google Play.
>
> Use the same Google account for both, and the same one your phone's Play Store is signed in to.

> **Español** — Dos pasos rápidos para instalar *Escrituras: Jesús el Cristo*:
> 1. Únete al grupo de probadores: `<group join link>`
> 2. Abre `<opt-in link>`, toca **Convertirme en probador** e instala desde Google Play.
>
> Usa la misma cuenta de Google en ambos pasos, y la misma con la que tu teléfono inició sesión
> en Play Store.

### Rules worth knowing before you recruit

- **12 testers, opted in *continuously* for 14 days.** Someone who opts out and back in restarts
  their own 14-day clock; they don't count until it completes again.
- **Since 2026 Google also checks testers genuinely used the app.** Recruit people who will
  actually open it, not twelve names who install and forget.
- **The first closed-testing release is reviewed** — up to about 7 days. Later ones are quick.
- **Testers must be signed into the Play Store with the account that joined the group.** This is
  the single most common "it says I'm not a tester" cause.

## Prerequisites that block review

Open in this repo today; any of them will stall a first review on a reviewed track:

- [ ] **Contact email and website** are still `<fill in — Molen Solutions>` placeholders —
      [play-listing.md](play-listing.md).
- [ ] **Feature graphic** (1024×500) and **2–8 phone screenshots** are not made yet —
      [play-listing.md](play-listing.md).
- [ ] **Privacy policy URL.** The policy is already GitHub-Pages-ready at
      [privacy-policy.md](privacy-policy.md) (`permalink: /privacy/`). Confirm it resolves
      publicly, then paste the URL into the Console.
- [ ] **App content declarations** — data safety, target audience, ads, content rating.

## A note on the release workflow

[`release.yml`](../../.github/workflows/release.yml) hardcodes `PLAY_TRACK: internal` and fires
only on a `v*` tag; there is no `workflow_dispatch` and no track input. That is deliberate — CI
always lands on the safe, unreviewed track, and moving a build to another track is a Console
promotion from the app library, which needs no rebuild. Don't edit the workflow to change tracks.

[testing-req]: https://support.google.com/googleplay/android-developer/answer/14151465
[set-up-test]: https://support.google.com/googleplay/android-developer/answer/9845334
