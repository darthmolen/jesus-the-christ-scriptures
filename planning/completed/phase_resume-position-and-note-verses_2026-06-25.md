# Phase: Resume-to-last-viewed scripture + verse numbers in notes pane (v1.0.4)

**Date:** 2026-06-25
**Branch:** `claude/v1-0-4-resume-position-and-note-verses`

## Objective

1. **Resume to the last-viewed scripture, per topic, across launches.** Track which scripture
   is at the top of the viewport as the reader scrolls, persist it per topic to SQLite, and on
   re-entry scroll that scripture back to the top. Item-pointer based (not pixel offset).
   Replaces the prior top-of-feed behavior and the earlier first-unread/last-read anchor idea.
2. **Verse numbers in the note editor's scripture pane.** Thread the structured `Verses` list
   (already used by the feed) through navigation and render numbered verses, replacing the
   single plain-text `VerseText` block.
3. Version bump 1.0.3 → 1.0.4 (versionCode 4 → 5).

## Approach (TDD, red → green → refactor)

- **Cycle 1** — `TopicPosition` model + `TopicPositionStore` (mirror `NoteStore`); register
  table in `AppDatabase.InitializeAsync`. Data tests.
- **Cycle 2** — `TopicFeedViewModel`: inject `TopicPositionStore`; add `ResumeCard()`,
  `RecordVisible(refId)`, `SavePositionAsync()`. Presentation tests + harness update.
- **Cycle 3** — App wiring (not unit-tested): DI registration; `TopicFeedPage` `Scrolled`
  handler → `RecordVisible` + debounced `SavePositionAsync`; `OnAppearing` scroll to
  `ResumeCard()`; `OnDisappearing` flush.
- **Cycle 4** — `NoteEditorViewModel`: replace `VerseText` with `IReadOnlyList<ContextLineViewModel> Verses`.
- **Cycle 5** — Navigation: `NoteVersesParameter`; `OpenNoteAsync` passes `card.Verses`.
- **Cycle 6** — `NoteEditorPage.xaml(.cs)`: render numbered verses via `BindableLayout` +
  `FormattedString` (feed template).
- Version bump.

## Files expected to change

- `src/JesusTheChrist.Data/`: new `TopicPosition.cs`, `TopicPositionStore.cs`; `AppDatabase.cs`.
- `src/JesusTheChrist.Presentation/ViewModels/TopicFeedViewModel.cs`, `NoteEditorViewModel.cs`.
- `src/JesusTheChrist.Presentation/Navigation/NavigationRoutes.cs`.
- `src/JesusTheChrist.App/`: `MauiProgram.cs`, `Views/TopicFeedPage.xaml(.cs)`,
  `Views/NoteEditorPage.xaml(.cs)`, `JesusTheChrist.App.csproj`.
- Tests: new `TopicPositionStoreTests.cs`; updated `TopicFeedViewModelTests.cs`,
  `NoteEditorViewModelTests.cs`.

## Success criteria

- All unit tests green (`dotnet test`), output pristine under strict CPM + warnings-as-errors.
- On-device: resume works within and across launches; first visit opens at top; note pane shows
  numbered verses.

## Outcomes / deviations / lessons

- All six TDD cycles completed red → green. Full suite green: Data 20, Core 32, Presentation 98
  (150 total). `JesusTheChrist.App` (net10.0-android) builds with 0 errors.
- New `TopicPosition` model + `TopicPositionStore` mirror `NoteEntry`/`NoteStore`; table
  registered in `AppDatabase.InitializeAsync`. SQLite `CreateTableAsync` is additive, so no
  migration is needed for existing installs.
- `TopicFeedViewModel` keeps the resume logic testable (`ResumeCard`, `RecordVisible`,
  `SavePositionAsync`); the page only wires the `Scrolled` event, a debounced save, an
  entry-time `ScrollTo`, and an `OnDisappearing` flush. The live pointer is seeded from the
  saved position on load so leaving without scrolling re-persists the same reference.
- Note editor now carries the structured `Verses` list end-to-end (navigation → VM → XAML),
  retiring the plain-text `VerseText`/`NoteVerseTextParameter` path entirely.
- Deviation from the original (rejected) plan: the read-state "first-unread/last-read" anchor
  was dropped in favor of the persisted last-viewed pointer — one mechanism for both reader
  types, decided during brainstorming.
- **Still pending on-device verification** (no emulator; deploy via VS to a phone): resume
  within/across launches, first-visit-opens-at-top, and numbered verses in the note pane.
