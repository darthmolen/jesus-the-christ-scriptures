# Phase: Plan 06 — Settings & Reading Comfort

**Date:** 2026-06-07
**Roadmap:** `planning/roadmap_2026-06-07.md`
**Branch/PR:** `feature/plan-06-settings`

## Objective

A Settings screen and app-wide reading comfort: adjustable reading font size, light/dark/system
theme, language (en/es), streak toggle, and an about/attribution section. Settings persist on
device (SettingsStore) and apply app-wide.

## Approach

- **Presentation (TDD):** `ThemeOption` enum; `IAppearanceApplier` seam (apply theme + reading
  font size); `SettingsViewModel(SettingsStore, IDatabaseInitializer, IAppearanceApplier)` —
  LoadAsync reads persisted values and applies them; explicit async setters persist + apply each
  setting (avoids two-way-bind/persist recursion and stays unit-testable).
- **App (verified by launch):** `AppearanceApplier` sets `Application.UserAppTheme` and a
  `ReadingFontSize` app resource (DynamicResource the verse text binds to); `SettingsPage`
  (font slider, theme + language pickers, streak switch, attribution); apply persisted appearance
  at startup; a toolbar item on Home opens Settings.

## Success criteria

- Changing font size/theme updates the reading feed and persists across launches.
- `dotnet build` clean (warnings-as-errors); `dotnet test` green (new SettingsViewModel tests).
- PR opened into main; owner merges.

## Outcomes

_(to be filled on completion)_
