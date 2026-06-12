# Phase: Note-editor split-pane redesign

**Date:** 2026-06-12
**Branch:** feature/note-editor-split-pane

## Objective
Replace the single-textbox note editor with a split, two-pane editor so a reader can
journal *with the verse visible* — no navigating back and forth.

- **Top pane** = the note (typed input). Top so it stays above the rising keyboard.
- **Bottom pane** = the scripture being annotated (target verse text), for reference.
- Both panes independently scrollable.
- Reference label (e.g. "Matt. 28:9") pinned so you always know which verse.

## Approach
The reference card already holds `RefLabel` and `VerseText`. Pass them through navigation
into the editor instead of doing a fragile id → corpus reverse lookup.

1. `NavigationRoutes` — add `NoteRefLabelParameter` ("refLabel") and `NoteVerseTextParameter` ("verseText").
2. `ReferenceCardViewModel` — change the `openNoteAsync` delegate to receive the card so the
   feed can read Id/RefLabel/VerseText; `OpenNoteAsync()` passes `this`.
3. `TopicFeedViewModel.OpenNoteAsync(card)` — navigate with refId + refLabel + verseText.
4. `NoteEditorViewModel` — add `ReferenceLabel`, `VerseText`, `HasVerse`; `LoadAsync` takes the
   label + verse (optional params keep existing call sites green).
5. `NoteEditorPage.xaml.cs` — read the new query params, pass them to `LoadAsync`.
6. `NoteEditorPage.xaml` — split layout: pinned ref label + actions header, note pane (Editor),
   scripture pane (scrollable verse), matching the feed's bordered-card visual language.

## Files expected to change
- src/JesusTheChrist.Presentation/Navigation/NavigationRoutes.cs
- src/JesusTheChrist.Presentation/ViewModels/ReferenceCardViewModel.cs
- src/JesusTheChrist.Presentation/ViewModels/TopicFeedViewModel.cs
- src/JesusTheChrist.Presentation/ViewModels/NoteEditorViewModel.cs
- src/JesusTheChrist.App/Views/NoteEditorPage.xaml(.cs)
- tests/.../NoteEditorViewModelTests.cs, TopicFeedViewModelTests.cs

## Success criteria
- Opening a note from a card shows that card's verse text and label in the scripture pane.
- Note pane stays usable with the keyboard up; both panes scroll independently.
- All Presentation tests green; Android build clean (0/0).

## Outcome
Done as planned. Passed the verse label + text through navigation (no corpus reverse
lookup). NoteEditorViewModel gained `ReferenceLabel`, `VerseText`, `HasVerse`; `LoadAsync`
takes optional label/verse so existing call sites stayed green. The page is now a pinned
ref-label + Save/Delete header, a bordered note Editor pane (top), a divider, and a tinted
bordered scripture pane (bottom) with its own pinned label over the scrollable verse.

- Presentation tests: 80/80 green (added 3: feed nav params, scripture-pane populated,
  no-verse-context case).
- Android Debug build clean (0/0); installed on device R5CY504DCKZ.
- Verified live on device: opening a note from Heb. 7:25 shows the verse in the scripture
  pane with the note input above it (`05-note-editor.png`).

### Deviations
- Used optional `LoadAsync` params instead of a new required overload — keeps the existing
  single-arg tests/call sites unchanged.

### Follow-ups
- Optional: recapture the note screenshot with sample note text for a richer store image.
- Remaining store captures (separate task): Settings (EN), full Spanish set, feed with
  read cards rolled up.
