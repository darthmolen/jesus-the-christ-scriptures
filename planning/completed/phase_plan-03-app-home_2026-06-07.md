# Phase: Plan 03 — MAUI App Scaffold + Home/Challenge Tracker

**Date:** 2026-06-07
**Plan:** `C:\Users\swmol\.claude\plans\read-c-dev-jesus-the-christ-scriptures-p-stateful-bubble.md`
**Branches/PRs:** PR-A `feature/maui-app-scaffold` (#10, merged); PR-B `feature/home-challenge-tracker` (#11, merged)

## Objective

Stand up the .NET 10 MAUI Android app on the finished Core + Data layers, then ship the
Home / Challenge screen (53 sub-topics with progress rings under the challenge header).

## Outcomes — DONE

- **PR-A (#10, merged):** `JesusTheChrist.Presentation` (net10.0) lib + `.Tests`; `JesusTheChrist.App`
  MAUI net10.0-android project (MAUI + Material 3, `MauiVersion` floated to 10.0.70 over the 10.0.20
  workload); DI wiring (`MauiAssetSource`, `DatabaseInitializer`); launches on a physical device.
  Reconciled the MAUI template with Central Package Management + StyleCop warnings-as-errors
  (scoped `Platforms/.editorconfig` as generated, `GlobalSuppressions` for CA1724).
- **PR-B (#11, merged):** Home / Challenge tracker. `RingMath`, `LanguageResolver`,
  `TopicRowViewModel`, `HomeViewModel` (CommunityToolkit.Mvvm) — all TDD. `ProgressRingDrawable` +
  bindable `ProgressRingView`; `HomePage` (Shell + CollectionView + ring + header). Verified on device.
- **Tests:** 74 green at PR-B merge (32 Core, 15 Data, 27 Presentation).

## Deviations / notes

- **Pre-existing Core build break fixed:** `Reference.ShowGloss` used a redundant `this.Note!` that
  trips `IDE0370` under SDK 10.0.300; removed (behavior-preserving). Shipped in PR-A.
- **MAUI workload baseline is 10.0.20**, below the plan's ≥10.0.60 for Material 3; floated `MauiVersion`
  to 10.0.70 via NuGet (supported, restores cleanly).
- **No emulator on the build machine** — on-device launch verified by the owner's phone via Visual Studio.
- Per-topic counts read 0 until the reading feed adds mark-as-read (Plan 04).
