# 0001 — Chapter navigation inside a reference card

**Status:** Accepted
**Date:** 2026-08-17
**Deciders:** Owner (`darthmolen`)
**Context:** 1.0.7 UAT — "the sub-chapter opens at the bottom of the viewport (it should be the top)"

## Context

The topic feed is one `CollectionView` whose items are **references**:

| Level | Primitive | Scrollable |
|-------|-----------|------------|
| References | `CollectionView`, `ItemsSource="{Binding References}"` | **Yes** — virtualizing |
| Chapters | `BindableLayout.ItemsSource="{Binding Segments}"` on a `VerticalStackLayout` | No |
| Verses | `BindableLayout.ItemsSource="{Binding VisibleVerses}"` | No |

`BindableLayout` is an attached property, not an `ItemsView`. It materializes children into a plain
stack: no index, no scroll surface, no `ScrollTo`. And `ItemsView.ScrollTo(item, …)` works by
resolving the item to an index **in the CollectionView's own `ItemsSource`**.

**Consequence: a reference is one opaque row.** A card holding 3 Nephi 11–26 — heading, 16 chapters,
445 verses, footer — cannot be scrolled *into*. The finest scroll target the framework offers is
the whole card.

`BindableLayout` was chosen deliberately in the cross-chapter work (`2026_06_27`). A `CollectionView`
nested inside a `CollectionView` item is a known MAUI measurement hazard — a virtualizing list inside
a virtualizing list has no natural height — and `BindableLayout` is what makes the lazy-realization
trick possible, where a collapsed chapter binds to an empty list and builds zero views.

1.0.7 added chapter memory: opening a chapter collapses its siblings and the choice is remembered.
That exposed the gap. Opening a chapter changes the card's height *above* the reader, and with one
stacked header per closed chapter, chapter 26 sat behind fifteen of them — roughly 450du, most of a
phone screen. The chapter the reader had just chosen appeared near the bottom of the viewport.

## Decision

**Keep references as the only scroll target. Remove the need to scroll inside a card instead.**

For a span long enough to use chapter memory:

1. Chapters are listed as a **compact wrapped strip of chapter numbers** at the top of the card,
   with the open one filled. Tapping a number opens that chapter; tapping the open one closes it.
2. A closed chapter **draws nothing** (`ChapterSegmentViewModel.IsRendered`). The strip carries the
   navigation its header used to provide.
3. Opening a chapter raises `TopicFeedViewModel.ChapterExpanded`, and the page re-anchors with the
   existing `ScrollTo(card, ScrollToPosition.Start)` seam — the same one the read-collapse fix uses.

Everything above the open chapter is now the card heading plus one or two wrapped rows — roughly
110du instead of 450du — so anchoring the *card* to the top puts the *chapter* effectively at the top.

Short spans and single-chapter references are untouched: no strip, headers inline, as before.

## Consequences

**Good**

- No new primitive, no platform-specific code, no restructuring. The fix is a view-model flag, a
  `FlexLayout`, and an event on a seam that already existed.
- The strip is better navigation than the stack it replaced: a 16-chapter span becomes a chapter
  picker rather than a column of headers to scroll past.
- Chapter position is now exact enough in practice **without** the framework ever needing to address
  a chapter.
- **Confirmed on device** (owner, 1.0.7 UAT): *"the little boxes actually make it more readable.
  Sure it's not scrolling, but scrolling can be tedious, so why not change within the same box?"*
  See the verdict below — this is the load-bearing consequence, not a nicety.

**Bad**

- The positioning is *good enough*, not exact. It depends on the strip staying short. A span of ~40+
  chapters would wrap to several rows and start pushing the chapter down again.
- Chapters remain unaddressable. Any future feature that must scroll to something *inside* a card —
  a verse-level deep link, search-result highlighting, "jump to verse 23" — hits this same wall and
  gets no help from this decision.
