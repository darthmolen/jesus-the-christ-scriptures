# Phase: Cold-start language culture fix

**Date:** 2026-06-12
**Branch:** fix/startup-language-culture

## Problem
On a cold start with a saved non-default language (e.g. Spanish), the Home page rendered
with **English UI chrome but Spanish content**. Root cause: culture was applied only inside
`SettingsViewModel.LoadAsync` (async, fire-and-forget from `App.CreateWindow`), so it ran
*after* the first page was already built. The settings store is async-only and can't be read
early enough.

## Fix
Apply the saved language's culture **synchronously, before the shell builds**, by mirroring
the language to a synchronous preference.

- `AppCulture.Apply(Language)` — extracted culture-setting helper (Presentation/Globalization).
- `ILanguagePreference` — synchronous get/set seam (Presentation); App implements it with MAUI
  `Preferences` (`PreferencesLanguagePreference`).
- `SettingsViewModel.ApplyCulture` now also mirrors the language to the preference (on load and
  on change), so the value is always available at the next startup.
- `App.CreateWindow` reads the preference and calls `AppCulture.Apply` before resolving `AppShell`.

## Migration note
The preference is written the first time the new build runs `LoadAsync`. So an existing
non-default-language user sees the old behavior **once** on the first launch after updating,
then it's correct on every subsequent cold start.

## Outcome
- Presentation tests: 83/83 (added AppCulture + two preference-mirroring tests).
- Android build clean (0/0).
- Verified on device: after the migration launch, a cold start shows Home fully in Spanish
  ("Ajustes", "8 / 2196 referencias leídas"). Captured `08-home-es.png`, `09-feed-es.png`.

## Files
- src/JesusTheChrist.Presentation/Globalization/AppCulture.cs (new)
- src/JesusTheChrist.Presentation/Globalization/ILanguagePreference.cs (new)
- src/JesusTheChrist.Presentation/ViewModels/SettingsViewModel.cs
- src/JesusTheChrist.App/Services/PreferencesLanguagePreference.cs (new)
- src/JesusTheChrist.App/MauiProgram.cs
- src/JesusTheChrist.App/App.xaml.cs
- tests/.../Fakes/FakeLanguagePreference.cs (new)
- tests/.../ViewModels/SettingsViewModelTests.cs
- tests/.../Globalization/AppCultureTests.cs (new)
