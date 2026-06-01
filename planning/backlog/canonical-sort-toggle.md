# Backlog: optional "sort canonically" toggle for a sub-topic feed

**Date:** 2026-05-31
**Origin:** Plan 01 finding (decided with owner: "preserve now, add sort toggle later")

## Finding

The Topical Guide orders references **thematically, not by book/chapter**. Verified against the
raw church TG page: under *Jesus Christ, Atonement through* the order is
`Lev. 17:11 · Isa. 53:6 · Mosiah 14:6 · Zech. 9:11 · Matt. 8:17 …` — Mosiah 14:6 sits next to
Isa. 53:6 because Mosiah 14 quotes Isaiah 53. Nearly every sub-topic cycles volumes this way.
Our extract faithfully preserves this order; the app preserves it too (v1 default).

## Decision (v1)

**Preserve the TG's thematic order** in the feed — most faithful, keeps related passages
together. Guarded by `RealDataSmokeTests.Preserves_the_topical_guides_thematic_order`.

## Deferred feature

Add an optional **"sort canonically"** view so a reader can switch a sub-topic's feed to strict
scriptural order (OT→PGP, book→chapter→verse), then back to TG order.

### Work required
- Add a canonical **book index** to Core (book order within each volume — 87 books; source order
  is in `lds-nl-scriptures` `_VOLUME_BOOKS`).
- Add a `CanonicalSorter` (volume order → book index → chapter → first verse); **Summary** keeps
  its narrative order and is never re-sorted.
- A per-feed toggle (TG order ⇄ canonical), remembered in settings.

### Notes
- This is a *view* concern; reference **ids and read-marks are order-independent**, so toggling
  sort does not affect progress/notes.
- Target: a UI plan (Plan 4/5), not the foundation.
