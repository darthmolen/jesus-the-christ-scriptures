using JesusTheChrist.Presentation.Globalization;

namespace JesusTheChrist.App.Services;

/// <summary>
/// Mirrors the UI language code into MAUI <see cref="Preferences"/>, which is readable
/// synchronously at startup — before the async settings database is available.
/// </summary>
public sealed class PreferencesLanguagePreference : ILanguagePreference
{
    private const string Key = "ui_language";

    /// <inheritdoc/>
    public string? GetCode() => Preferences.Default.Get<string?>(Key, null);

    /// <inheritdoc/>
    public void SetCode(string code) => Preferences.Default.Set(Key, code);
}
