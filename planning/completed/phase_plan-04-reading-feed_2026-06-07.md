# Phase: Plan 04 — Readable Reference Feed + Mark-Read

**Date:** 2026-06-07
**Roadmap:** `planning/roadmap_2026-06-07.md`
**Branch/PR:** `feature/topic-reading-feed` (PR-04)

## Objective

Make the app usable for study: tapping a sub-topic opens a feed of reference cards showing the
actual target verse text, a "show context (±2)" expander, the note gloss (only when informative),
and a per-reference read checkmark that persists and advances the Home rings. Replaces the
PR-A `topic` stub page.

## Approach

- **Presentation (TDD):** `ContextLineViewModel`, `ReferenceCardViewModel` (read toggle + context
  toggle), `TopicFeedViewModel.LoadAsync(key)` composing `ContentService` + `ReadMarkStore`
  (gloss via `Reference.ShowGloss`, ids via `Reference.Id(key)`).
- **App (verified by launch):** `TopicFeedPage` (CollectionView of verse cards, checkmark via
  tap, context via `BindableLayout`), wired to the `topic` route receiving the key.

## Success criteria

- Tap a topic → readable verse cards; checkmark persists; returning to Home shows advanced rings.
- `dotnet build` clean (warnings-as-errors); `dotnet test` green (new TopicFeed tests).
- Owner confirms on device. PR-04 opened into `main`; owner merges.

## Outcomes

_(to be filled on completion)_