- Closed chapters draw nothing, so their per-chapter ↗ study links are unreachable until the chapter
  is opened. Accepted: open the chapter, then use its link.

**Neutral**

- `ShowChapterStrip` is deliberately the same condition as `UsesChapterMemory`. Two names for one
  idea, kept because each reads correctly at its own end (view vs. behaviour). If they ever need to
  diverge, that is the signal this decision is being outgrown.

## Verdict after use (2026-08-17)

The framing above — a constraint worked around — undersells what happened, and the correction
matters more than the record of it.

The strip was reached for because chapters could not be scrolled to. On device it turned out to be
**the better interaction on its own merits**, independent of the constraint that produced it. The
owner's words: *"I don't know what I was expecting but I love it... Sure it's not scrolling, but
scrolling can be tedious, so why not change within the same box? I wouldn't have thought of that. I
was still stuck on scroll scroll scroll."*

This is a reading app. Scrolling is the reader's principal cost, and a design that spends scrolling
on *navigation* is spending it away from the content. Swapping content inside a fixed container is
not a workaround for a scroll limitation here; it is the thing that should have been designed first.

**The consequence for whoever reads this next:** do not treat the strip as debt awaiting repayment
by the "proper" grouped `CollectionView`. When a revisit trigger fires, the move is almost certainly
to add chapter addressability **underneath** the strip — so that a verse deep link or a search hit
can scroll precisely — while keeping the strip as the reader-facing navigation. Replacing the strip
with per-chapter scrolling would trade a better interaction for a more orthodox one.

## Alternatives considered

### Grouped `CollectionView` — groups are references, items are chapters

The native answer. `IsGrouped="true"` makes `ScrollTo(chapter, card, ScrollToPosition.Start)` a
supported call and chapters genuinely first-class.

Rejected **for now**, not on merit but on blast radius. The card's rounded `Border` chrome would
split into `GroupHeaderTemplate` / `GroupFooterTemplate`; the context window and footer action bar
move into the footer template; and `OnReferencesScrolled` uses `FirstVisibleItemIndex` against
`References` to persist reading position — that index would start meaning "chapter", so the resume
logic needs remapping. That is a substantial rewrite of the feed and its own UAT pass, to benefit
**2 references out of 2,196**.

### Android scroll-by-offset through the handler

Reach past `CollectionView` to the platform `RecyclerView` and scroll a measured pixel delta. Exact,
and could even hold the tapped header stationary under the reader's finger. Rejected as the first
platform-specific code in the repo, needing on-device iteration to tune, for a positioning problem
that a layout change removes outright.

### Nested `CollectionView` per card

Rejected twice over: the measurement hazard above, and it would not work anyway — scrolling an inner
list moves content within a fixed box, not the page. The chapter would land at the top of a small
window sitting wherever the card happened to be.

### Do nothing (accept card-top scrolling with stacked headers)

The cheapest option, and the behaviour is at least deterministic. Rejected because it degrades
exactly where the feature matters most: the reader deep in a long span, which is the whole reason
chapter memory exists.

## Revisit when

Any one of these should reopen the question. The answer will likely involve the grouped
`CollectionView` — but read the verdict above first: reopening this means asking how to gain
chapter and verse **addressability**, not whether to replace the strip.

- **The app grows past "Topical Guide scroller."** This decision is scoped to a feed of short
  references where cross-chapter spans are a rare special case. Full-chapter reading, a search
  results view, or any browsing surface changes that premise.
- **Anything needs to scroll to a verse.** Verse-level deep links (inbound from
  `docs/DEEP-LINKING-TO-LDS-WEBSITE.md`), search hits, or note back-links all need a target finer
  than a card, and none of them are served by this.
- **A span gets long enough that the strip wraps past ~2 rows**, which puts the open chapter back
  down the viewport.
- **Chapters need independent virtualization** — a card whose realized chapter is itself too large
  to build at once.
