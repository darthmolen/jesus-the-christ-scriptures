# Phase: Roll the heading down instead of jumping the feed up on "done"

Status: revised-after-review
Date: 2026-06-22
Branch: claude/scripture-scroll-position-fix-toiga0
Audit trail: planning/needs-review/completed/phase_feed-scroll-anchor-fix_2026-06-22.md

## Context

The app is a social-feed-style scripture reader (.NET 10 MAUI, Android-first). Each
reference is a card in a `CollectionView` (`TopicFeedPage`). When the reader taps the
"done" (✓) checkmark, the card collapses to just its heading so the feed reads like a
progress checklist.

For a **tall** scripture (taller than the screen), the reader scrolls down and taps the
✓ in the card's **footer** action bar, which sits near the bottom of the screen. The
card's heading is scrolled off the **top** of the viewport at that moment. Today, after
the collapse, `TopicFeedPage.OnCardCollapsedAfterRead` calls
`ReferencesView.ScrollTo(card, ScrollToPosition.MakeVisible)`. Because the now-tiny
heading sits above the viewport, "make visible with the least scrolling" yanks the whole
feed **up** so the heading lands at the **top** of the screen. That abrupt jump is
disconcerting and breaks the smooth, keep-reading feel of a feed.

Desired behavior: maintain the reader's position and let the heading **roll down into the
footer's place** at the bottom of the viewport, so the next reference flows in naturally
below — no jump to the top.

Related prior work: `planning/completed/phase_feed-scroll-on-collapse_2026-06-16.md`
introduced the current `MakeVisible` re-anchor; this phase refines it.

## Review disposition (this revision)

Adversarial review verdict was **Implementable with fixes**. Changes folded in here:

- **[Critical] Brittle tall-card predicate → MERGED.** Concern accepted; the reviewer's
  geometry-based suggestion is rejected as impractical (MAUI `CollectionView` exposes no
  reliable per-item viewport bounds). Resolved by extracting the decision into a pure,
  unit-testable helper and documenting the residual brittleness as an on-device check.
- **[Important] No automated coverage → ACCEPTED.** The anchor decision lives in a helper
  in the `JesusTheChrist.Presentation` project (the MAUI `App` head is not referenced by
  the test project) and is unit-tested.
- **[Important] Storing `ItemsViewScrolledEventArgs` underspecified → MERGED.** Cache only
  `int?` first/last visible indices; reset on topic load; read-before-write makes
  programmatic-scroll feedback benign.
- **[Minor] Missing phase-closing steps → ACCEPTED.** Added below.

## Root cause

`ScrollToPosition.MakeVisible` resolves the off-screen-above heading to the **top** of the
viewport. We instead want it at the **bottom** (`ScrollToPosition.End`) for the tall-card
case, while preserving today's "leave an already-visible short card untouched" behavior
(documented in the comment at `TopicFeedPage.xaml.cs:74-77`).

## Files to modify

- `src/JesusTheChrist.Presentation/Views/ScrollAnchor.cs` — **new** pure helper holding the
  "which `ScrollToPosition`?" decision (unit-testable, no MAUI view dependency).
- `src/JesusTheChrist.App/Views/TopicFeedPage.xaml` — wire up `CollectionView.Scrolled`.
- `src/JesusTheChrist.App/Views/TopicFeedPage.xaml.cs` — cache visible indices, delegate to
  `ScrollAnchor.Resolve`, reset on load.
- `tests/JesusTheChrist.Presentation.Tests/Views/ScrollAnchorTests.cs` — **new** unit tests.

The ViewModel (`ReferenceCardViewModel.ToggleReadAsync` / `onReadCollapsed`) and the
`CardCollapsedAfterRead` event plumbing are correct and stay as-is — only where/how the
view re-anchors the scroll changes.

> Note: confirm `ScrollToPosition` is reachable from the Presentation project. It lives in
> `Microsoft.Maui.Controls`. If the Presentation project does not reference MAUI controls,
> have `ScrollAnchor.Resolve` return a small local `enum ScrollAnchorPosition { End,
> MakeVisible }` and map it to `ScrollToPosition` in the code-behind. Pick whichever keeps
> the helper dependency-free; the tests assert the helper's own return type.

## Approach

### 1. Track the latest scroll state (XAML)

Add a `Scrolled` handler to the feed `CollectionView` (`TopicFeedPage.xaml`, the
`ReferencesView` element, lines 18-22):

```xml
<CollectionView
    x:Name="ReferencesView"
    Grid.Row="1"
    ItemsSource="{Binding References}"
    Scrolled="OnReferencesScrolled"
    SelectionMode="None">
```

### 2. Anchor the collapsed heading via a pure, testable helper

Pure decision helper (Presentation project, no view state):

