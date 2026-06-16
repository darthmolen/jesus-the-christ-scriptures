# Phase: Plan 05 — Per-reference Notes

**Date:** 2026-06-07
**Roadmap:** `planning/roadmap_2026-06-07.md`
**Branch/PR:** `feature/plan-05-notes` (stacked on `feature/plan-06-settings`)

## Objective

A private, free-text note per reference (design spec §3.6). Each reference card shows whether a
note exists; tapping the note affordance opens an editor; saving persists via the existing
`NoteStore` (empty text deletes the note).

## Approach

- **Presentation (TDD):** add `INavigationService.GoBackAsync`; `NoteEditorViewModel`
  (LoadAsync(refId) → text; SaveCommand persists + navigates back; DeleteCommand). Extend
  `ReferenceCardViewModel` with `HasNote` + `OpenNoteCommand` (callback). `TopicFeedViewModel`
  gains `NoteStore` + `INavigationService`, sets each card's `HasNote`, opens the note route, and
  exposes `RefreshNotesAsync` so the feed reflects edits on return.
- **App (verified by launch):** `NoteEditorPage` (multiline editor, save/delete); a note button on
  each reference card; `note` route + DI registration. `TopicFeedPage` refreshes note flags on
  appearing.

## Success criteria

- Add/edit/delete a note; the card's note indicator reflects it on return.
- `dotnet build` clean (warnings-as-errors); `dotnet test` green (new note tests).
- PR opened (base = `feature/plan-06-settings`); owner merges Plan 06 then Plan 05.

## Outcomes

_(to be filled on completion)_
