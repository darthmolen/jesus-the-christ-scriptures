# Phase: Plan 03 — MAUI App Scaffold + Home/Challenge Tracker

**Date:** 2026-06-07
**Plan:** `C:\Users\swmol\.claude\plans\read-c-dev-jesus-the-christ-scriptures-p-stateful-bubble.md`
**Branches:** PR-A `feature/maui-app-scaffold`; PR-B `feature/home-challenge-tracker`

## Objective

Stand up the .NET 10 MAUI Android app on top of the finished Core + Data layers, then ship the
Home / Challenge screen: the 53 sub-topics as a scrollable list, each with a progress ring, under
an "X / 2,196 references read" header, wired live to read-marks and progress.

## Approach

- **PR-A** — add a `JesusTheChrist.Presentation` class library (net10.0, unit-testable) and a
  `JesusTheChrist.App` MAUI project (net10.0-android); reconcile with Central Package Management +
  StyleCop warnings-as-errors; DI wiring + `MauiAssetSource`; launch to a stub Home page.
- **PR-B** — `RingMath` + `LanguageResolver` + `HomeViewModel`/`TopicRowViewModel` (all TDD), a
  `ProgressRingDrawable` IDrawable, and the Home XAML (Shell + CollectionView + ring).
- TDD throughout; CommunityToolkit.Mvvm; no changes to completed Core/Data projects.

## Success criteria

- `dotnet build` clean under warnings-as-errors; `dotnet test` green (new Presentation tests).
- App launches on Android; Home shows the challenge header + 53 sub-topics in thematic order,
  each with a progress ring reflecting real read-marks.
- Two PRs opened into `main`; owner (darthmolen) merges. Agent stops at "PR opened."

## Files expected to change

- Create: `src/JesusTheChrist.Presentation/**`, `tests/JesusTheChrist.Presentation.Tests/**`,
  `src/JesusTheChrist.App/**` (csproj, MauiProgram, App, AppShell, Views, Services, Drawing).
- Modify: `src/Directory.Packages.props` (CommunityToolkit.Mvvm + MAUI package versions),
  `JesusTheChrist.slnx`.
- Do NOT modify `src/JesusTheChrist.Core/**` or `src/JesusTheChrist.Data/**`.

## Dependencies / prerequisites

- .NET SDK 10.0.300 (confirmed); workloads `android` + MAUI (confirmed installed).
- Content already vendored: `src/JesusTheChrist.App/Resources/Raw/jesus-christ.{en,es}.json`.

## Outcomes

_(to be filled on completion — actual results, deviations, lessons)_
