namespace JesusTheChrist.Presentation.Globalization;

/// <summary>
/// A synchronously-readable cache of the saved UI language code, so the app can apply the
/// correct culture at startup before the first page is built. The authoritative copy lives
/// in the settings store, which is async-only and therefore not usable that early.
/// </summary>
public interface ILanguagePreference
{
    /// <summary>
    /// Gets the saved language code (for example <c>"es"</c>), or <see langword="null"/> if unset.
    /// </summary>
    /// <returns>The saved language code, or <see langword="null"/>.</returns>
    public string? GetCode();

    /// <summary>
    /// Saves the language code so it is available at the next startup.
    /// </summary>
    /// <param name="code">The language code to save.</param>
    public void SetCode(string code);
}
