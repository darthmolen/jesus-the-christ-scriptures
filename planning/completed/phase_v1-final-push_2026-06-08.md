# Phase: v1 Final Push — Card Roll-Up + Spanish Invitation

**Date:** 2026-06-08
**Branch:** `feature/v1-final-push`

## Objective

Two pre-v1 items:

1. **Card roll-up.** When the user marks a reference read (the ✓), the card rolls up to just
   its heading, giving a sense of progression through the feed. Tapping the heading rolls it
   back out. The heading tap must not toggle the read state.
2. **Spanish invitation.** Provide `invitation.es.md` (the first-run Invitation) with the two
   referenced talks quoted from the Church's *official published Spanish*, not machine
   translation. The author's own framing paragraphs are left for the owner to translate.

## Approach

### Task 1 — roll-up
- `ReferenceCardViewModel`: added `IsExpanded` (default `true`), a `ChevronGlyph` computed
  property (`▾` expanded / `▸` collapsed, notified off `IsExpanded`), and `ToggleExpanded`.
  `ToggleReadAsync` now sets `IsExpanded = !isReadNext` — marking read collapses, un-reading
  re-expands.
- `TopicFeedPage.xaml`: the left heading area (gloss + ref + chevron) carries the
  `ToggleExpandedCommand` tap recognizer; the Note button and ✓ remain separate controls, so
  the heading tap can never fire read (no nested-gesture ambiguity). The verses + context
  toggle + context window are wrapped in a `VerticalStackLayout IsVisible="{Binding IsExpanded}"`.

### Task 2 — Spanish invitation
- **Andersen, "Hablamos de Cristo" (Oct 2020 GC)** — pulled verbatim from the local Spanish
  conference corpus at `lds-nl-scriptures/content/processed/conference/es/2020_10.json`
  (`/general-conference/2020/10/45andersen`, paragraphs 11–16). Footnote superscripts stripped.
- **Nelson, "Los profetas, el liderazgo y la ley divina" (Jan 8 2017 devotional)** — not in the
  local corpus (devotionals aren't archived there), so fetched from the Church site
  (`...prophets-leadership-and-divine-law?lang=spa`).
- Reference links point at `?lang=spa`. Author framing paragraphs are kept in English, each
  prefixed with a visible `**(ES pendiente)**` tag so the owner can find and translate them.

## Files Modified

- `src/JesusTheChrist.Presentation/ViewModels/ReferenceCardViewModel.cs`
- `src/JesusTheChrist.App/Views/TopicFeedPage.xaml`
- `tests/JesusTheChrist.Presentation.Tests/ViewModels/TopicFeedViewModelTests.cs` (+3 tests)
- `src/JesusTheChrist.App/Resources/Raw/invitation.es.md` (new)

## Outcome

- `dotnet test` (Presentation.Tests): **73 passed, 0 failed** — includes the 3 new roll-up tests.
- `dotnet build` MAUI head (`net10.0-android`): **0 warnings, 0 errors** (XAML source-gen compiled).

## Deviations / Notes

- The Spanish invitation's author paragraphs are intentionally left untranslated (owner will do
  their own prose); only the two official quotes were the research deliverable. The file is
  complete and renders; pending paragraphs are flagged `**(ES pendiente)**`.
- Per branch policy, work stops at "PR opened"; the owner (`darthmolen`) reviews and merges.
