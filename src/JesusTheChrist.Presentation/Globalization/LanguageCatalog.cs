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
    private static readonly LanguageOption[] Options =
    [
        new LanguageOption(Language.En, AppResources.LanguageEnglish),
        new LanguageOption(Language.Es, AppResources.LanguageSpanish),
    ];

    /// <summary>
    /// Gets the offered languages, in display (picker) order. The collection is a read-only
    /// wrapper, so callers cannot mutate the catalog through it.
    /// </summary>
    public static IReadOnlyList<LanguageOption> All { get; } = Array.AsReadOnly(Options);

    /// <summary>
    /// Gets the language autonyms in display order, for the Settings picker's item source.
    /// </summary>
    public static IReadOnlyList<string> Autonyms { get; } = Array.AsReadOnly(Options.Select(o => o.Autonym).ToArray());

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
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the catalog range.</exception>
    public static Language At(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, All.Count);
        return All[index].Language;
    }
}
