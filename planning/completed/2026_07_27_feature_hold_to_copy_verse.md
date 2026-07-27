# Phase: Hold a verse to copy it

**Date:** 2026-07-27
**Branch:** `feature/hold-to-copy-verse` (stacked on `feature/card-copy-button`)
**PR:** _(pending)_
**Status:** In progress
**Backlog item:** `planning/backlog/copy-verse-button.md` — this phase completes it; the backlog file is
deleted here.

## Context

Phase 1 gave the card a **Copy** button that lifts the whole reference. The owner then asked for a
finer instrument: press and hold a single verse line to copy just that verse, with the highlight
ramping while held and a toast confirming the copy.

Two constraints from phase 1 carry forward, and one is new:

1. The presentation layer still has no MAUI reference — the gesture and the toast are therefore view
   concerns, and only the copy-text logic can be unit-tested.
2. Nothing may be allocated per verse at load. `ContextLineViewModel` is the most-instantiated type in
   the app — a bare immutable POCO with no `INotifyPropertyChanged`, deliberately — and a feed can hold
   hundreds of them.
3. **New:** no gesture, behaviour, or animation code exists anywhere in `src/` today. `TapGestureRecognizer`
   is the only recognizer in the codebase. This is all new ground.

## Objective / success criteria

- Pressing and holding any verse line for **600ms** copies that verse; releasing early copies nothing.
- The verse's background ramps from transparent to an accent tint over the hold, so the reader can see
  the gesture arming.
- A scroll that begins on a verse must **not** arm the gesture.
- The copy is `Book Chapter:Verse`, one line break, then the verse text.
- A toast confirms, drawn in-page — no new NuGet package.
- Works on both the target-verse lines and the ±context window lines.
- Localized in English and Spanish. Solution builds clean; all tests green.

## Decisions

| Question | Decision |
|---|---|
| Where the command lives | **`ReferenceCardViewModel`**, one `CopyVerseCommand` per card, with the verse as `CommandParameter` |
| Why not on the verse | A command per verse means a delegate plus an `AsyncRelayCommand` for every line in the feed, allocated at load. `ContextLineViewModel` stays untouched |
| Header source | Resolved at press time by scanning `Segments` — costs nothing until the gesture fires |
| Hold duration | 600ms, just past Android's ~500ms long-press threshold |
| When the copy fires | When the ramp **completes**, not on release — see the amendment below |
| Hold effect | Background fill ramp on the existing `Label`. An outline would need a `Border` per verse, roughly doubling per-verse view count on a feed that already lazy-realizes chapters |
| Toast | In-page overlay `Border` on `TopicFeedPage`, faded via the page's existing `DispatchDelayed` idiom. `CommunityToolkit.Maui` would add a package for one call |
| Feedback consistency | The card button keeps its in-place `✓ Copied` flip; a verse line has no persistent control to flip, hence the toast |

### Resolving the chapter

`ContextVerse.Ch` is discarded when `ContextLineViewModel` is built, so a verse line cannot name itself.
It does not need to:

1. Find the segment whose `Verses` contains the instance (by reference) → that segment's `ChapterLabel`.
2. Otherwise the line came from the ±context window → fall back to the first segment's `ChapterLabel`.

The fallback is exact rather than a guess: of 2,196 references in the shipped corpus only three are
cross-chapter, and all three carry zero non-target context verses — so every context-window verse
belongs to the reference's own chapter. This is a data-shape assumption a future corpus could break, so
it is commented in the code and pinned by a test.

`ChapterLabel` is `$"{BookTitle} {Ch}"` and is localized by the corpus (`Matthew 10` / `Mateo 10`), so
the header needs no resource string.

## Files expected to change

- `src/JesusTheChrist.Presentation/ViewModels/ReferenceCardViewModel.cs` — `CopyVerseCommand`,
  header resolution, verse copy text
- `src/JesusTheChrist.Presentation/ViewModels/TopicFeedViewModel.cs` — `VerseCopied` event
- `src/JesusTheChrist.App/Views/TopicFeedPage.xaml` — pointer gestures on both verse templates, toast overlay
- `src/JesusTheChrist.App/Views/TopicFeedPage.xaml.cs` — hold tracking, ramp, toast
- `src/JesusTheChrist.App/App.xaml` — highlight tint resources
- The three resource files — `ToastVerseCopied`, `A11yHoldToCopyVerse`
- `tests/.../ViewModels/ReferenceCardViewModelTests.cs`, `tests/.../Resources/AppResourcesTests.cs`
- `planning/backlog/copy-verse-button.md` — deleted

## Approach — test-driven where testable

The view-model half is TDD'd: header from chapter label and verse number, the single line break, no
verse number in the body, cross-chapter attribution, the context-window fallback, the clipboard write,
and a null parameter as a no-op.

The gesture, the ramp, the movement-cancel threshold, and the toast have **no automated coverage** and
are carried entirely by the device checklist below. That is stated plainly because it is the risky half.

## Verification

