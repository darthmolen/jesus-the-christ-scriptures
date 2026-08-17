# v1.0.7 — three reader-facing features

## Context

`planning/08-16-2026-link-each-reference-to-lds-org.md` captures three owner requests from
reading on the phone. All three are friction the reader hits in the topic feed:

1. **No escape hatch to study further.** A reference label is inert. On Android an
   `https://www.churchofjesuschrist.org/…` link opens **Gospel Library** via App Links when
   installed, so a tap could hand the reader footnotes, cross-references, and their own
   Church-account notes — falling back to the browser otherwise.
2. **"Show scriptural context" strands the reader.** The toggle sits in the footer action bar,
   which renders *above* the context window it opens. After reading the context you must scroll
   back up to close it.
3. **Long multi-chapter references lose your place.** 3 Ne. 11–26 (445 verses, 16 chapters)
   always reopens at chapter 11 with everything else collapsed, so a reader working through it
   over days re-hunts their chapter every session.

Outcome: v1.0.7 ships all three, as three **sequenced** feature branches → three PRs into `main`.
They share view-model surface, so they are stacked deliberately rather than run in parallel — see
**Order** at the end. The `ApplicationDisplayVersion` bump lands in a separate release PR after
all three merge, the pattern used for 1.0.6.

---

## Feature 1 — Reference header links to churchofjesuschrist.org

Branch `feature/reference-deep-link`. Retires `planning/backlog/title-deeplink-to-lds.md`.

### Design

A discrete link-coloured **↗** glyph sits beside the reference label. `RefLabel` keeps its
existing expand/collapse tap, so no reader loses the collapse gesture or lands in a browser by
mis-tap. **Cross-chapter cards get a ↗ on each chapter header too**, opening that chapter — which
pairs with feature 3, where a reader lives inside one chapter at a time.

### Core — `ScriptureUrlBuilder` (new, `src/JesusTheChrist.Core/Models/ScriptureUrlBuilder.cs`)

Port of the Python reference in [docs/DEEP-LINKING-TO-LDS-WEBSITE.md](docs/DEEP-LINKING-TO-LDS-WEBSITE.md).
URL shape: `…/study/scriptures/{volume}/{book}/{chapter}?lang={eng|spa}&id={ids}#p{first}`.

The port is small because the corpus already carries church codes. **Verified: every `book`
value in `jesus-christ.en.json` already equals the church book code** (`heb`, `1-jn`, `w-of-m`,
`moro`, `js-h`, `dc`, …) and matches the doc's table exactly — so **no book override map is
needed**, only a volume map. This was the backlog's one open unknown; it is now closed.

- `VolumeExtensions.SiteCode()` — new sibling of the existing `Parse`/`IsBible`/`Order` in
  [VolumeExtensions.cs](src/JesusTheChrist.Core/Models/VolumeExtensions.cs):
  `OldTestament→ot`, `NewTestament→nt`, `BookOfMormon→bofm`,
  `DoctrineAndCovenants→dc-testament`, `PearlOfGreatPrice→pgp`.
- `LanguageExtensions.SiteCode()` — `En→eng`, `Es→spa`, beside the existing `Code()` in
  [LanguageExtensions.cs](src/JesusTheChrist.Core/Models/LanguageExtensions.cs). Path segments
  are identical across languages; only `?lang=` differs.
- Verse ids: single `p16`; a contiguous ascending run `p7-p8`; otherwise a comma list
  `p7,p9,p11`. Anchor is always `#p{first}`. Empty verse list → chapter URL with neither.
- D&C needs no special case: `Vol.SiteCode()` gives `dc-testament` and `Book` is already `dc`.

Build the card's URL from **`reference.TargetSegments()[0]`** (chapter + its target verse
numbers) rather than `Ch`/`Verses`. For a single-chapter reference the two are identical, and
for a spanning reference it correctly yields the first chapter and only that chapter's verses —
one code path, no branch.

### Presentation

