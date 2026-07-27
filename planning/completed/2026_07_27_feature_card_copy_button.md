# Phase: Copy button on the reference card

**Date:** 2026-07-27
**Branch:** `feature/card-copy-button`
**PR:** _(pending)_
**Status:** In progress
**Backlog item:** `planning/backlog/copy-verse-button.md` (owner request, 2026-06-16) — **partially**
satisfied by this phase; the single-verse hold gesture is phase 2, so the backlog file stays until then.

## Context

The reference card offers only two actions today, note and read. A reader who wants a verse in a
message, a journal, or a lesson has to retype it. The backlog asks for a one-tap **Copy** control in
the card's action row.

The backlog spec proposes calling `Clipboard.Default.SetTextAsync(...)` straight from the view model.
That does not compile: `JesusTheChrist.Presentation` is a plain `net10.0` library with no
`Microsoft.Maui.Controls` reference, which is exactly what lets the view models be unit-tested on a
desktop host. So this phase adds a platform seam in the shape of the three that already exist
(`INavigationService`, `IAppearanceApplier`, `ILanguagePreference`).

## Objective / success criteria

- A **Copy** button in the card heading (while collapsed) and in the footer action bar (while
  expanded), mirroring the note button's placement pattern from `phase_card-inline-actions`.
- Tapping it puts the reference label plus the numbered target verses on the clipboard, including
  chapter headers and the verses of chapters that are collapsed on screen.
- The button confirms in place: its label flips to `✓ Copied` for two seconds, then reverts.
- Localized in English and Spanish; the button carries a semantic description for TalkBack.
- No new NuGet package. No eager per-card or per-verse allocation at feed load.
- Solution builds clean under strict CPM + warnings-as-errors; all three test projects green.

## Decisions

| Question | Decision |
|---|---|
| What is copied | `RefLabel`, then one `{verse} {text}` line per target verse; chapter-label lines only on cross-chapter refs |
| Source of the text | `Segments` — carries chapter labels, excludes ±context verses, and includes chapters collapsed on screen |
| Line separator | Literal `"\n"`, not `Environment.NewLine`, so the value is deterministic across the test host and Android |
| Placement | Heading (gated on `IsCollapsed`) **and** footer, mirroring `+ Note` |
| Feedback | In-place label flip. A toast would need `CommunityToolkit.Maui`; phase 2 adds an in-page overlay instead |
| Seam shape | `IClipboardService` in Presentation, `MauiClipboardService` in App, registered singleton |
| How the card reaches it | A `Func<string, Task>` ctor delegate, matching the card's three existing delegates — keeps the card a POCO and lets tests drive it without MAUI |

### Performance notes (raised by the owner during planning)

Cards are **not** built on scroll: `LoadAsync` constructs the whole `References` collection up front,
because `CollectionView` virtualizes views, not the bound list. So anything built in the card
constructor is paid for every reference in the sub-topic at load.

- `CopyText` is therefore **lazy-cached** (`this.copyText ??= this.BuildCopyText()`), not built in the
  constructor. A cross-chapter card can hold hundreds of verses; joining them at load for cards nobody
  copies is pure waste.
- The clipboard delegate is **hoisted into a field** on `TopicFeedViewModel` and shared by every card.
  Its receiver is a field (`this.clipboard.SetTextAsync`), so unlike the existing `this.SetReadAsync`
  method groups it is not a candidate for the compiler's method-group caching and would otherwise
  allocate one delegate per card.
- The delay delegate is a `private static readonly` — one instance for the app's lifetime.

## Files expected to change

**New**
- `src/JesusTheChrist.Presentation/Platform/IClipboardService.cs`
- `src/JesusTheChrist.App/Services/MauiClipboardService.cs`
- `tests/JesusTheChrist.Presentation.Tests/ViewModels/ReferenceCardViewModelTests.cs`
- `tests/JesusTheChrist.Presentation.Tests/Fakes/FakeClipboard.cs`

**Modified**
- `src/JesusTheChrist.Presentation/ViewModels/ReferenceCardViewModel.cs`
- `src/JesusTheChrist.Presentation/ViewModels/TopicFeedViewModel.cs`
- `src/JesusTheChrist.App/MauiProgram.cs`
- `src/JesusTheChrist.App/Views/TopicFeedPage.xaml`
- `src/JesusTheChrist.Presentation/Resources/AppResources.resx`, `AppResources.es.resx`,
  `AppResources.Designer.cs`
- `tests/JesusTheChrist.Presentation.Tests/ViewModels/TopicFeedViewModelTests.cs`
- `tests/JesusTheChrist.Presentation.Tests/Resources/AppResourcesTests.cs`

