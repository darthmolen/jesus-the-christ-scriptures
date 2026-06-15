# Phase: Frictionless localization via a language catalog

**Date:** 2026-06-15
**Branch:** feature/language-catalog

## Objective

Adding a content language used to mean editing several scattered places, including a
binary `es ? 1 : 0` picker-index assumption that breaks on the third language. Centralize
the offered languages so adding one is a single catalog entry plus its content/strings.

## Approach

- **New `LanguageCatalog`** (`Globalization`) — the single ordered source of truth:
  `All`, `Autonyms`, `IndexOf`, `At`. Plus `LanguageOption` (Language + Code + Autonym).
- **`LanguageResolver.Resolve`** now loops `LanguageCatalog.All` instead of a hardcoded
  `es` check — kept as the named entry point so its 6 call sites and tests are untouched.
- **Settings picker** binds `ItemsSource` to `{x:Static glob:LanguageCatalog.Autonyms}`
  (like the ThemePicker's static items), so a new language appears automatically.
- **`SettingsPage.xaml.cs`** uses `LanguageCatalog.IndexOf` / `At` in place of the binary
  index logic.
- **Completeness guard test** fails if the `Language` enum has a value the catalog omits.
- Rewrote `docs/HOW-TO-ADD-LANGUAGES.md` to the shorter catalog-driven flow.

## Files

- NEW `src/JesusTheChrist.Presentation/Globalization/LanguageOption.cs`
- NEW `src/JesusTheChrist.Presentation/Globalization/LanguageCatalog.cs`
- `src/JesusTheChrist.Presentation/LanguageResolver.cs`
- `src/JesusTheChrist.App/Views/SettingsPage.xaml` + `.xaml.cs`
- NEW `tests/JesusTheChrist.Presentation.Tests/Globalization/LanguageCatalogTests.cs`
- `docs/HOW-TO-ADD-LANGUAGES.md`

## Outcome

- Presentation suite: **89 passed** (4 new catalog tests; existing resolver tests green).
- Android App project builds clean (0 warnings/0 errors) — XAML SourceGen accepts the
  `x:Static` picker source.
- No release/tag cut — lands on `main` only.

## Notes / deviations

- Kept `LanguageResolver` data-driven rather than deleting it, to avoid editing the two
  hard-to-verify Android files (App.xaml.cs / MauiProgram.cs) and to keep its tests as
  regression cover. The catalog is still the lone place to extend.
- Picker bound via `x:Static` to the static `Autonyms` (not a VM property) — avoids a
  CA1822 "mark static" on a would-be instance member and matches the ThemePicker pattern.
