# Backlog: verse text truncates on-screen (Android FormattedString measurement bug)

**Date:** 2026-08-02
**Origin:** Owner report (2026-08-02 session) — 3 Nephi 20:41 rendered as
"…be ye clean that bear the vessels of" with "the Lord." missing on-screen, yet
hold-to-copy pasted the **full** verse. A second example, 3 Nephi 20:46 ("…land of their
inheritance." → "inheritance." clipped), confirmed the pattern.

## STATUS: fix applied on branch `claude/incomplete-scriptures-english-fejdke`, awaiting on-device verification

Refined root cause and the fix are below. The original suspicion (mixing `VerseNumberFontSize`
with `ReadingFontSize`) turned out to be a **co-varying red herring** — the two truncating
templates also both set `LineHeight`, which is the actual, well-documented Android trigger.
The second example ("the *final* line after word-wrap gets cut off") is the `LineHeight`
last-line-clip signature.

## Summary (the "why")

**This is not a data problem — the bundled content is complete.** The source verse in
`src/JesusTheChrist.App/Resources/Raw/jesus-christ.en.json` reads in full:

> …be ye clean that bear the vessels of **the Lord.**

Copy/paste works because `CopyCommand` and the on-screen `Span` both bind to the *same*
`ContextLineViewModel.Text` string (loaded verbatim in `TopicalGuideLoader`). The clipboard
gets the whole string; the screen clips it. So the defect is purely in **how the `Label`
renders**, not in what it holds.

### Root cause

The verse line is a `Label` with `LineHeight="1.35"`. On Android, a `Label` whose
`LineHeight` > 1 and that word-wraps to multiple lines **clips the tail of its last wrapped
line**: MAUI adds the extra inter-line spacing natively but under-measures the view's height by
that accumulated amount, so the final line overflows the allocated box and is cut. The deficit
grows with the number of wrapped lines, which is why only **long** verses lose text, and why the
clipped part is always at the very **end** (3 Nephi 20:41 "the Lord.", 3 Nephi 20:46
"inheritance.").

### Corroborating evidence (why we're confident)

The three verse templates behave exactly as this theory predicts:

| Template | File / line | `LineHeight` | Truncates? |
|---|---|---|---|
| Primary reader `VisibleVerses` | `TopicFeedPage.xaml` ~188 | `1.35` | **Yes** (reported ×2) |
| Note editor `Verses` | `NoteEditorPage.xaml` ~82 | `1.4` | **Yes** (same pattern, at risk) |
| Context window `Context` | `TopicFeedPage.xaml` ~321 | *(none)* | **No** |

The only template with **no `LineHeight`** is the only one that doesn't clip — regardless of the
mixed font size, which is present in the two truncating templates but is not the trigger.

## "Are there more like this?"

- **In the data: no** — every verse string is stored complete; this never affects the clipboard.
- **On-screen: yes, potentially any long verse** that wraps to enough lines in the two
  **mixed-font-size** templates above (primary reader + note editor). It's not tied to specific
  references, so there is no finite "list of bad verses" to fix — it's one rendering pattern.
  The longest verses are the most exposed; 1 Nephi / Isaiah / 3 Nephi chapters have several.

## Fix applied

Owner chose to **preserve the current look** (inline small verse number, wrapped lines return to
the left margin). A two-column layout can't produce that — its wrapped lines hang-indent under the
text column — so the single-`Label` layout is kept and the actual trigger is removed instead:

Two changes, both toward the safest possible layout — the verse line now matches our own
context-window template (which never truncated) **and** the Gospel Library app's own rendering
(bold, same-size, inline verse numbers), which the owner asked us to mirror:

**1. Removed `LineHeight`** from the wrapping verse/paragraph labels (the actual trigger) so the
native measurement matches and the last line renders complete:

- `TopicFeedPage.xaml` — VisibleVerses verse-line `Label`: dropped `LineHeight="1.35"`.
- `NoteEditorPage.xaml` — Verses verse-line `Label`: dropped `LineHeight="1.4"`.
- `Controls/MarkdownView.cs` — `BuildLabel`: dropped `LineHeight = 1.35` (same bug class for long
  wrapping markdown paragraphs on the invitation/topic pages).

**2. Made verse numbers bold + same font size** as the body text (Gospel Library style),
removing the last remaining reason a verse `Label` mixed metrics:

- `TopicFeedPage.xaml` / `NoteEditorPage.xaml` verse-number `Span`s: dropped
  `FontSize="{DynamicResource VerseNumberFontSize}"`, keeping `FontAttributes="Bold"`.
- Removed the now-dead plumbing: `VerseNumberFontSize` key in `App.xaml`, and
  `VerseNumberFontSizeKey` / `VerseNumberRatio` and their assignment in `AppearanceApplier`.

Net result: all three verse templates are now identical (bold same-size number, no `LineHeight`).

If the tighter default line spacing reads too cramped on-device versus the airier Gospel Library
look, restore spacing via an approach that doesn't hit the native measure bug (inter-item
`Spacing` / paragraph padding), **not** `LineHeight`.

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
