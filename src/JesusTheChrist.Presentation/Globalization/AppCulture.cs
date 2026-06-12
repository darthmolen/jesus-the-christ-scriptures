using System.Globalization;
using JesusTheChrist.Core.Models;
using JesusTheChrist.Presentation.Resources;

namespace JesusTheChrist.Presentation.Globalization;

/// <summary>
/// Applies a language choice to the process-wide culture and the localized string table,
/// so UI chrome built afterward renders in that language.
/// </summary>
public static class AppCulture
{
    /// <summary>
    /// Applies the given language as the current culture and UI culture, app-wide.
    /// </summary>
    /// <param name="language">The language whose culture to apply.</param>
    public static void Apply(Language language)
    {
        var culture = CultureInfo.GetCultureInfo(language.Code());
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        AppResources.Culture = culture;
    }
}
