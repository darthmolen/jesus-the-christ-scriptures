# Phase: Re-anchor feed scroll when a tall card rolls up

**Date:** 2026-06-16
**Branch:** fix/feed-scroll-on-collapse

## Problem

After the card inline-actions change (PR #32), marking a reference read collapses the card
to its heading. For a reference taller than the screen, the reader marks it read from the
footer checkmark — but the `CollectionView` keeps its absolute scroll offset while the card
shrinks above it, leaving the viewport stranded *past* the next reference, forcing a scroll
back up.

## Root cause

Collapse is a pure VM state flip (`ReferenceCardViewModel.ToggleReadAsync` sets
`IsExpanded = false`). Nothing re-anchors the feed, and MAUI's `CollectionView` does not
maintain a visible anchor item when content above the viewport shrinks.

## Fix

Re-anchor the scroll to the just-collapsed card, after its smaller layout settles:

- `ReferenceCardViewModel` gains an `onReadCollapsed` callback, invoked in `ToggleReadAsync`
  **only when marking read** (not on un-read, not on manual header collapse).
- `TopicFeedViewModel` raises `CardCollapsedAfterRead` (with a small `ReferenceCardEventArgs`)
  and passes the callback into each card.
- `TopicFeedPage` subscribes (OnAppearing) / unsubscribes (OnDisappearing) and, on the event,
  `DispatchDelayed(100ms)` → `ReferencesView.ScrollTo(card, MakeVisible, animate)`.

`MakeVisible` is deliberate: when the collapsed card sits above the viewport (the tall-card
case) it scrolls up to it so the next reference lands right below; a short card that was
already fully visible is left untouched, so the common case doesn't get a jump.

## Files

- `src/JesusTheChrist.Presentation/ViewModels/ReferenceCardViewModel.cs`
- `src/JesusTheChrist.Presentation/ViewModels/TopicFeedViewModel.cs`
- NEW `src/JesusTheChrist.Presentation/ViewModels/ReferenceCardEventArgs.cs`
- `src/JesusTheChrist.App/Views/TopicFeedPage.xaml` (+ `.xaml.cs`)
- `tests/JesusTheChrist.Presentation.Tests/ViewModels/TopicFeedViewModelTests.cs`

## Outcome

- Presentation suite **93 passed** (2 new: event raised on read-collapse, not on un-read).
- Android App builds clean (0/0).
- **Needs on-device verification** (scroll timing): the 100 ms defer is a starting value;
  if the re-anchor still lands early/late on the physical phone, tune the delay.
