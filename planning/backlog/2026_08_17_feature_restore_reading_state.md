# Backlog: come back to a topic the way you left it

**Date:** 2026-08-17
**Origin:** Owner, during 1.0.7 UAT.

## The report

> If a note is marked "read" and then re-opened, when exiting and coming back, it is closed.

Confirmed, and **pre-existing** — not introduced by 1.0.7. It dates to
`phase_card-inline-actions_2026-06-14`, which made marking a reference read roll its card up.

## Root cause

[`ReferenceCardViewModel.cs:102`](../../src/JesusTheChrist.Presentation/ViewModels/ReferenceCardViewModel.cs#L102)
derives expansion from read state at construction:

```csharp
this.IsExpanded = !isRead;
```

`ToggleExpanded` flips that flag in memory and nothing persists it. Both `TopicFeedPage` and
`TopicFeedViewModel` are `AddTransient` (`MauiProgram.cs`), so leaving the topic and returning
rebuilds every card from read state alone and discards the reader's manual re-open.

The same is true of the ±context window (`IsContextVisible`), which also resets on return.

## The decision (owner)

There are two competing models of what the feed is:

- **Reading Mode** — the point is the content; you are working through a passage.
- **Checklist Mode** — the point is completion; read cards roll up so progress is visible.

> I think we can treat them identically. At the end of the day they are about reading the content;
> the achievement part is just a nice add-on. So if I leave off at a place (even if it was completed
> before) then when I come back, it should still be how I left it.

So **restore state as left**, permanently, independent of read state. Read state stops driving
expansion after the first time; it becomes the *initial* value only, not a standing override.

This is the same philosophy as [[2026_06_29_feature_readers_view_toggle]] — content first,
achievement as an add-on — and the two should feel coherent if both ship.

## Scope when picked up

Per-reference view state that should survive navigation and launch:

1. **Card expansion** (`IsExpanded`) — the reported case.
2. **Context window** (`IsContextVisible`) — same class of reset, worth doing together.
3. **Chapter position within a span** — already done in 1.0.7 via `ChapterPositionStore`; it is the
   precedent to follow, and arguably these should share one store rather than accumulate tables.

### Suggested shape

Rather than a table per flag, consider a single `ReferenceViewState` row keyed by the
language-invariant `Reference.Id`: `{ RefId, IsExpanded, IsContextVisible, Ch, UpdatedAtUtc }`,
folding in `ChapterPosition`. That keeps one bulk read at feed load (`GetAllAsync`) instead of one
per concern, which is the pattern `TopicFeedViewModel.LoadAsync` already uses for read-marks and
notes.

Migration note: `ChapterPosition` shipped in 1.0.7, so folding it in means either a real migration
or keeping both tables and reading the old one once. `CreateTableAsync` is additive, so the cheap
path is a new table plus a one-time copy.

### Open questions

- Does marking a card read still collapse it *at that moment*? (Assumed yes — that is the reward
  moment, and it is what makes the checklist legible.) The change is only that a later manual
  re-open sticks.
- Should un-reading a card re-expand it? Today it does. Probably keep.
- Is there a "reset this topic's view state" escape hatch, for a reader who re-opened dozens of
  cards and wants the checklist back? Possibly a Settings action rather than per-card.

## Not doing now

Deferred out of 1.0.7 at the owner's request — 1.0.7 is in UAT and this is pre-existing behaviour,
not a regression from the three features in it.
