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

---

## Outcome

**Status:** All three features implemented, reviewed, and merged to `main` on 2026-08-16; a
fourth PR followed on 2026-08-17 from UAT (see below). Shipped as **1.0.7 / versionCode 8**.

| PR | Feature | Merge |
|----|---------|-------|
| [#50](https://github.com/darthmolen/jesus-the-christ-scriptures/pull/50) | Context toggle at the card bottom | `84874a0` |
| [#51](https://github.com/darthmolen/jesus-the-christ-scriptures/pull/51) | Reference deep links | `d1ea376` |
| [#52](https://github.com/darthmolen/jesus-the-christ-scriptures/pull/52) | Remembered chapter in long spans | `33eee46` |
| [#54](https://github.com/darthmolen/jesus-the-christ-scriptures/pull/54) | Chapter strip (UAT fix) | see UAT round 1 |

**236 tests** green (Core 65, Data 26, Presentation 145), up from 188 — 229 after the three
features, plus seven from the UAT fix. Build clean at 0 warnings under strict CPM and
warnings-as-errors, including the Android XAML compile.

### Deviations from plan

1. **`Uri` rather than `string` for study links.** CA1054/CA1056 rejected string URLs on public
   surface. The analyzer improved the design: `StudyUri` is nullable, so a URL that will not parse
   hides the link through `HasStudyLink` instead of throwing during feed load. Core's builder still
   returns `string`; the single conversion happens in `TopicFeedViewModel.StudyUri`.
2. **The chapter toggle became an async command.** Persisting on expand needs an await, and
   `ToggleReadAsync` was already the repo's idiom for a toggle that writes to a store.
3. **`MauiLinkOpener` awaits rather than copying `MarkdownView`'s fire-and-forget.** `_ = Launcher…`
   would leave an unobserved faulted task; the awaited-and-contained shape matches `CopyVerseAsync`
   from the hold-to-copy phase, with the `CA1031` entry in `GlobalSuppressions.cs`.
4. **The feed fixture needed a generated long span.** Chapter memory does not engage below the
   60-verse threshold, and the existing Matt 9:35–11:1 fixture is 47 verses, so the accordion tests
   could not have been written against it. `BuildGuide()` splices in a 75-verse span.
5. **`CLAUDE.md` was corrected in two places, not one.** The Copilot review found the commit-message
   template still carried `phase_[name]_[date].md` after the Pre-Task line had been updated.

### Copilot review

Five comments across the three PRs, all verified against the code and all accepted — two of them
defects introduced by this work:

- **`BuildSegments` lost its doc comment** (#51). The summary was left stranded above `StudyUri`,
  giving that method two `<summary>` blocks and `BuildSegments` none.
- **`TargetSegments()` was recomputed once per chapter** (#51). `ScriptureUrlBuilder.Build(Reference,
  Language, int)` re-groups internally, so 3 Ne 11–26 regrouped all 445 verses sixteen times per
  feed load — and every card is built up front. Fixed with a `ChapterSegment` overload, pinned to
  the chapter-number overload by a test.
- **The target-verse count was computed twice** (#52), in `UsesChapterMemory` and again as
  `expandAll`. `LoadAsync` now groups once and derives both; three passes per reference became one.
- Two documentation-accuracy points on #50 (the context-window comment and the `CLAUDE.md`
  commit template).

### Lessons

- **Do not pass `--delete-branch` when merging a stacked PR.** Deleting `feature/context-bar-at-bottom`
  auto-**closed** #51, which targeted it, and a closed PR cannot be retargeted or reopened while its
  base is missing. Recovery was to push the branch back, reopen, retarget to `main`, merge, then
  delete. Merge stacked PRs first and delete branches last.
- GitHub did not auto-retarget the child PR here, contrary to the usual behaviour — most likely
  because the branch deletion landed in the same operation as the merge. Retarget explicitly.
- A review that flags a "redundant computation" in a feed that builds every card up front is worth
  taking seriously rather than filing as a nit; the cost scales with the largest passage in the corpus.

### Remaining

**On-device UAT by the owner** for the combined 1.0.7 build — round 1 is recorded below; this is
round 2, covering the chapter strip and a re-check of the three original features. The device checklist above is the whole
safety net for the XAML and gesture work, which has no automated coverage by design. The highest-risk
item is the ↗ tap target: it sits inside containers that already carry their own tap recognizers, on
both the card header and the chapter header.

---

## UAT round 1 (2026-08-17)

Owner exercised the merged 1.0.7 build on the phone and reported two things.

### 1. A chapter opened at the bottom of the viewport — fixed

**Mine.** The plan asserted no chapter-level scroll seam was needed, reasoning that "with the
accordion only one chapter is realized, so the card is short." That ignored the chapter *index*:
one stacked header per closed chapter still sat above the open one — fifteen of them above chapter
26, roughly 450du — and nothing re-anchored the feed when the previously open chapter collapsed.
The same class of bug as `phase_feed-scroll-on-collapse_2026-06-16`.

The owner's question — *what are the primitives driving the chapters, and why is only the card
available for positioning?* — reframed it. `BindableLayout` is not an `ItemsView`, so chapters have
no index for `ScrollTo` to resolve; a card is one opaque row to the scrolling machinery. The fix
therefore removes the need to scroll inside a card rather than fighting for the ability to:

- Long spans list chapters as a **compact wrapped strip of numbers**, open one filled.
- A closed chapter draws nothing (`ChapterSegmentViewModel.IsRendered`); the strip replaces its header.
- Opening one raises `TopicFeedViewModel.ChapterExpanded`; the page re-anchors via the existing
  `ScrollTo(card, Start)` seam.

Everything above the open chapter drops from ~450du to ~110du, so anchoring the card lands the
chapter effectively at the top. Short spans and single-chapter references are untouched.

Recorded as **[ADR 0001](../../docs/adr/0001-chapter-navigation-inside-a-reference-card.md)** at the
owner's request — the first ADR in the repo, with `docs/adr/README.md` establishing the practice.
The grouped-`CollectionView` alternative is documented there with its revisit triggers, the chief
one being "the app grows past a Topical Guide scroller."

### 2. A re-opened read card closes again on return — deferred

**Pre-existing**, from `phase_card-inline-actions_2026-06-14`, not a 1.0.7 regression.
`ReferenceCardViewModel` derives `IsExpanded` from read state at construction and never persists a
manual re-open, while the page and view model are transient.

Deferred to `planning/backlog/2026_08_17_feature_restore_reading_state.md` at the owner's request,
carrying the decision that Reading Mode and Checklist Mode are the same thing — a topic should come
back the way it was left, regardless of read state.

**Tests:** 236 green (Core 65, Data 26, Presentation 145), up from 229. Build clean, 0 warnings.
Seven new Presentation tests cover the strip, the render gating, the collapse-to-index case, and
that re-anchoring fires on open but not on close.

### UAT round 2 — the strip was one-way

The owner caught what the first pass missed: the strip is only reachable from the top of the card.
After reading a chapter the reader sits dozens of verses below it, so moving to the next chapter
meant scrolling back over everything they had just read — the tedium the strip existed to remove,
reintroduced at every chapter boundary. The strip solved entering a chapter and did nothing about
leaving one.

Fixed with a **"↑ To top"** link at the end of each open chapter, on strip cards only, scrolling back
to the card top where the strip lives. Deliberately a pure view concern: the handler walks up from
the tapped label with the existing `CardFor` helper, exactly as a held verse finds its card, so no
view model learns that scrolling exists. No view-model change at all — `ChapterSegmentViewModel.InStrip`
already gates it, and because a segment only renders while expanded, the link appears exactly when a
chapter is open and never otherwise.

**Lesson:** the strip was designed for the *entry* half of the interaction and reviewed the same way.
A navigation affordance needs its return path designed at the same time, or it is one-way by
construction. Worth checking for on any future in-place swap — see ADR 0001.
