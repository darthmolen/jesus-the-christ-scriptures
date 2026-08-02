# Backlog: verse text truncates on-screen (Android FormattedString measurement bug)

**Date:** 2026-08-02
**Origin:** Owner report (2026-08-02 session) — 3 Nephi 20:41 rendered as
"…be ye clean that bear the vessels of" with "the Lord." missing on-screen, yet
hold-to-copy pasted the **full** verse.

## Summary (the "why")

**This is not a data problem — the bundled content is complete.** The source verse in
`src/JesusTheChrist.App/Resources/Raw/jesus-christ.en.json` reads in full:

> …be ye clean that bear the vessels of **the Lord.**

Copy/paste works because `CopyCommand` and the on-screen `Span` both bind to the *same*
`ContextLineViewModel.Text` string (loaded verbatim in `TopicalGuideLoader`). The clipboard
gets the whole string; the screen clips it. So the defect is purely in **how the `Label`
renders**, not in what it holds.

### Root cause

The verse line is a single `Label` whose `FormattedString` mixes **two font sizes**:

```xml
<!-- TopicFeedPage.xaml, VisibleVerses template (~line 188) -->
<Span Text="{Binding Verse}" FontAttributes="Bold" FontSize="{DynamicResource VerseNumberFontSize}" />
<Span Text="  " />
<Span Text="{Binding Text}" />   <!-- inherits the Label's ReadingFontSize (larger) -->
```

On Android, .NET MAUI measures a multi-span `Label`'s wrapped height from the wrong span's
line metrics (the smaller `VerseNumberFontSize` leading span). The text then lays out with the
larger reading font, needs more vertical space than was allocated, and the renderer **clips the
tail of the final wrapped line**. Short verses fit inside the under-measured box and look fine;
only long verses that wrap enough lines overflow and lose their last few words — exactly what
3 Nephi 20:41 does.

### Corroborating evidence (why we're confident)

The three verse templates behave exactly as this theory predicts:

| Template | File / line | Number-span font size | Truncates? |
|---|---|---|---|
| Primary reader `VisibleVerses` | `TopicFeedPage.xaml` ~188 | `VerseNumberFontSize` (**mixed**) | **Yes** (reported) |
| Note editor `Verses` | `NoteEditorPage.xaml` ~82 | `VerseNumberFontSize` (**mixed**) | **Yes** (same pattern, at risk) |
| Context window `Context` | `TopicFeedPage.xaml` ~321 | *(none — same size as text)* | **No** |

The only template that keeps a single font size is the only one that doesn't clip.

## "Are there more like this?"

- **In the data: no** — every verse string is stored complete; this never affects the clipboard.
- **On-screen: yes, potentially any long verse** that wraps to enough lines in the two
  **mixed-font-size** templates above (primary reader + note editor). It's not tied to specific
  references, so there is no finite "list of bad verses" to fix — it's one rendering pattern.
  The longest verses are the most exposed; 1 Nephi / Isaiah / 3 Nephi chapters have several.

## Recommended fix

Never mix font sizes inside one wrapping `Label`. Two options:

1. **Two-column layout (recommended, preserves the small superscript number).**
   Replace the single `FormattedText` `Label` with a `Grid` (or `HorizontalStackLayout`):
   column 0 = a small bold verse-number `Label`, column 1 = a plain **single-font** wrapping
   text `Label`. Single-font Labels don't hit the bug. This is the classic hanging-indent
   scripture layout. Apply to both `TopicFeedPage.xaml` (VisibleVerses) and `NoteEditorPage.xaml`.

2. **Drop the mixed size (smallest diff).** Remove `FontSize="{DynamicResource VerseNumberFontSize}"`
   from the number span so number and text share `ReadingFontSize` (proven safe by the context
   template). Lowest risk, but the verse number loses its smaller styling — a **design change**,
   so it needs owner sign-off.

## Verification

- Cannot be reproduced in the unit-test suite — it's an Android native-measurement defect, not
  view-model logic. Verify on a **physical/emulated Android device** with a known long verse
  (3 Nephi 20:41, 2 Nephi 8, Isaiah-heavy chapters) at the **largest** reading font size
  (worst case for wrapping).
- Regression guard idea: a small UI/screenshot test on Android CI, or at minimum a manual
  checklist entry for the longest verses at max font size.

## Affected files

- `src/JesusTheChrist.App/Views/TopicFeedPage.xaml` (VisibleVerses template, ~line 170–193)
- `src/JesusTheChrist.App/Views/NoteEditorPage.xaml` (Verses template, ~line 79–89)
- (No change needed in `TopicalGuideLoader`, `ContextVerse`, or the JSON content.)
