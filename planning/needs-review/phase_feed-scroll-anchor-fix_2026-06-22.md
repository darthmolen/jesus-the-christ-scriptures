# Phase: Roll the heading down instead of jumping the feed up on "done"

Status: needs-review
Date: 2026-06-22
Branch: claude/scripture-scroll-position-fix-toiga0

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

## Root cause

`ScrollToPosition.MakeVisible` resolves the off-screen-above heading to the **top** of the
viewport. We instead want it at the **bottom** (`ScrollToPosition.End`) for the tall-card
case, while preserving today's "leave an already-visible short card untouched" behavior
(documented in the comment at `TopicFeedPage.xaml.cs:74-77`).

## Files to modify

- `src/JesusTheChrist.App/Views/TopicFeedPage.xaml.cs` — change the scroll anchor logic.
- `src/JesusTheChrist.App/Views/TopicFeedPage.xaml` — wire up the `CollectionView.Scrolled`
  event so we can tell the tall-card case from a short, already-visible card.

The ViewModel (`ReferenceCardViewModel.ToggleReadAsync` / `onReadCollapsed`) and the
`CardCollapsedAfterRead` event plumbing are correct and stay as-is — only where/how the
view re-anchors the scroll changes.

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

### 2. Anchor the collapsed heading to the footer's place (code-behind)

In `TopicFeedPage.xaml.cs`:

- Store the most recent `ItemsViewScrolledEventArgs` from a new `OnReferencesScrolled`
  handler.
- In `OnCardCollapsedAfterRead`, decide the scroll position:
  - **Tall-card case** — the expanded card filled the viewport, so its heading was above
    the top edge. Detect this from the last scroll state: the card was the first visible
    item and (at most) only the next item peeked in below it
    (`FirstVisibleItemIndex == cardIndex && LastVisibleItemIndex <= cardIndex + 1`).
    Use `ScrollToPosition.End` so the collapsed heading rolls down to the bottom of the
    viewport — exactly where the footer ✓ just was.
  - **Otherwise** (short card already fully on screen, or no scroll has happened) — keep
    `ScrollToPosition.MakeVisible`, which no-ops when the card is already visible and so
    preserves today's behavior of leaving the next reference right below it.

Sketch:

```csharp
private ItemsViewScrolledEventArgs? lastScroll;

private void OnReferencesScrolled(object? sender, ItemsViewScrolledEventArgs e)
    => this.lastScroll = e;

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

            // Tall-card case: the expanded card filled the screen, so its heading sat
            // above the top edge while the reader tapped the footer ✓. Roll the heading
            // down into the footer's place (End) instead of yanking the feed up to the
            // top (MakeVisible). A short, already-visible card keeps MakeVisible, which
            // no-ops and leaves the next reference sitting right below it.
            var filledViewport = this.lastScroll is { } s
                && s.FirstVisibleItemIndex == index
                && s.LastVisibleItemIndex <= index + 1;

            var position = filledViewport
                ? ScrollToPosition.End
                : ScrollToPosition.MakeVisible;

            this.ReferencesView.ScrollTo(e.Card, position: position, animate: true);
        });
}
```

Keep `animate: true` and the 100 ms settle delay — the animated scroll is what makes the
heading visibly "roll" into place rather than teleport. Update the existing comment block
(lines 74-77) to describe the new End/MakeVisible split.

## Why this matches the request

- "Maintain position" → the feed no longer jumps to the top; the reader's gaze stays near
  the bottom of the screen where they just tapped.
- "Roll the header down so it takes the place of the footer" → `ScrollToPosition.End`
  lands the collapsed heading at the bottom of the viewport, where the footer ✓ was.
- Smooth feed feel → `animate: true` plays the short scroll as a roll, and short cards are
  left untouched so nothing moves when it doesn't need to.

## Verification

- Build: `dotnet build src/JesusTheChrist.App/JesusTheChrist.App.csproj` to confirm the
  XAML wiring and code-behind compile.
- Manual (emulator/device), since the behavior is view-layer:
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
- Build succeeds; no regressions in expand/collapse or un-read.

## Outcomes (to fill in after implementation)

- _Pending review/approval of this plan._
