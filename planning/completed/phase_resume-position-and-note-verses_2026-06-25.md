# Phase: Resume-to-last-viewed scripture + verse numbers in notes pane (v1.0.4)

**Date:** 2026-06-25 → 2026-06-26
**Branch:** `claude/v1-0-4-resume-position-and-note-verses`
**PR:** #44
**Status:** Completed — implemented, unit-tested, App builds; verified on-device by owner.

This phase skipped the formal in_progress→completed waterfall (it grew out of a live
brainstorm), so this document captures both the **initial plan** as approved and **what was
actually accomplished**, for the corpus.

---

## Part 1 — Initial Plan (as approved)

### Context

Two reader-facing changes for v1.0.4, decided through brainstorming:

1. **Resume to the last-viewed scripture, per topic, across launches.** A topic always opened
   at the top, so progress felt lost. Track which scripture sits at the top of the viewport as
   the reader scrolls, remember it per topic, and on return scroll that scripture back to the
   top. An *item pointer* (not a pixel offset — MAUI can't restore those reliably), serving
   both the linear "read them all" reader and the skip-around reader, replacing the earlier
   "first-unread / last-read" anchor idea. Persisted to SQLite so an OS kill of a backgrounded
   app never loses position.
2. **Verse numbers in the notes scripture pane.** The note editor rendered the verse as one
   plain-text block; the feed shows numbered verses. The editor was handed only the joined
   `VerseText` string, never the structured `Verses` the feed uses. Thread the structured
   verses through and render them like the feed.

Plus the release version bump (1.0.3 → 1.0.4, versionCode 4 → 5).

### Method

Test-driven, red → green → refactor, one behavior at a time. The testable core lives in the
Data stores and view models; the MAUI page wiring (scroll event, lifecycle persist,
programmatic scroll) is kept thin and verified by on-device launch, not unit tests.

### Feature 1 — Resume to last-viewed scripture

- **Cycle 1 — `TopicPositionStore` (Data).** Mirror `NoteStore`: ctor takes `AppDatabase` +
  optional clock; `SaveAsync(topicKey, refId)` upserts, `GetAsync(topicKey)` returns the saved
  ref or null. New `TopicPosition` model (`[PrimaryKey] TopicKey`, `RefId`, `UpdatedAtUtc`).
  Register the table in `AppDatabase.InitializeAsync`. Tests: null when empty; save/get;
  overwrite; per-topic independence.
- **Cycle 2 — `TopicFeedViewModel` resume + record.** Inject `TopicPositionStore`. On load,
  stash the topic key and the saved resume ref. Add `ResumeCard()` (loaded card whose `Id`
  matches the saved ref, else null), `RecordVisible(refId)` (in-memory pointer), and
  `SavePositionAsync()` (upsert the pointer; no-op when nothing known). Tests via the existing
  `Harness` (extended with a `TopicPositionStore`).
- **Cycle 3 — App wiring (verified on-device).** DI registration; `TopicFeedPage` `Scrolled`
  handler → `RecordVisible` + debounced `SavePositionAsync`; `OnAppearing` initial-load branch
  scrolls to `ResumeCard()` (deferred, `animate: false`); `OnDisappearing` flush. Note-return
  and app-resume branches do not re-scroll.

### Feature 2 — Verse numbers in the note editor's scripture pane

- **Cycle 4 — `NoteEditorViewModel` carries verses.** Replace `VerseText` with
  `IReadOnlyList<ContextLineViewModel> Verses`; `HasVerse => Verses.Count > 0`.
- **Cycle 5 — Navigation passes structured verses.** `NoteVersesParameter = "verses"`;
  `OpenNoteAsync` passes `card.Verses`. Retire `NoteVerseTextParameter`.
- **Cycle 6 — Render (XAML, verified on-device).** Read the `verses` query param; render with
  the feed's `BindableLayout` + `FormattedString` per-verse template.

### Version

`ApplicationDisplayVersion` 1.0.3 → 1.0.4, `ApplicationVersion` 4 → 5.

### Verification plan

Unit tests red→green per cycle; full `dotnet test` green; on-device resume (within and across
launches), first-visit-opens-at-top, numbered verses in the note pane.

---

## Part 2 — What Was Accomplished

### Delivered

- **`TopicPosition` + `TopicPositionStore`** mirroring `NoteEntry`/`NoteStore`; table
  registered in `AppDatabase.InitializeAsync`. `CreateTableAsync` is additive → no migration
  for existing installs.
- **`TopicFeedViewModel`**: `ResumeCard()`, `RecordVisible(refId)`, `SavePositionAsync()`. The
  live pointer is seeded from the saved position on load, so leaving without scrolling
  re-persists the same reference rather than wiping it.
- **`TopicFeedPage`**: `Scrolled` capture (`FirstVisibleItemIndex` → ref id), debounced
  (750 ms) save on scroll-settle, entry-time `ScrollTo(Start, animate: false)`, and an
  `OnDisappearing` flush. Note-return / app-resume never re-yank an active reader.
- **Note editor**: structured `Verses` threaded end-to-end (navigation → VM → XAML); the
  plain-text `VerseText` / `NoteVerseTextParameter` path retired. Verses render with the feed's
  numbered-verse template.
- **Font sizing confirmed**: the note pane verses bind to the same `ReadingFontSize` /
  `VerseNumberFontSize` app-level `DynamicResource` keys the feed uses (updated live by
  `AppearanceApplier`, verse number at 0.6× via `VerseNumberRatio`), so they scale with the
  Settings slider in real time — inherited for free by reusing the feed template.
- **Version** bumped to 1.0.4 / versionCode 5.

### Verification results

- All six TDD cycles completed red → green.
- Full suite green: Data 20, Core 32, Presentation 98 (**150 total**), output pristine under
  strict CPM + warnings-as-errors.
- `JesusTheChrist.App` (net10.0-android) builds with **0 errors**.
- **On-device (owner): both features confirmed working**; note-pane verse numbers present.

### Deviations from plan

- The original (rejected) read-state "first-unread / last-read" anchor was dropped entirely in
  favor of the persisted last-viewed item pointer — one mechanism for both reader types,
  decided during the brainstorm. Across-launch persistence (not session-only) was a deliberate
  call: OSes aggressively hibernate/kill apps, and session-only memory would feel flaky. Owner
  feedback confirmed the across-launch stickiness "makes it feel so much more seamless."

### Lessons / notes

- Pushing all decision logic into the unit-tested view model kept the untestable MAUI page
  seam minimal (event wiring + a scroll call), which is the right trade for a no-emulator
  project that verifies UI on a physical device.
- Restoring by *item* rather than pixel offset sidesteps MAUI's unreliable offset restore and
  survives card collapse/expand height changes.
- `gh pr create` body with embedded double quotes trips PowerShell 5.1 native-arg quoting —
  use `--body-file` for PR bodies.

### Follow-ups

- PR #44 stays BLOCKED until all review threads (incl. Copilot) are resolved via GraphQL; only
  `darthmolen` merges (see main branch protection).
