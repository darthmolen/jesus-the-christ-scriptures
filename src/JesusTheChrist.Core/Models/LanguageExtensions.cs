namespace JesusTheChrist.Core.Models;

/// <summary>
/// Helpers for <see cref="Language"/>.
/// </summary>
public static class LanguageExtensions
{
    /// <summary>
    /// Gets the two-letter content code (used in bundled asset file names).
    /// </summary>
    /// <param name="language">The language.</param>
    /// <returns>The language code, for example <c>en</c> or <c>es</c>.</returns>
    public static string Code(this Language language) => language switch
    {
        Language.En => "en",
        Language.Es => "es",
        _ => throw new System.ArgumentOutOfRangeException(nameof(language)),
    };

    /// <summary>
    /// Gets the language code used by churchofjesuschrist.org study URLs. Only the <c>lang</c>
    /// query parameter varies by language; the volume, book, and chapter path segments do not.
    /// </summary>
    /// <param name="language">The language.</param>
    /// <returns>The site language code, <c>eng</c> or <c>spa</c>.</returns>
    public static string SiteCode(this Language language) => language switch
    {
        Language.En => "eng",
        Language.Es => "spa",
        _ => throw new System.ArgumentOutOfRangeException(nameof(language)),
    };
}
