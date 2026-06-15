using JesusTheChrist.Core.Models;
using JesusTheChrist.Presentation.Resources;

namespace JesusTheChrist.Presentation.Globalization;

/// <summary>
/// The single ordered source of truth for the languages the app offers. The Settings
/// picker, the picker-index mapping, and culture resolution all derive from this list, so
/// adding a language is one entry here (plus its bundled content and translated string table).
/// </summary>
public static class LanguageCatalog
{
    /// <summary>
    /// Gets the offered languages, in display (picker) order.
    /// </summary>
    public static IReadOnlyList<LanguageOption> All { get; } =
    [
        new LanguageOption(Language.En, AppResources.LanguageEnglish),
        new LanguageOption(Language.Es, AppResources.LanguageSpanish),
    ];

    /// <summary>
    /// Gets the language autonyms in display order, for the Settings picker's item source.
    /// </summary>
    public static IReadOnlyList<string> Autonyms { get; } = All.Select(o => o.Autonym).ToList();

    /// <summary>
    /// Gets the display index of the given language, or 0 when it is not offered.
    /// </summary>
    /// <param name="language">The language to locate.</param>
    /// <returns>The zero-based display index.</returns>
    public static int IndexOf(Language language)
    {
        for (var i = 0; i < All.Count; i++)
        {
            if (All[i].Language == language)
            {
                return i;
            }
        }

        return 0;
    }

    /// <summary>
    /// Gets the language at the given display index.
    /// </summary>
    /// <param name="index">The zero-based display index.</param>
    /// <returns>The language at that index.</returns>
    public static Language At(int index) => All[index].Language;
}
