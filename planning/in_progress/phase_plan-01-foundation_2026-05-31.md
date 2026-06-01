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
(to be filled at completion)