- `dotnet build` clean, `dotnet test` green.
- On the physical Android phone:
  - Hold a target verse ~600ms → highlight ramps, release copies, toast reads "Verse copied".
  - Paste is `Matthew 10:25`, newline, verse text — no verse number in the body.
  - Release early (~300ms) → no copy, highlight clears.
  - **Scroll starting on a verse** → no highlight, no copy. Test a slow drag as well as a flick; this is
    the most likely defect.
  - Hold a ±context verse → correct chapter in the header.
  - Hold a verse in the second chapter of a cross-chapter card → that chapter's label, not the first's.
  - Toast in Spanish; two copies in quick succession do not truncate each other's toast.
  - Light and dark theme both show a legible highlight.
  - TalkBack: the hold gesture is invisible to it by design; the card's **Copy** button remains the
    accessible route to the same content.

## Versioning

No version bump here either. Both phases ship under one Play Store version, bumped in a separate
release PR after both merge.

## Outcomes

Implemented as designed. `dotnet build JesusTheChrist.slnx` succeeds with zero warnings; `dotnet test`
is green across all three projects — **188 tests** (Core 44, Data 20, Presentation 124), up from 180.

Eight new tests: six on `ReferenceCardViewModel` (header from chapter label and verse number, no verse
number in the body, cross-chapter attribution, the context-window fallback, null parameter as a no-op,
and that a verse copy leaves the card button's `JustCopied` alone), and two on `TopicFeedViewModel`
(a verse copy writes the verse and raises `VerseCopied`; the card button's copy does **not** raise it).

The gesture, ramp, slop threshold, and toast have **no automated coverage** and are entirely unverified
— nothing here has run on a device yet. The device checklist above is the whole safety net for that
half, and the scroll-versus-hold case is the one most likely to need tuning.

`planning/backlog/copy-verse-button.md` is deleted; both halves of it now exist.

## Amendment — copy on ramp completion (2026-07-27, after device testing)

Both paths were verified working on the phone. The owner then asked for the copy to fire at the **end
of the animation** rather than on release, so the gesture reads as strictly either/or: you abandon it,
or it completes and copies.

The ramp was already the clock; it now also commits. `ViewExtensions.Animate` takes a `finished`
callback of `(double, bool cancelled)`, and every way of abandoning a hold — early release, drifting
past the slop radius, pointer exit, navigating away — routes through `CancelVerseHold`, which calls
`AbortAnimation` and therefore arrives at that callback with `cancelled: true`. So the not-cancelled
branch is reached only by a hold that ran its full 600ms, and it is the single place the copy fires.

What fell out:

- `Stopwatch` timing and the `heldStartTimestamp` field are gone — the animation measures the hold, so
  nothing needs to time it. The `System.Diagnostics` using went with them.
- `OnVersePointerReleased` and `OnVersePointerExited` became identical (end the hold, clear the tint)
  and collapsed into one `OnVerseHoldEnded` handler, wired to both events on both verse templates.
- The copy moved into a small static `CopyVerse(Label)` helper, called from the callback.

Net effect on the reader: the verse copies the instant the highlight fills, and lifting a finger is no
longer part of the contract. Lifting afterwards just clears the highlight. Build clean, 188 tests still
green — the change is entirely in the untested view layer, so it carries no new automated coverage and
was verified by hand.

**Known cosmetic behaviour:** the highlight stays at full tint from the moment the copy fires until the
finger lifts. The toast is the confirmation, so this reads acceptably, but fading the tint out on fire
would punctuate the gesture more clearly. Left as-is pending the owner's preference.

## Deviations from plan

1. **The card gained a second copy delegate rather than an event.** `CopyVerseCommand` calls
   `copyVerseAsync`, distinct from the button's `copyAsync`. `TopicFeedViewModel` supplies a method
   that writes the clipboard and then raises `VerseCopied`. The card therefore never learns what a
   toast is — whether a copy is confirmed, and how, stays the feed's business.
2. **`Animation` + `Commit` replaced with `ViewExtensions.Animate`.** CA2000 flagged the `Animation`
   instance as an undisposed `IDisposable`; the `Animate` extension takes the same callback without
   constructing one.
3. **`FadeTo` is obsolete in MAUI 10** (CS0618, an error here) — switched to `FadeToAsync`. Worth
   noting for any future animation work in this repo, since there was no prior animation code to
   copy an idiom from.
4. **Highlight colours are two plain resource keys, not an `AppThemeBinding`.** The ramp is animated
   from code-behind, where a markup extension does not resolve; the theme is read once from
   `Application.Current.RequestedTheme` at press time.
5. **Toast hide moved into a `HideToastAsync` helper.** An `async` lambda handed to `DispatchDelayed`
   would have been `async void`; the helper keeps it a `Task` the caller can discard deliberately.

## Lessons learned

- MAUI 10 obsoletes the `*To` animation extensions in favour of `*ToAsync`. This repo had no prior
  animation code, so there was no local idiom to follow and the compiler was the first thing to say so.
- CA2000 makes the `Animation` class awkward in a warnings-as-errors project. `Animate` is the better
  default for a one-off tween regardless.
- Definite assignment interacts awkwardly with hoisting a pattern-match out of a guard: capturing
  `is ContextLineViewModel line` into a separate bool left `line` unassigned as far as the compiler
  was concerned. Keeping the pattern inside the `||` guard is both shorter and legal.
- A press-and-hold inside a scrolling list is mostly a story about *cancellation*, not about the hold:
  three of the four handlers exist only to abandon the gesture. That is worth remembering if this
  pattern is ever reused elsewhere in the app.
