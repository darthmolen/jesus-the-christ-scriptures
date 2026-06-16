# Backlog: copy-verse button on the reference card

**Date:** 2026-06-16
**Origin:** Owner request (2026-06-16 session)

## Idea

Add a **copy** control to the reference card — a button in the heading next to the **Note**
button — that copies the whole verse to the clipboard in one tap, so a reader can paste it
into a message, journal, lesson, etc.

## Work required

- **`ReferenceCardViewModel`**: add a `CopyCommand` (`[RelayCommand]`) that writes the verse
  to the clipboard via `Clipboard.Default.SetTextAsync(...)`.
- **What to copy:** the reference label + the verse text, e.g. `Heb. 7:25 — <verse text>`
  (decide whether to include the `RefLabel`; including it makes a paste self-identifying).
  The card already exposes `RefLabel` and `VerseText`.
- **`TopicFeedPage.xaml`**: add the button to the heading action row, next to the `+ Note`
  button (Grid columns `*,Auto,Auto` today → add a column). Use a short label or a copy glyph;
  add `SemanticProperties.Description` for accessibility.

## Notes

- **Placement vs. the collapsed/expanded action pattern.** Note + read currently live in the
  *heading while collapsed* and move to the *footer while expanded* (see
  `phase_card-inline-actions`). Decide whether copy follows the same pattern (consistent) or
  stays heading-only. Easiest consistent option: add a copy button to **both** the heading
  (gated on `IsCollapsed`) and the footer action bar, mirroring the note button.
- **Confirmation feedback** (a toast/snackbar "Copied") would need `CommunityToolkit.Maui`
  (the app currently references only `CommunityToolkit.Mvvm`). Optional — a silent copy is
  acceptable for v1; revisit if it feels unconfirmed on device.
- Pure view/VM concern; no data, ids, or read-marks involved.