- New `ILinkOpener { Task OpenAsync(string url); }` in `src/JesusTheChrist.Presentation/Platform/`,
  mirroring [IClipboardService.cs](src/JesusTheChrist.Presentation/Platform/IClipboardService.cs)
  exactly — Presentation has no MAUI reference, so platform capability must arrive through a seam.
- [ReferenceCardViewModel.cs](src/JesusTheChrist.Presentation/ViewModels/ReferenceCardViewModel.cs):
  `string StudyUrl` (ctor-supplied) + `[RelayCommand] OpenStudyAsync()`. Reuse the existing
  `Func<…> delegate` ctor idiom rather than injecting a service.
- [ChapterSegmentViewModel.cs](src/JesusTheChrist.Presentation/ViewModels/ChapterSegmentViewModel.cs):
  same pair. Segment counts are tiny (16 max in the whole corpus), so a `RelayCommand` per
  segment is cheap and consistent with its existing `ToggleExpandedCommand`.
- [TopicFeedViewModel.cs](src/JesusTheChrist.Presentation/ViewModels/TopicFeedViewModel.cs):
  hoist `this.openLinkAsync = links.OpenAsync` into a field beside `copyAsync` (same reason —
  a field receiver defeats the compiler's method-group caching, so one delegate is shared by
  every card). `LoadAsync` already resolves `Language`, so URLs are baked in at construction.

### App

- New `src/JesusTheChrist.App/Services/MauiLinkOpener.cs` → `Launcher.Default.OpenAsync(uri)`.
  Register singleton in [MauiProgram.cs](src/JesusTheChrist.App/MauiProgram.cs) beside
  `IClipboardService`.
