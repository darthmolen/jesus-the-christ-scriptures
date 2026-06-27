# Phase: Cross-chapter scripture references — a vetted engine + data fix

**Date:** 2026-06-27
**Branch:** `claude/cross-chapter-references`
**PR:** _(pending — opened after owner review)_
**Status:** Implemented — 169 unit tests pass, App builds clean (0 warnings under strict CPM). Awaiting on-device verification by owner.

## Context

The topic feed renders each scripture reference as a card of verses. Three references in the
English **"Summary"** subtopic span multiple chapters, and all three are broken: the external
`lds-nl-scriptures` pipeline grabbed only the **start chapter in full** but kept the original
multi-chapter label, so the label and the verses disagree and context is lost.

| Entry (en.json) | Label shown | Data actually contains | True passage |
|---|---|---|---|
| `Matt. 9:35–11:1` (note "sends disciples forth by twos") | all of Matt 9:1–38 | Matt 9:35–38 + all 10 + 11:1 |
| `Matt. 5–7` (note "Sermon on the Mount") | all of Matt 5:1–48 | Matt 5, 6, 7 |
| `chaps. 11–26` (3 Nephi, no note → meaningless header) | all of 3 Ne 11:1–41 | 3 Ne 11–26 |

The Spanish corpus "fixed" these the wrong way too — relabeled to the start chapter
(`Mateo 9:1–38`, `Mateo 5:1–48`, `3 Nefi 11:1–41`), silently dropping the rest.

**Decision:** build a real, vetted cross-chapter primitive now, apply it to these 3, keep it
general for the rest of the Topical Guide later. Be faithful — show the true span. Fix EN **and** ES.
Full verse text is local at `c:/dev/lds-nl-scriptures/content/processed/scriptures/{en,es}/` — no API.

## Objective / success criteria
- Data model carries per-verse chapter + a reference end-chapter, fully back-compatible with the
  2,196 existing single-chapter entries.
- A reusable `TargetSegments()` / `MissingChapters()` engine on `Reference`, unit-tested.
- Feed renders spanned chapters as collapsible sections (lazy verse realization) so large spans
  (Matt 5–7 ≈111 v, 3 Ne 11–26 ≈600 v) don't jank; small spans (Matt 9:35–11:1 ≈47 v) open fully.
- The 3 entries corrected in both bundled JSON files from the local corpus, hand-verified.
- Solution builds clean under strict CPM + warnings-as-errors; Core tests pass.

## Files expected to change
- Core: `Json/ContextDto.cs` (+`ch`), `Json/ReferenceDto.cs` (+`end_ch`), `Models/ContextVerse.cs`
  (+`Ch`), `Models/Reference.cs` (+`EndCh`, `SpansChapters`, `TargetSegments()`, `MissingChapters()`,
  span-aware `Id`), `Models/ChapterSegment.cs` (new), `Json/TopicalGuideLoader.cs`.
- Presentation: `ViewModels/ContextLineViewModel.cs` (+`Ch`), `ViewModels/ChapterSegmentViewModel.cs`
  (new), `ViewModels/ReferenceCardViewModel.cs` (`Segments`), `ViewModels/TopicFeedViewModel.cs`.
- App: `Views/TopicFeedPage.xaml` (collapsible chapter sections).
- Data: `Resources/Raw/jesus-christ.en.json`, `jesus-christ.es.json` (3 entries each).
- Tooling: a committed one-off extractor (decide C# console under `tools/` vs `scripts/` Python).
- Tests: Core test project.

## Decisions / defaults taken on auto
- **Default-expand rule:** first chapter segment always expanded; later segments expanded only when
  the card's total target verses ≤ 60. Keeps Matt 9:35–11:1 fully open, big spans collapsed-after-first.
- **`end_ch` only emitted for spanning refs** (omitted = single chapter), so the diff to the corpus
  is minimal and back-compatible.
- **Relabel `chaps. 11–26` → `3 Ne. 11–26`** and set `book_title` "3 Nephi" so the header names the book.
- Collapsed sections bind their verse `BindableLayout` to an **empty list** so child `Label`s are
  never built (true lazy realization, not just `IsVisible=false`).

## Out of scope (logged to backlog)
General feed scroll jitter — likely the same non-virtualized in-card measure cost. Proposed separate
fix: prefetch ~2–3 cards around the viewport. Captured in `planning/backlog/feed-scroll-prefetch.md`.

## Outcome

Delivered as planned. Commits on `claude/cross-chapter-references`:
1. Planning docs + backlog item.
2. Core engine: `ContextVerse.Ch`, `Reference.EndCh`/`SpansChapters`/`TargetSegments()`/
   `MissingChapters()`/span-aware `Id()`, new `ChapterSegment`, loader projection — 9 new Core tests.
3. Presentation: `ChapterSegmentViewModel` (lazy realization), `ReferenceCardViewModel.Segments`,
   `TopicFeedViewModel.BuildSegments` (first chapter always expanded; rest expanded only when the
   passage is ≤ 60 target verses) — 7 new Presentation tests.
4. XAML per-chapter collapsible sections + the data fix (extractor + rewritten JSON).

**Verification:** Core 44, Data 20, Presentation 105 (169 total) pass; App builds clean, 0 warnings.
The extractor self-checks that corpus text matches the prior bundled text on overlapping verses —
passed for all 6 entries, so the spans are faithful and stylistically consistent.

**Blast radius confirmed at 3** (EN "Summary" / ES "Resumen", indices 27/28/74; EN & ES align 1:1
across 103 refs). True spans inlined: Matt 5–7 = 111 v, Matt 9:35–11:1 = 47 v, 3 Ne 11–26 = 445 v.

**Decisions / deviations:**
- `3 Ne 11–26` is 445 verses (the planning estimate of ~600 was high). Still inlined in full per
  the "faithful" decision; lazy per-chapter realization keeps the card cheap until expanded.
- Relabeled only the broken `chaps. 11–26` → `3 Ne. 11–26`; kept the other two EN labels. Fixed
  the Spanish wrong-collapses (`Mateo 5:1–48` → `Mateo 5–7`, etc.); preserved ES `note_en`/
  `note_source` fields untouched.
- The data diff is large (~16k lines) — inherent to inlining 603 verses × 2 languages — but the
  `git round-trip` fidelity (indent=2, no trailing newline) keeps every other entry byte-identical.

**Lessons:** the bundled JSON round-trips exactly through `json.dumps(indent=2, ensure_ascii=False)`
with no trailing newline, which made a surgical 3-entry edit possible without reformatting 87k lines.
The cross-chapter Id change means read-state/notes/position keys for these 3 refs reset — acceptable
since the prior entries were broken; no migration needed.

**Remaining:** on-device verification by the owner (no emulator), then open the PR into `main`
(owner-only merge).