```csharp
public static class ScrollAnchor
{
    // End -> roll the collapsed heading down to the footer's old spot (tall card filled the
    // viewport, so its heading sat above the top edge). MakeVisible -> no-op for a short,
    // already-visible card, leaving the next reference right below it.
    public static ScrollToPosition Resolve(int cardIndex, int? firstVisible, int? lastVisible)
    {
        var filledViewport = firstVisible == cardIndex
            && lastVisible is { } last
            && last <= cardIndex + 1;

        return filledViewport ? ScrollToPosition.End : ScrollToPosition.MakeVisible;
    }
}
```

Code-behind caches only primitives and delegates:

```csharp
private int? lastFirstVisible;
private int? lastLastVisible;

private void OnReferencesScrolled(object? sender, ItemsViewScrolledEventArgs e)
{
    // Cache primitives, not the framework args object. We read these before we issue our
    // own ScrollTo, so the Scrolled event our scroll raises can't feed back into a decision.
    this.lastFirstVisible = e.FirstVisibleItemIndex;
    this.lastLastVisible = e.LastVisibleItemIndex;
}

private void OnCardCollapsedAfterRead(object? sender, ReferenceCardEventArgs e)
{
    this.Dispatcher.DispatchDelayed(
        TimeSpan.FromMilliseconds(100),
        () =>
        {
            if (!this.isVisible)
            {
                return;
            }

            var index = this.viewModel.References.IndexOf(e.Card);
            var position = ScrollAnchor.Resolve(index, this.lastFirstVisible, this.lastLastVisible);
            this.ReferencesView.ScrollTo(e.Card, position: position, animate: true);
        });
}
```

Reset the cached indices at the top of `LoadAsync` (topic load/reload) so stale indices
from a prior topic cannot misclassify the first collapse:

```csharp
this.lastFirstVisible = null;
this.lastLastVisible = null;
```

Keep `animate: true` and the 100 ms settle delay — the animated scroll is what makes the
heading visibly "roll" into place rather than teleport. Update the existing comment block
(`TopicFeedPage.xaml.cs:74-77`) to describe the new End/MakeVisible split via the helper.

**Residual brittleness (on-device check).** The `lastVisible <= cardIndex + 1` allowance is
the known weak spot: two medium cards that together fill the screen can satisfy it. Verify
on a real device across font sizes that short, already-visible cards still take the
`MakeVisible` (no-op) path. If misclassification shows up, tighten the predicate (e.g.
require `lastVisible == cardIndex`) — the helper makes this a one-line, test-covered change.

## Why this matches the request

- "Maintain position" → the feed no longer jumps to the top; the reader's gaze stays near
  the bottom of the screen where they just tapped.
- "Roll the header down so it takes the place of the footer" → `ScrollToPosition.End`
  lands the collapsed heading at the bottom of the viewport, where the footer ✓ was.
- Smooth feed feel → `animate: true` plays the short scroll as a roll, and short cards are
  left untouched so nothing moves when it doesn't need to.

## Verification

- Build: `dotnet build src/JesusTheChrist.App/JesusTheChrist.App.csproj`.
- Unit tests: `dotnet test tests/JesusTheChrist.Presentation.Tests/...` covering
  `ScrollAnchor.Resolve`:
  1. Card fills viewport, nothing peeks (`first == index`, `last == index`) → `End`.
  2. Card fills viewport, next peeks (`first == index`, `last == index + 1`) → `End`.
  3. Short card with two+ items below (`first == index`, `last >= index + 2`) → `MakeVisible`.
  4. No scroll yet (`null` indices) → `MakeVisible`.
  5. Card is not the first visible item (`first < index`) → `MakeVisible`.
- Manual (emulator/device), since the rest is view-layer:
  1. Open a topic with a scripture longer than one screen; expand it and scroll to the
     footer ✓.
  2. Tap ✓ → confirm the heading rolls **down** to the bottom of the screen (footer's old
     spot) and the next reference flows in below — no jump to the top.
  3. Tap ✓ on a **short**, fully-visible card → confirm the feed does **not** scroll and
     the next reference stays right below it (unchanged behavior).
  4. Un-read a collapsed card → confirm it re-expands and nothing scrolls unexpectedly.

## Success criteria

- Marking a tall scripture done leaves its heading at the bottom of the viewport (footer's
  place); no jump to the top.
- Short-card "done" behavior is unchanged.
- `ScrollAnchor.Resolve` unit tests pass; build succeeds; no regressions in
  expand/collapse or un-read.

## Phase-closing checklist (CLAUDE.md)

After implementation:

1. Fill in the Outcomes section below with actual results and any deviations.
2. Move this doc to `planning/completed/`.
3. Commit on the feature branch using the `[PHASE]` message format (never direct to
   `main`); open/update the PR. Owner merges.

