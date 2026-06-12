using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using JesusTheChrist.Core.Models;
using JesusTheChrist.Data;
using JesusTheChrist.Presentation.Appearance;
using JesusTheChrist.Presentation.Data;
using JesusTheChrist.Presentation.Resources;

namespace JesusTheChrist.Presentation.ViewModels;

/// <summary>
/// Backs the Settings screen: reading font size, theme, language, and the streak toggle.
/// Each setting persists to <see cref="SettingsStore"/> and applies app-wide on change.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    /// <summary>
    /// The default reading font size when the user has no saved preference.
    /// </summary>
    public const double DefaultReadingFontSize = 16;

    private readonly SettingsStore settings;
    private readonly IDatabaseInitializer databaseInitializer;
    private readonly IAppearanceApplier appearance;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
    /// </summary>
    /// <param name="settings">The settings store.</param>
    /// <param name="databaseInitializer">Ensures the database schema before reads.</param>
    /// <param name="appearance">Applies theme and font choices app-wide.</param>
    public SettingsViewModel(SettingsStore settings, IDatabaseInitializer databaseInitializer, IAppearanceApplier appearance)
    {
        this.settings = settings;
        this.databaseInitializer = databaseInitializer;
        this.appearance = appearance;
    }

    /// <summary>
    /// Gets or sets the reading font size.
    /// </summary>
    [ObservableProperty]
    public partial double ReadingFontSize { get; set; } = DefaultReadingFontSize;

    /// <summary>
    /// Gets or sets the colour theme.
    /// </summary>
    [ObservableProperty]
    public partial ThemeOption Theme { get; set; } = ThemeOption.System;

    /// <summary>
    /// Gets or sets the content language.
    /// </summary>
    [ObservableProperty]
    public partial Language Language { get; set; } = Language.En;

    /// <summary>
    /// Gets or sets a value indicating whether the reading streak is tracked.
    /// </summary>
    [ObservableProperty]
    public partial bool StreakEnabled { get; set; }

    /// <summary>
    /// Loads persisted settings and applies the current appearance.
    /// </summary>
    /// <returns>A task that completes when settings are loaded and applied.</returns>
    public async Task LoadAsync()
    {
        await this.databaseInitializer.EnsureInitializedAsync();

        this.ReadingFontSize = await this.settings.GetIntAsync(SettingKeys.FontSize, (int)DefaultReadingFontSize);

        var savedTheme = await this.settings.GetAsync(SettingKeys.Theme);
        this.Theme = Enum.TryParse<ThemeOption>(savedTheme, ignoreCase: true, out var theme) ? theme : ThemeOption.System;

        var savedLanguage = await this.settings.GetAsync(SettingKeys.Language);
        this.Language = string.IsNullOrWhiteSpace(savedLanguage) ? Language.En : LanguageResolver.Resolve(savedLanguage);

        this.StreakEnabled = await this.settings.GetBoolAsync(SettingKeys.StreakEnabled, false);

        ApplyCulture(this.Language);
        this.appearance.ApplyTheme(this.Theme);
        this.appearance.ApplyReadingFontSize(this.ReadingFontSize);
    }

    /// <summary>
    /// Applies a reading font size for live preview without persisting it
    /// (call while the slider is moving).
    /// </summary>
    /// <param name="fontSize">The font size to preview.</param>
    public void PreviewReadingFontSize(double fontSize)
    {
        var rounded = Math.Round(fontSize);
        this.ReadingFontSize = rounded;
        this.appearance.ApplyReadingFontSize(rounded);
    }

    /// <summary>
    /// Sets, persists, and applies the reading font size. The persisted, in-memory,
    /// and applied values are all the same rounded integer, so the session matches
    /// what is read back after a restart.
    /// </summary>
    /// <param name="fontSize">The new font size.</param>
    /// <returns>A task that completes when persisted and applied.</returns>
    public async Task SetReadingFontSizeAsync(double fontSize)
    {
        var rounded = Math.Round(fontSize);
        this.ReadingFontSize = rounded;
        await this.settings.SetIntAsync(SettingKeys.FontSize, (int)rounded);
        this.appearance.ApplyReadingFontSize(rounded);
    }

    /// <summary>
    /// Sets, persists, and applies the colour theme.
    /// </summary>
    /// <param name="theme">The new theme.</param>
    /// <returns>A task that completes when persisted and applied.</returns>
    public async Task SetThemeAsync(ThemeOption theme)
    {
        this.Theme = theme;
        await this.settings.SetAsync(SettingKeys.Theme, theme.ToString().ToLowerInvariant());
        this.appearance.ApplyTheme(theme);
    }

    /// <summary>
    /// Sets and persists the content language.
    /// </summary>
    /// <param name="language">The new language.</param>
    /// <returns>A task that completes when persisted.</returns>
    public async Task SetLanguageAsync(Language language)
    {
        this.Language = language;
        await this.settings.SetAsync(SettingKeys.Language, language.Code());
        ApplyCulture(language);
    }

    /// <summary>
    /// Applies the given language to the app-wide culture and the localized string table.
    /// Pages built after this (on the next navigation) render in the new language; the
    /// existing page does not change live, matching the content "applies on next load" model.
    /// </summary>
    /// <param name="language">The language whose culture to apply.</param>
    private static void ApplyCulture(Language language)
    {
        var culture = CultureInfo.GetCultureInfo(language.Code());
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        AppResources.Culture = culture;
    }

    /// <summary>
    /// Sets and persists whether the reading streak is tracked.
    /// </summary>
    /// <param name="enabled">Whether the streak is enabled.</param>
    /// <returns>A task that completes when persisted.</returns>
    public async Task SetStreakEnabledAsync(bool enabled)
    {
        this.StreakEnabled = enabled;
        await this.settings.SetBoolAsync(SettingKeys.StreakEnabled, enabled);
    }
}
