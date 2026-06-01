# Phase: Plan 02 — Local Data Layer

**Date:** 2026-05-31
**Plan:** planning/plan-02-data-layer_2026-05-31.md
**Branch:** feature/plan-02-data

## Objective
On-device SQLite persistence (read-marks, notes, settings) + derived progress + streak logic,
pure .NET, fully tested.

## Success criteria
- `dotnet test` green across Core + Data tests.
- Read-marks/notes keyed by language-invariant Reference.Id; progress derived; streak by date.

## Files expected to change
- src/JesusTheChrist.Data/**, tests/JesusTheChrist.Data.Tests/**, JesusTheChrist.slnx

## Outcomes

- **Data layer complete: 15/15 Data tests pass; Core 32/32 still green.**
  - `JesusTheChrist.Data` (net10.0, pure .NET): `AppDatabase` + `ReadMark`/`NoteEntry`/`Setting`,
    `ReadMarkStore`, `NoteStore`, `SettingsStore` (+ `SettingKeys`), `ProgressService`,
    `StreakService`/`StreakStore`. sqlite-net-pcl 1.9.172 + SQLitePCLRaw.bundle_e_sqlite3 2.1.10.
  - Native SQLite loads fine on Ubuntu (the AppDatabase round-trip test is the smoke check).
  - Time injected (`Func<DateTime>` / passed `DateOnly`) — deterministic tests.
- **Review hardening applied** before implementation (preflight gate, `.slnx`, centralized streak
  keys, contracts + backwards-date guard, all-three-tables test, conventions).
- **Follow-up logged:** `planning/backlog/code-quality-and-cpm.md` — StyleCop/quality rules +
  Central Package Management (src vs tests), planned by owner for the next session.
- **Deviation:** per-task TDD commits batched into one `feat(data)` commit during autonomous
  execution; the layer is the reviewable unit on the PR.
