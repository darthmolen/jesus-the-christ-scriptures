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
}
