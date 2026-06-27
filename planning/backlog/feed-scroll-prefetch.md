# Backlog: Smooth feed scrolling via viewport prefetch

## Problem
The topic feed sometimes stutters/jitters when scrolling forward and back. The outer card list is
a virtualized `CollectionView`, but each card's verse content renders through a **non-virtualized**
`BindableLayout`/`VerticalStackLayout` ([TopicFeedPage.xaml](../../src/JesusTheChrist.App/Views/TopicFeedPage.xaml)).
Variable-height cards re-measure as they enter the viewport, which is the likely source of the jitter.

## Hypothesis (validate before building)
Eagerly realizing/measuring cards just outside the viewport would absorb the re-measure cost off the
critical scroll path. **Profile first** (confirm the stutter is in-card measure, not template inflation
or image decode) before committing to an approach.

## Proposed direction
- Prefetch / eagerly realize ~2–3 cards ahead and behind the viewport.
- Consider `CollectionView.RemainingItemsThreshold`, `ItemSizingStrategy`, or measuring cards on a
  background pass and caching heights.
- Keep it independent of content correctness — purely a scroll-performance concern.

## Notes
- Surfaced during the cross-chapter-references work (2026-06-27). The collapsible chapter sections
  added there are designed to *not* worsen this (collapsed sections build no verse views).
