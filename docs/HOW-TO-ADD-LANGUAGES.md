# How to add a content language

The app ships its content (the Topical Guide JSON, the invitation, and every UI string)
**bundled per language**. Adding a language is a content + wiring task — no network, no
server. This guide walks the full set of touch points using **Portuguese (`pt`)** as the
worked example. Replace `pt` / `Pt` / `Português` with your language throughout.

> Two-letter ISO codes are used as the asset/key suffix (`en`, `es`, `pt`). The picker and
> a couple of mappings still assume **exactly two** languages today — step 9 is where you
> generalize that, so read it before you start if you're adding the *third* language.

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
`Code()` is the single source of truth for the file-name suffix. It `throw`s on an
unmapped value, so the build/tests will point you here if you forget.

```csharp
public static string Code(this Language language) => language switch
{
    Language.En => "en",
    Language.Es => "es",
    Language.Pt => "pt",
    _ => throw new System.ArgumentOutOfRangeException(nameof(language)),
};
```

## 3. Teach the resolver the new culture

[src/JesusTheChrist.Presentation/LanguageResolver.cs](../src/JesusTheChrist.Presentation/LanguageResolver.cs)
maps a device/saved culture (e.g. `"pt-BR"`) back to a `Language`. It's currently a binary
`es ? Es : En`. Generalize it:

```csharp
var twoLetter = cultureName.Split('-', 2)[0].ToLowerInvariant();
return twoLetter switch
{
    "es" => Language.Es,
    "pt" => Language.Pt,
    _ => Language.En,   // English is the fallback for unknown/empty input
};
```

This drives **two** things: the device-default language at first launch
([MauiProgram.cs](../src/JesusTheChrist.App/MauiProgram.cs) resolves
`CultureInfo.CurrentUICulture`) and reading the saved preference back from the settings
store.

## 4. Add the bundled content JSON

Drop `jesus-christ.pt.json` into
[src/JesusTheChrist.App/Resources/Raw/](../src/JesusTheChrist.App/Resources/Raw/), matching
the exact schema of `jesus-christ.en.json` (same topic/subtopic/reference structure, just
translated text and the localized scripture). It's auto-bundled — the `.csproj` globs
`Resources\Raw\**` as `MauiAsset`, and `ContentService` opens it by the bare name
`jesus-christ.pt.json`. No csproj edit needed.

## 5. Add the bundled invitation

Add `invitation.pt.md` to the same `Resources/Raw/` folder, alongside `invitation.en.md` /
`invitation.es.md`.

## 6. Add the UI string table

Copy [AppResources.resx](../src/JesusTheChrist.Presentation/Resources/AppResources.resx)
(the neutral/English table) to `AppResources.pt.resx` in the same folder and translate
**every** `<value>`. The SDK auto-compiles it to a satellite assembly keyed by culture; you
do **not** edit the `.csproj`. `AppCulture.Apply` sets `AppResources.Culture`, and pages
built afterward render from the matching satellite. (`NeutralLanguage=en` in the
`.csproj` means the neutral table *is* English — there is no `AppResources.en.resx`.)

> Adding a **new key** (not just a translation) means editing the hand-written
> `AppResources.Designer.cs` plus the neutral table **and every** culture table. Adding a
> language only adds translations of existing keys, so the Designer is untouched — unless
> you also do step 7.

## 7. Add the language's autonym

The picker shows each language's name **in its own language** (e.g. "Español" reads the same
in the English and Spanish UIs). Add a key like `LanguagePortuguese` with value `Português`
to the neutral `AppResources.resx` **and** every culture `.resx`, and expose it in
`AppResources.Designer.cs` (this is the new-key case from step 6). Model it on the existing
`LanguageEnglish` / `LanguageSpanish` keys.

## 8. Add the picker option

[src/JesusTheChrist.App/Views/SettingsPage.xaml](../src/JesusTheChrist.App/Views/SettingsPage.xaml) —
add the autonym to the `LanguagePicker` items, in the same order you'll use in step 9:

```xml
<Picker x:Name="LanguagePicker">
    <Picker.ItemsSource>
        <x:Array Type="{x:Type x:String}">
            <x:Static Member="loc:AppResources.LanguageEnglish" />
            <x:Static Member="loc:AppResources.LanguageSpanish" />
            <x:Static Member="loc:AppResources.LanguagePortuguese" />
        </x:Array>
    </Picker.ItemsSource>
</Picker>
```

## 9. Fix the picker ↔ Language mapping (the binary assumption)

[src/JesusTheChrist.App/Views/SettingsPage.xaml.cs](../src/JesusTheChrist.App/Views/SettingsPage.xaml.cs)
maps the picker index to a `Language` with hard-coded two-language logic:

```csharp
// OnAppearing
this.LanguagePicker.SelectedIndex = this.viewModel.Language == Language.Es ? 1 : 0;
// OnLanguageChanged
await this.viewModel.SetLanguageAsync(this.LanguagePicker.SelectedIndex == 1 ? Language.Es : Language.En);
```

Replace both with an ordered list whose order matches the picker in step 8 — one place to
extend next time:

```csharp
private static readonly Language[] LanguageOrder = [Language.En, Language.Es, Language.Pt];

// OnAppearing
this.LanguagePicker.SelectedIndex = Array.IndexOf(LanguageOrder, this.viewModel.Language);
// OnLanguageChanged
await this.viewModel.SetLanguageAsync(LanguageOrder[this.LanguagePicker.SelectedIndex]);
```

The `switchingLanguage` re-entrancy guard and the `SelectedIndex < 0` check stay as-is.

## 10. Update the tests

Run `dotnet test` and follow the failures, but expect to touch:

- `tests/JesusTheChrist.Core.Tests/LanguageTests.cs` — the `Code()` mapping.
- `tests/JesusTheChrist.Presentation.Tests/LanguageResolverTests.cs` — the new culture (`pt`, `pt-BR`) resolving correctly, and unknown still falling back to `En`.
- `tests/JesusTheChrist.Presentation.Tests/ViewModels/AppResourcesTests.cs` — translation-coverage assertions (mirror the Spanish checks for the new language).
- Any exhaustive `switch` over `Language` the compiler/analyzer flags.

## 11. Verify end to end

1. `dotnet build` clean and `dotnet test` green.
2. Run on a device (see [HOW-TO-DEV.md](HOW-TO-DEV.md)). In **Settings → Language**, pick the
   new language; navigate to Home/a topic — chrome **and** content should render translated.
3. Cold-start check: with the new language selected, fully close and reopen the app — the
   first screen should already be in that language (no English flash). This exercises the
   `ILanguagePreference` startup path in [App.xaml.cs](../src/JesusTheChrist.App/App.xaml.cs).

## Checklist

- [ ] `Language` enum value
- [ ] `LanguageExtensions.Code()` mapping
- [ ] `LanguageResolver.Resolve()` culture mapping
- [ ] `Resources/Raw/jesus-christ.<code>.json`
- [ ] `Resources/Raw/invitation.<code>.md`
- [ ] `AppResources.<code>.resx` (all keys translated)
- [ ] Autonym key in neutral + all culture `.resx` + `Designer.cs`
- [ ] Picker option in `SettingsPage.xaml`
- [ ] Picker ↔ Language order in `SettingsPage.xaml.cs`
- [ ] Tests updated and green
- [ ] Device + cold-start verification
