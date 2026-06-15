# How to add a content language

The app ships its content (the Topical Guide JSON, the invitation, and every UI string)
**bundled per language**. Adding a language is a content + one-entry wiring task — no
network, no server. This guide uses **Portuguese (`pt`)** as the worked example; replace
`pt` / `Pt` / `Português` with your language throughout.

Everything user-facing derives from a single **`LanguageCatalog`** entry: the Settings
picker, the picker-index mapping, and culture resolution all read from it. So you **no
longer touch** the resolver, the picker XAML, or any index logic — add the catalog entry
and those update themselves.

## 1. Add the enum value

[src/JesusTheChrist.Core/Models/Language.cs](../src/JesusTheChrist.Core/Models/Language.cs)

```csharp
public enum Language
{
    En,
    Es,
    Pt,
}
```

## 2. Map the content code

[src/JesusTheChrist.Core/Models/LanguageExtensions.cs](../src/JesusTheChrist.Core/Models/LanguageExtensions.cs) —
`Code()` is the source of truth for the file-name suffix. It `throw`s on an unmapped value,
so the build/tests point here if you forget.

```csharp
public static string Code(this Language language) => language switch
{
    Language.En => "en",
    Language.Es => "es",
    Language.Pt => "pt",
    _ => throw new System.ArgumentOutOfRangeException(nameof(language)),
};
```

## 3. Add the language's autonym string

The picker shows each language's name **in its own language** (e.g. "Español" reads the same
in every UI). Add a key — say `LanguagePortuguese` with value `Português` — to the neutral
[AppResources.resx](../src/JesusTheChrist.Presentation/Resources/AppResources.resx) **and
every** culture `.resx`, then expose it in the hand-written `AppResources.Designer.cs`. Model
it on the existing `LanguageEnglish` / `LanguageSpanish` keys. (This is the one place a *new
key* — not just a translation — touches the Designer.)

## 4. Add the catalog entry (the one wiring step)

[src/JesusTheChrist.Presentation/Globalization/LanguageCatalog.cs](../src/JesusTheChrist.Presentation/Globalization/LanguageCatalog.cs) —
add one line, in the order you want it to appear in the picker:

```csharp
public static IReadOnlyList<LanguageOption> All { get; } =
[
    new LanguageOption(Language.En, AppResources.LanguageEnglish),
    new LanguageOption(Language.Es, AppResources.LanguageSpanish),
    new LanguageOption(Language.Pt, AppResources.LanguagePortuguese),
];
```

That single entry drives all three of these automatically — **do not edit them**:

- **`LanguageResolver.Resolve`** loops the catalog, so `pt` / `pt-BR` now resolves to
  `Language.Pt` (and is the first-launch default when the device is Portuguese).
- The **Settings picker** binds its items to `LanguageCatalog.Autonyms`
  ([SettingsPage.xaml](../src/JesusTheChrist.App/Views/SettingsPage.xaml)).
- The **picker-index mapping** uses `LanguageCatalog.IndexOf` / `At`
  ([SettingsPage.xaml.cs](../src/JesusTheChrist.App/Views/SettingsPage.xaml.cs)).

## 5. Add the bundled content JSON

Drop `jesus-christ.pt.json` into
[src/JesusTheChrist.App/Resources/Raw/](../src/JesusTheChrist.App/Resources/Raw/), matching
the exact schema of `jesus-christ.en.json` (same structure, translated text + localized
scripture). It's auto-bundled — the `.csproj` globs `Resources\Raw\**` as `MauiAsset`, and
`ContentService` opens it by the bare name `jesus-christ.pt.json`. No csproj edit.

## 6. Add the bundled invitation

Add `invitation.pt.md` to the same `Resources/Raw/` folder, alongside the `en` / `es` ones.

## 7. Add the UI string table

Copy the neutral [AppResources.resx](../src/JesusTheChrist.Presentation/Resources/AppResources.resx)
(English) to `AppResources.pt.resx` and translate **every** `<value>`. The SDK auto-compiles
it to a satellite assembly keyed by culture — no `.csproj` edit. `AppCulture.Apply` sets
`AppResources.Culture`, and pages built afterward render from the matching satellite.
(`NeutralLanguage=en` means the neutral table *is* English — there is no `AppResources.en.resx`.)

## 8. Update the tests

`dotnet test` and follow the failures. The **completeness guard** in
`tests/JesusTheChrist.Presentation.Tests/Globalization/LanguageCatalogTests.cs`
(`All_CoversEveryLanguageExactlyOnce`) fails the moment the enum has a value the catalog
doesn't — that's your safety net for step 4. Also expect to touch:

- `tests/JesusTheChrist.Core.Tests/LanguageTests.cs` — the `Code()` mapping.
- `tests/JesusTheChrist.Presentation.Tests/LanguageResolverTests.cs` — `pt` / `pt-BR`
  resolve to `Pt`, unknown still falls back to `En`.
- `tests/JesusTheChrist.Presentation.Tests/ViewModels/AppResourcesTests.cs` — mirror the
  Spanish translation-coverage checks for the new language.

## 9. Verify end to end

1. `dotnet build` clean (warnings are errors) and `dotnet test` green.
2. Run on a device (see [HOW-TO-DEV.md](HOW-TO-DEV.md)). In **Settings → Language**, the new
   language appears; pick it and navigate to Home/a topic — chrome **and** content render
   translated.
3. Cold-start check: with it selected, fully close and reopen — the first screen is already
   in that language (no English flash). This exercises the `ILanguagePreference` startup path
   in [App.xaml.cs](../src/JesusTheChrist.App/App.xaml.cs).

## Checklist

- [ ] `Language` enum value
- [ ] `LanguageExtensions.Code()` mapping
- [ ] Autonym key in neutral + all culture `.resx` + `Designer.cs`
- [ ] **`LanguageCatalog` entry** (drives picker + index + resolver)
- [ ] `Resources/Raw/jesus-christ.<code>.json`
- [ ] `Resources/Raw/invitation.<code>.md`
- [ ] `AppResources.<code>.resx` (all keys translated)
- [ ] Tests updated and green
- [ ] Device + cold-start verification
