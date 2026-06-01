# Phase: Plan 01 — Foundation & Content Layer

**Date:** 2026-05-31
**Plan:** planning/plan-01-foundation-content_2026-05-31.md
**Branch:** feature/plan-01-foundation

## Objective
Scaffold the solution and build the MAUI-free content domain library (models, JSON loader,
scope filter, content service) with full xUnit coverage.

## Approach
TDD task-by-task per the plan; pure Core library so tests run without MAUI/Android.

## Success criteria
- Solution builds; `dotnet test` green (fixtures + real-data smoke).
- Language-invariant reference ids; Bible-Only filter; order preserved.

## Files expected to change
- src/JesusTheChrist.Core/** , src/JesusTheChrist.App/** (scaffold), tests/JesusTheChrist.Core.Tests/**

## Environment notes
- .NET 10.0.108 present; `android` workload present; **`maui` workload NOT installed**
  (SDK at /usr/lib/dotnet is root-owned → install needs sudo). Decision pending on whether the
  MAUI App project is scaffolded here (WSL2) or in Visual Studio on Windows. The Core library +
  tests are pure .NET 10 and need no MAUI workload.

## Outcomes

- **Content layer complete and green: 32/32 xUnit tests pass** (`dotnet test`).
  - Core: Scope/Language/Volume, Slug, ContextVerse, Reference (Id/TargetText/ShowGloss),
    SubTopic (language-invariant Key), TopicalGuide, JSON DTOs + order-preserving loader,
    ScopeFilter, ContentService (IAssetSource seam). All MAUI-free, net10.0.
  - Tests: models, gloss rule, loader (incl. null/malformed negatives), scope filter,
    content service, and real-data smoke (53 sub-topics / 2,196 refs, unique keys).
- **MAUI App project delegated to Windows/VS** (owner has .NET MAUI + Android SDK there).
  The Core library + all tests run on Ubuntu with the .NET 10 SDK and need no MAUI workload.
  The EN/ES JSON is vendored to `src/JesusTheChrist.App/Resources/Raw/` ready for the App.
- **Deviations:**
  - .NET 10 emits `JesusTheChrist.slnx` (not `.sln`); use `dotnet build`/`dotnet test`
    (auto-detect). Plan text referencing `JesusTheChrist.sln` should read `.slnx`.
  - Task 1's `dotnet new maui` skipped here (no workload on Ubuntu); App is created in VS.
- **Major finding (decided with owner):** the Topical Guide orders references **thematically,
  not by book/chapter** (verified vs the raw church page: *Atonement through* →
  `Lev. 17:11 · Isa. 53:6 · Mosiah 14:6 …`, Mosiah 14 quoting Isaiah 53). The data faithfully
  preserves this. Replaced the false-premise canonical-order test with
  `Preserves_the_topical_guides_thematic_order`. Decision: **preserve TG order in v1**, add an
  optional canonical-sort toggle later (`planning/backlog/canonical-sort-toggle.md`). The
  design-spec/plan note "canonical TG order" is inaccurate and should be corrected.
