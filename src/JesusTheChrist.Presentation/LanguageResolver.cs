using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Presentation;

/// <summary>
/// Maps a device/culture identifier to a supported content <see cref="Language"/>.
/// </summary>
public static class LanguageResolver
{
    /// <summary>
    /// Resolves a culture name (for example <c>"es"</c> or <c>"es-MX"</c>) to a supported language.
    /// </summary>
    /// <param name="cultureName">The culture name; the leading two-letter code is significant.</param>
    /// <returns>
    /// <see cref="Language.Es"/> for Spanish cultures; <see cref="Language.En"/> for everything else,
    /// including unknown, empty, or null input.
    /// </returns>
    public static Language Resolve(string? cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
        {
            return Language.En;
        }

        var twoLetter = cultureName.Split('-', 2)[0].ToLowerInvariant();
        return twoLetter == "es" ? Language.Es : Language.En;
    }
}