- **Failure is always a silent no-op** — a reader who taps ↗ must never get a crash:
  - Malformed URL → the `Uri.TryCreate(url, UriKind.Absolute, out var uri)` guard from
    [MarkdownView.cs:77-83](src/JesusTheChrist.App/Controls/MarkdownView.cs#L77-L83) returns early.
  - No handler → `OpenAsync` returns `false`; nothing happens.
  - Launcher throws → caught and swallowed.
  Note this **awaits** rather than copying MarkdownView's older `_ = Launcher…` fire-and-forget,
  which would leave an unobserved faulted task. This follows the idiom the hold-to-copy Copilot
  round already established for `CopyVerseAsync` — await, contain exceptions, and record the
  `CA1031` entry in `GlobalSuppressions.cs` (this repo keeps suppressions there, not inline).
- [TopicFeedPage.xaml](src/JesusTheChrist.App/Views/TopicFeedPage.xaml): add the ↗ `Label` to
  the header `HorizontalStackLayout` (lines 47–59) and to the chapter-header `Grid` (144–166,
  gated on `ShowHeader`). Link colour `#4D8DFF` matching MarkdownView; `Padding` sized for a
  ≥44px touch target; its own `TapGestureRecognizer`.

### Risk

The ↗ sits inside a `VerticalStackLayout` that already carries a collapse `TapGestureRecognizer`
(xaml:39–41). A child *view* recognizer takes precedence over an ancestor's in MAUI — the
Android flakiness noted at MarkdownView.cs:58–61 concerns per-`Span` taps, not per-`View`. If
device testing shows the ↗ tap falling through to collapse, the fallback is to lift the
`HorizontalStackLayout` (chevron + label + ↗) out of the tapped stack and leave collapse on the
gloss line and chevron.

### Strings

`A11yOpenOnChurchSite` — en "Open on churchofjesuschrist.org" / es "Abrir en
churchofjesuschrist.org". Add to `AppResources.resx`, `AppResources.es.resx`, **and the
hand-maintained `AppResources.Designer.cs`** (its header says keep keys in sync manually).

---

## Feature 2 — Context toggle travels to the bottom of the card

Branch `feature/context-bar-at-bottom`. **XAML-only; no view-model, string, or data change.**

The card body order today is: segments → separator → footer action bar → context window. The
bar therefore sits above the thing it reveals. Reorder to:

```
segments (target verses, per chapter)
[separator]   ← IsVisible="{Binding IsContextVisible}"
[context window]
separator     ← existing, always shown
footer action bar   (Show/Hide context · Copy · + Note · ✓)   ← now always last
```

Concretely in [TopicFeedPage.xaml](src/JesusTheChrist.App/Views/TopicFeedPage.xaml): move the
Context window block (296–329) up to sit immediately after the Segments stack (ends 201), and
prepend a second `BoxView` separator bound to `IsContextVisible`. Copy the separator verbatim
from lines 204–207 (`HeightRequest="1" Margin="0,4"`,
`Color="{AppThemeBinding Light=#E0E0E0, Dark=#444444}"`) — the same inline pattern is already
duplicated in [NoteEditorPage.xaml:55-58](src/JesusTheChrist.App/Views/NoteEditorPage.xaml#L55-L58),
so a third inline copy is consistent with the tree; there is no keyed separator style to reuse.

This satisfies both halves of the request: the bar lands at the true bottom of the card, and a
divider separates the original passage from the contextual read. The toggle button already flips
its own text between `CardShowContext` / `CardHideContext`, so no "Done" string is needed.

**Not changed, deliberately:** the target verses render twice while context is open — once in
`Segments` at reading font size, once bolded inside the ± window. The request explicitly asks to
*separate* the original from the contextual read, which presumes both stay. The new separator is
what makes that duplication read as intentional.

**Device-verification item:** collapsing the context now pulls the footer bar *upward*, away from
the finger that tapped it, leaving the reader looking at the next card. That is probably fine
(they just finished). If it reads badly, the fix is the existing re-anchor seam — the
`CardCollapsedAfterRead` → `ScrollTo` path in
[TopicFeedPage.xaml.cs:366-384](src/JesusTheChrist.App/Views/TopicFeedPage.xaml.cs#L366-L384) —
not a new mechanism. Left unbuilt rather than pre-solving a hypothetical.

---

## Feature 3 — Remember the chapter inside a long multi-chapter reference

Branch `feature/remember-span-chapter`.

### Why it resets today

`TopicFeedViewModel.BuildSegments` (lines 187–204) sets
`var isExpanded = !spans || i == 0 || expandAll;` — the first chapter is *always* expanded and
nothing else is, with no memory anywhere. Both the page and the view model are `AddTransient`
([MauiProgram.cs:80-81](src/JesusTheChrist.App/MauiProgram.cs#L80-L81)), so every navigation
rebuilds `Segments` from scratch.

### Design

Accordion + across-launch SQLite memory, applied **only to long spans**:

- `UsesChapterMemory` = `SpansChapters && !expandAll`. Matt 9:35–11:1 (47 verses) stays under
  the existing `EagerVerseLimit = 60` and keeps opening all three chapters at once — forcing an
  accordion on a short passage would be a regression. Matt 5–7 (111 v) and 3 Ne 11–26 (445 v)
  get the new behaviour.
- Opening a chapter collapses its siblings, so **at most one** is open. Not *exactly* one:
  `ChapterSegmentViewModel.ToggleExpanded` is a true toggle, so tapping the already-open chapter
  collapses it and leaves none open. That is kept deliberately — on a 16-chapter card it turns
  the card into a chapter index, which is the only cheap way to jump a long span.
- The saved chapter is written **on expand only, and never cleared on collapse**. So a reader who
  collapses everything and leaves still returns to the chapter they were last reading.
- On the next visit that chapter alone is expanded; everything before and after stays minimized,
  exactly as asked.

### Data (new)

- `src/JesusTheChrist.Data/ChapterPosition.cs` — `[PrimaryKey] string RefId`, `int Ch`,
  `DateTime UpdatedAtUtc`. Keyed by `Reference.Id(subTopicKey)`
  ([Reference.cs:109](src/JesusTheChrist.Core/Models/Reference.cs#L109)), which is already
  stable and language-invariant.
- `src/JesusTheChrist.Data/ChapterPositionStore.cs` — a near-clone of
  [TopicPositionStore.cs](src/JesusTheChrist.Data/TopicPositionStore.cs): `SaveAsync(refId, ch)`
  via `InsertOrReplaceAsync`, `GetAsync(refId)` → `int?`, injectable `Func<DateTime>` clock, plus
  a bulk **`GetAllAsync()` → `Dictionary<string,int>`** so `LoadAsync` does one query rather than
  one per card (mirrors `readMarks.GetReadIdsAsync()` / `notes.GetNoteIdsAsync()`). Under the
  `UsesChapterMemory` rule the table holds **at most two rows** on the current corpus — Matt 5–7
  and 3 Ne 11–26; Matt 9:35–11:1 is 47 verses so it never persists a chapter. Reference ids are
  language-invariant, so Spanish adds no rows.
- Register `CreateTableAsync<ChapterPosition>()` in
  [AppDatabase.InitializeAsync](src/JesusTheChrist.Data/AppDatabase.cs#L29-L35) — additive, so
  no migration for existing installs. Register the store in `MauiProgram`.

### Presentation

- `ChapterSegmentViewModel` gains `int Ch` (Core's `ChapterSegment` already carries it; the view
  model currently keeps only `ChapterLabel`) and an `Action<ChapterSegmentViewModel>? onExpanded`
  callback raised from `ToggleExpanded`.
- `ReferenceCardViewModel` owns the accordion — segments hold no reference to their siblings, and
  the card is the only thing that sees all of them. On `onExpanded`, collapse every other segment
  and call a `Func<string,int,Task> saveChapterAsync(this.Id, segment.Ch)` delegate (same ctor
  delegate idiom as `copyAsync` / `copyVerseAsync`). Inert when the card does not use chapter memory.
- `TopicFeedViewModel.BuildSegments` takes the saved chapter and picks the one expanded segment:
  the saved chapter when it matches a segment, otherwise the first. `LoadAsync` reads
  `GetAllAsync()` once alongside `readIds` / `noteIds`; `saveChapterAsync` is hoisted to a field
  beside `copyAsync`.

### Scroll

`CollectionView.ScrollTo` can only target a **card**, not a segment inside its `DataTemplate`, so
there is no way to scroll a chapter header into view without a new measurement seam. It is not
needed: with the accordion only one chapter is realized, the card is short, and the existing
resume-position scroll (`ScrollTo(resume, ScrollToPosition.Start, animate: false)`,
[TopicFeedPage.xaml.cs:152-173](src/JesusTheChrist.App/Views/TopicFeedPage.xaml.cs#L152-L173))
already puts the card's top at the viewport top — which is where the open chapter now is.

---

## Verification

**Automated** — `dotnet build JesusTheChrist.slnx` clean (strict CPM, warnings-as-errors,
`AnalysisMode=all`, StyleCop) and `dotnet test` green on all three projects. Current baseline
is 188 tests (Core 44, Data 20, Presentation 124). New coverage:

- **Core** `ScriptureUrlBuilderTests` — every worked example in the doc (John 3:16, 1 Ne 3:7,
  Alma 32:21 `spa`, D&C 121:7–8 as a range, Moses 1:39, Helaman 5:12), plus a discrete verse
  list, an empty verse list (chapter-only URL, no `id`/anchor), and all five volume codes.
- **Data** `ChapterPositionStoreTests` — mirror the `TopicPositionStore` suite: null when empty,
  save/get, overwrite, independence across reference ids, and `GetAllAsync` shape.
- **Presentation** — card `StudyUrl` for single- and cross-chapter refs; `OpenStudyCommand`
  reaches a new `FakeLinkOpener` (beside the existing `FakeClipboard`); per-chapter segment URLs;
  accordion (expanding one collapses the siblings); the save delegate receives `(refId, ch)`;
  `BuildSegments` restores a saved chapter and falls back to the first when it no longer matches;
  a ≤60-verse span still opens all chapters and does **not** accordion.
- **Resources** — `A11yOpenOnChurchSite` asserted in `AppResourcesTests` for en and es.

XAML has no automated coverage by design (the App project has no test project), so feature 2 and
every gesture are carried entirely by the device checklist.

**On the physical Android phone** (no emulator available):

1. Tap ↗ on a card header → Gospel Library opens at the right verse, highlighted; uninstalling
   Gospel Library falls back to the browser. Confirm the ↗ tap does **not** collapse the card and
   a tap on the label still does.
2. On a cross-chapter card, tap ↗ on a **chapter header** → that chapter opens on the site, and
   the segment does **not** expand or collapse. Same nested-gesture risk as the card header, since
   that header already carries `ToggleExpandedCommand` — verify it separately, not by inference.
3. Spanish → the same link carries `?lang=spa` and lands on the Spanish text.
4. D&C and Pearl of Great Price references resolve (`dc-testament/dc/121`, `pgp/moses/1`).
5. Airplane mode with Gospel Library uninstalled → tapping ↗ fails quietly, no crash, no dialog.
6. Open "Show scriptural context" on a long card → the toggle now sits below the context, with a
   divider separating the original passage from the contextual read; tapping Hide needs no scroll.
7. Open 3 Ne. 11–26, expand chapter 18 → chapter 11 closes. Leave the topic, force-stop the app,
   relaunch, reopen → chapter 18 alone is open and the card is at the top of the viewport.
8. On that card, tap the open chapter 18 header → it collapses, leaving a bare chapter index.
   Leave and return → chapter 18 is open again (collapse must not clear the memory).
9. Open Matt 9:35–11:1 → all three chapters still open together, no accordion.
10. Both themes: the ↗ and the new separator are legible in light and dark.
11. TalkBack announces the ↗ from `A11yOpenOnChurchSite`.

## Planning artifacts & delivery

### Order — the branches are *not* independent

Ship in this order, each rebased onto the previous:

| # | Branch | Why here |
|---|---|---|
| 1 | `feature/context-bar-at-bottom` | XAML-only, touches lines 201–207 / 296–329 of `TopicFeedPage.xaml`, which no other feature goes near. Smallest possible first merge. |
| 2 | `feature/reference-deep-link` | Lands the shared groundwork: `ChapterSegmentViewModel.Ch`, the widened card/segment constructors, the `MauiProgram` seam. Its XAML regions (47–59, 144–166) don't overlap feature 2's. |
| 3 | `feature/remember-span-chapter` | Largest, and rebases onto #2 rather than fighting it. |

Features 1 and 3 **both** add a constructor parameter to `ChapterSegmentViewModel`, both add a
delegate to `ReferenceCardViewModel`, both change `TopicFeedViewModel.BuildSegments` and the
`MauiProgram` registrations, and both need `Ch` on the segment view model. Landing that surface
once in #2 turns three-way conflicts into one ordinary rebase.

### Artifacts

This document is copied to `planning/in_progress/2026_08_16_feature_v1_0_7_three_features.md` and
serves as the phase document for all three branches — it already carries the objective, approach,
files, and success criteria `CLAUDE.md` asks for, and splitting it three ways would only duplicate
the shared design. It moves to `planning/completed/` with outcomes once the third PR opens.

`CLAUDE.md`'s Planning Protocol still specifies `phase_[name]_[date].md`, but the repo migrated to
date-first months ago — `2026_07_27_feature_card_copy_button.md`,
`2026_07_27_feature_hold_to_copy_verse.md`, `2026_06_27_feature_cross-chapter-references.md`. The
convention here follows the tree, not the stale doc; **correcting that line in `CLAUDE.md` rides
along on branch #1** so the next plan isn't a coin flip.

Delete `planning/backlog/title-deeplink-to-lds.md` as part of feature 1 — it is fully absorbed here.

Three PRs into `main`, owner-only merge. The 1.0.6 → 1.0.7 / versionCode 7 → 8 bump ships in a
separate release PR after all three merge.
