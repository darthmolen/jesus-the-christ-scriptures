using JesusTheChrist.Core.Models;
using JesusTheChrist.Presentation.Globalization;

namespace JesusTheChrist.Presentation;

/// <summary>
/// Maps a device/culture identifier to a supported content <see cref="Language"/>, using the
/// offered set from <see cref="LanguageCatalog"/> (so a new language resolves automatically).
/// </summary>
public static class LanguageResolver
{
    /// <summary>
    /// Resolves a culture name (for example <c>"es"</c> or <c>"es-MX"</c>) to a supported language.
    /// </summary>
    /// <param name="cultureName">The culture name; the leading two-letter code is significant.</param>
    /// <returns>
    /// The offered language whose code matches the leading two-letter code; <see cref="Language.En"/>
    /// for unknown, empty, or null input.
    /// </returns>
    public static Language Resolve(string? cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
        {
            return Language.En;
        }

        var twoLetter = cultureName.Split('-', 2)[0].ToLowerInvariant();
        foreach (var option in LanguageCatalog.All)
        {
            if (option.Code == twoLetter)
            {
                return option.Language;
            }
        }

        return Language.En;
    }
}
