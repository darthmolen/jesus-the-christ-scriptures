# Gold completion state for the Home progress ring

Date: 2026-09-06
Branch: `feature/progress-ring-complete-state`

## Objective

A fully-read sub-topic on the Home screen rendered identically to an untouched one — the
reward for finishing disappeared at the moment it should have been strongest. Give 100% a
distinct gold ring, and make the ring's colours theme-aware while in the same code.

## Root cause

`ICanvas.DrawArc` takes **start and end angles, not a sweep**, but `ProgressRingDrawable.Draw`
passed `endAngle = TopAngle - sweep`. At `Fraction == 1.0`, `sweep == 360`, so
`endAngle == 90 - 360 == -270`. The platform canvases normalise a negative end angle back into
`[0, 360)` before computing the sweep, turning `-270` into `90` — exactly the start angle.
`GetSweep(90, 90, clockwise: true)` returns `0`, and a zero-degree arc paints nothing. Only the
`#E0E0E0` track circle survived, which is precisely what 0% looks like.

Every value in `(0, 360)` happens to round-trip correctly through that wrap arithmetic, so 99%
drew an almost-complete ring and 100% drew none. `RingMathTests.SweepDegrees_AtFull_IsThreeSixty`
passed the whole time because it tested the pure math and never the angle translation — the
defect lived in the MAUI head, which has no test project.

## Approach

Push the "is this complete?" decision down into `RingMath` (in `JesusTheChrist.Presentation`,
which *is* covered by tests) so the regression is guarded, then have the drawable paint a closed
circle rather than an arc in that case.

## Files modified

| File | Change |
|---|---|
| `src/JesusTheChrist.Presentation/Drawing/RingMath.cs` | added `IsComplete`; deleted the unused `StartAngleDegrees`, which contradicted the drawable's own `TopAngle` |
| `src/JesusTheChrist.App/Drawing/ProgressRingDrawable.cs` | `CompleteColor` property; full ring drawn as an ellipse instead of a 360° arc |
| `src/JesusTheChrist.App/Controls/ProgressRingView.cs` | `TrackColor` / `ProgressColor` / `CompleteColor` bindable properties |
| `src/JesusTheChrist.App/Resources/Styles/Colors.xaml` | `Gold` `#B8860B`, `GoldDark` `#F5C542` |
| `src/JesusTheChrist.App/Resources/Styles/Styles.xaml` | implicit `ProgressRingView` style with `AppThemeBinding` |
| `src/JesusTheChrist.App/Views/HomePage.xaml` | semantic description now binds `ProgressDescription` |
| `src/JesusTheChrist.Presentation/ViewModels/TopicRowViewModel.cs` | `IsComplete` + `ProgressDescription` |
| `AppResources.resx` / `.es.resx` / `.Designer.cs` | `HomeTopicCompleteFormat` (en + es-419) |
| `RingMathTests` / `TopicRowViewModelTests` / `AppResourcesTests` | 12 new tests |

## Decisions

- **Gold at 100%, not a filled medal or a check mark.** A solid gold ring keeps the shape
  consistent down the list; a column of filled gold discs is visually heavy, and a check glyph is
  illegible at 44px.
- **Colours moved to an implicit style, not inline on `HomePage`.** `AppThemeBinding` inside a
  `DataTemplate` is a known MAUI leak, and a style keeps the palette in one place.
- **The gold cue is not colour-only.** `ProgressLabel` is *visible* text in the row, so appending
  "complete" there would have cluttered the list. A separate `ProgressDescription` carries the
  completion word to TalkBack instead, satisfying WCAG 1.4.1 without changing the visuals.
- **Two gold tones.** No single gold clears 3:1 non-text contrast against both a white and a
  near-black ground.

## Success criteria

- [x] `dotnet test` — 245 passing (65 Core, 26 Data, 154 Presentation)
- [x] `dotnet build -f net10.0-android` — 0 warnings, 0 errors under strict CPM
- [ ] On-device: 0% grey, partial purple, **100% gold**, in both light and dark themes
- [ ] On-device: finishing a topic turns its ring gold on return to Home without a restart
- [ ] TalkBack speaks the completion word on a finished row

## Open risks

- The two gold hex values are unverified on real hardware. There is no emulator on this machine,
  so light-mode `#B8860B` in particular may read brown and need tuning.
- `TopicRowViewModel` is immutable and not `INotifyPropertyChanged`; rows refresh only when
  `HomeViewModel.LoadAsync` rebuilds the collection. If the ring does not turn gold on return to
  Home, that is a separate pre-existing bug, not this change.