No data, id, read-mark, note, or migration surface is touched.

## Approach — test-driven

Every test is written and watched fail before the code that satisfies it.

1. `FakeClipboard` + `ReferenceCardViewModelTests` covering `CopyText` shape (single-chapter,
   cross-chapter, collapsed segments, ±context exclusion, caching).
2. Copy command behaviour: writes to the clipboard, raises `JustCopied`, clears after the delay,
   waits the advertised duration, refuses re-entry while confirming.
3. `IClipboardService` + card implementation to turn them green.
4. `TopicFeedViewModel` wiring, with a harness test proving a card's copy reaches the clipboard.
5. Resource keys `CardCopy` / `CardCopied` / `A11yCopyVerse` in en and es, with assertions.
6. XAML: a fourth column in both action rows.

The XAML has no automated coverage — the App project has no unit tests by design — so it is carried
by the device checklist below.

## Verification

- `dotnet build` clean (warnings-as-errors, `AnalysisMode=all`, StyleCop).
- `dotnet test` — all three projects green, output pristine.
- On the physical Android phone:
  - Collapsed card → **Copy** → label flips to `✓ Copied`, reverts ~2s later; paste shows the label
    and numbered verses.
  - Expanded card → **Copy** sits in the footer beside `+ Note`; same behaviour.
  - Cross-chapter card with a collapsed second chapter copies **all** chapters, with headers.
  - Spanish → `Copiar` / `✓ Copiado`; TalkBack announces the button.
  - The action row does not visibly jump width when the label changes.

## Versioning

This phase does **not** bump `ApplicationDisplayVersion` / `ApplicationVersion`. Play Store review is
a heavy gate, so this phase and the hold-to-copy phase ship under one version, bumped in a separate
release PR after both merge.

## Outcomes

Implemented as designed. `dotnet build JesusTheChrist.slnx` succeeds with zero warnings; `dotnet test`
is green across all three projects — **180 tests** (Core 44, Data 20, Presentation 116), up from 170.

Ten new tests cover the card: five on the shape of `CopyText` (single-chapter, cross-chapter headers,
collapsed segments, ±context exclusion, caching) and five on the command (clipboard write, the
`JustCopied` window, the advertised duration, the property-changed pair, disabled-while-confirming).
An eleventh in `TopicFeedViewModelTests` proves a real card's copy reaches the injected clipboard.

**On-device verification is still outstanding** — the XAML has no automated coverage, so the checklist
below has not been exercised yet.

## Deviations from plan

1. **`Copy_WhileConfirming_DoesNotRunAgain` asserted the wrong mechanism.** It was written believing
   `AsyncRelayCommand.ExecuteAsync` refuses a concurrent call. It does not — the guard is `CanExecute`
   returning false while the command runs, which is what disables the button. The original test held
   its second invocation on a gate forever and hung the suite. Rewritten as
   `Copy_WhileConfirming_CannotBeInvokedAgain`, asserting `CanExecute` before, during, and after. The
   code comment in `CopyAsync` was corrected to match.
2. **`CopyText` uses the C# 14 `field` keyword** rather than an explicit `private string? copyText`.
   IDE0032 (error under `AnalysisMode=all`) flagged the separate backing field as redundant now that
   `field` exists. `public string CopyText => field ??= this.BuildCopyText();` is the same lazy cache
   with one fewer moving part.
3. **`ArgumentNullException.ThrowIfNull(clipboard)` added to the `TopicFeedViewModel` constructor.**
   CA1062 fires on `clipboard` and not on its eight siblings because it is the only parameter the
   constructor dereferences — the delegate hoist reads `clipboard.SetTextAsync` on the spot.
4. **New files were written without a byte-order mark**, matching every existing `.cs` file in the
   repo, even though the root `.editorconfig` declares `charset = utf-8-bom`. Consistency with the
   tree beat the declared setting; nothing enforces it at build time.

## Lessons learned

- `.editorconfig` claims `utf-8-bom`, but the repo is uniformly BOM-less. Worth reconciling one way or
  the other, otherwise every new file is a coin flip. Logged as a nit, not fixed here.
- Concurrency on a generated `AsyncRelayCommand` is a `CanExecute` property, not an `ExecuteAsync`
  guard. Tests that drive commands directly bypass the very protection the UI relies on — assert
  `CanExecute` when the intent is "the button is inert".
- A test that hangs rather than fails is a design signal: the gated-delay double made the wrong
  assumption observable only as a deadlock. Worth preferring assertions on state over assertions on
  scheduling wherever both are available.