## Outcomes

Implemented 2026-06-22 on branch `claude/scripture-scroll-position-fix-toiga0`.

- **Helper + enum (new):** `src/JesusTheChrist.Presentation/Views/ScrollAnchor.cs` and
  `ScrollAnchorPosition.cs`. The `Presentation` project is a plain SDK project with **no
  MAUI reference** (confirmed in its `.csproj`), so the helper returns a local
  `ScrollAnchorPosition { End, MakeVisible }` enum — the review's geometry suggestion was
  correctly rejected, and the testable-helper merge proved necessary, not just nice-to-have.
- **Unit tests (new):** `tests/JesusTheChrist.Presentation.Tests/Views/ScrollAnchorTests.cs`
  — 5 cases (fills-no-peek → End, fills-peek → End, multiple-below → MakeVisible, no-scroll
  → MakeVisible, not-first-visible → MakeVisible). All green; full suite 98/98.
- **View wiring:** `TopicFeedPage.xaml` gained `Scrolled="OnReferencesScrolled"`;
  `TopicFeedPage.xaml.cs` caches `int?` first/last visible indices, resets them on fresh
  topic load (the `References.Count == 0` branch in `OnAppearing`), and maps the helper's
  result to MAUI `ScrollToPosition` in `OnCardCollapsedAfterRead`.
- **TDD note:** the `MakeVisible`-only stub failed to compile under IDE0060
  (warnings-as-errors flag unused params) rather than failing an assertion — that compile
  failure served as the RED before the real predicate made it green.

### Deviations / open items

- **MAUI head not built here:** `dotnet build` of the App project fails with `NETSDK1147`
  (missing `maui-android` workload) in this WSL environment. The view-layer changes use
  only standard MAUI APIs already present in the original code-behind
  (`ItemsViewScrolledEventArgs.FirstVisibleItemIndex/LastVisibleItemIndex`,
  `ScrollToPosition`, `ScrollTo`); the App build must be confirmed where the workload is
  installed (CI/owner machine).
- **Manual device verification still pending** (steps in Verification), including the
  on-device check of the `<= cardIndex + 1` predicate across font sizes.

## Revision 2 — on-device test + design pivot (2026-06-22)

On-device testing on the owner's physical phone surfaced two things:

1. **Deploy issue, not a code issue.** The "fix not reflected" symptom was a **stale duplicate
   app**: the phone still had the old-namespace package `org.jesusthechrist.full` installed
   alongside the current `com.vozloop.jesusthechristscriptures.full`. Tapping the old icon ran a
   fixless APK. Resolved by uninstalling both via `adb` and doing a clean rebuild/redeploy.
   (Separately, a VS clean-build hit spurious `NU1010`/`APT0003` from a half-restored `obj/`; a
   CLI `dotnet build` restored cleanly and regenerated `obj/`. Both gotchas recorded in the
   `maui-build-environment` memory.)

2. **Behavior pivot, by owner preference.** With the fix actually running, the owner observed
   that parking the read heading at the **top** (with the next scripture cued up right below it)
   read better than the originally-specified "roll down to the footer's bottom spot." Decision:
   make that the intentional behavior for **every** card, tall or short.

### What changed in this revision

- `TopicFeedPage.xaml.cs` `OnCardCollapsedAfterRead` now unconditionally calls
  `ScrollTo(e.Card, ScrollToPosition.Start, animate: true)` after the 100 ms settle delay. The
  `isVisible` guard and the deferred-settle delay are kept.
- **Removed as now-dead:** the `Scrolled="OnReferencesScrolled"` wiring in `TopicFeedPage.xaml`,
  the `OnReferencesScrolled` handler, the cached `lastFirstVisible`/`lastLastVisible` fields and
  their reset in `OnAppearing`, and the whole resolver layer —
  `src/JesusTheChrist.Presentation/Views/ScrollAnchor.cs`, `ScrollAnchorPosition.cs`, and
  `tests/JesusTheChrist.Presentation.Tests/Views/ScrollAnchorTests.cs`. The End/MakeVisible
  branching is no longer needed, so the brittle `<= cardIndex + 1` predicate (the review's chief
  concern) is gone entirely rather than tuned.

### Verification (this revision)

- App build: `dotnet build src/JesusTheChrist.App/JesusTheChrist.App.csproj -c Debug
  -p:TargetFramework=net10.0-android` → **0 warnings, 0 errors**.
- Tests: `dotnet test tests/JesusTheChrist.Presentation.Tests` → **93/93** (was 98; the 5
  removed `ScrollAnchorTests` accounts for the drop).
- Manual (owner, physical phone): mark a tall scripture done → its heading scrolls to the top of
  the viewport and the next reference sits ready below; un-read re-expands without a jarring jump.
