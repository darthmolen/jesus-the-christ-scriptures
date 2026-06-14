# Phase: Card inline actions (note + read follow the reader)

**Date:** 2026-06-14
**Branch:** feature/card-inline-actions

## Objective

On a topic feed card, the note button and read checkmark only lived in the card
heading. For a long passage, a reader had to scroll back up to mark it read or
start a note. Make those actions follow the reader: keep them in the heading
while the card is collapsed, and move them to the foot of the card (next to the
"Show scriptural context" toggle) while the card is expanded.

## Approach

- `ReferenceCardViewModel`: add `IsCollapsed` (inverse of `IsExpanded`), notified
  whenever `IsExpanded` changes.
- `TopicFeedPage.xaml`:
  - Heading note button + read box gated on `IsCollapsed` (Auto columns collapse
    to zero width when hidden, so the heading reads full-width while expanded).
  - Footer rebuilt into a single action bar: context toggle on the left, a
    matching note button + read box on the right. The whole footer already lives
    inside the `IsExpanded` collapsible body, so it only shows while expanded.
  - Separator BoxView now always shows in the expanded body (divides scripture
    from the action bar) instead of only when context exists.

## Files modified

- src/JesusTheChrist.Presentation/ViewModels/ReferenceCardViewModel.cs
- src/JesusTheChrist.App/Views/TopicFeedPage.xaml
- tests/JesusTheChrist.Presentation.Tests/ViewModels/TopicFeedViewModelTests.cs

## Outcome

- Added `IsCollapsed_MirrorsInverseOfIsExpanded` test. Full suite: 84 passed.
- Behavior: collapsed card shows actions in the heading (as before); expanded
  card moves them to the foot so the reader never scrolls up to act.

## Notes / deviations

- No new converter needed; a computed `IsCollapsed` VM property keeps the XAML
  declarative.
