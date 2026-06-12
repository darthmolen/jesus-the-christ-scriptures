using JesusTheChrist.Presentation.Appearance;

namespace JesusTheChrist.App.Services;

/// <summary>
/// Applies appearance choices to the running MAUI application.
/// </summary>
public sealed class AppearanceApplier : IAppearanceApplier
{
    /// <summary>
    /// The resource key the scripture text binds to via DynamicResource.
    /// </summary>
    public const string ReadingFontSizeKey = "ReadingFontSize";

    /// <summary>
    /// The resource key the verse-number superscript binds to via DynamicResource.
    /// </summary>
    public const string VerseNumberFontSizeKey = "VerseNumberFontSize";

    /// <summary>
    /// Ratio of the verse-number size to the reading size, keeping the number
    /// visibly smaller than the verse body text at every slider position.
    /// </summary>
    public const double VerseNumberRatio = 0.6;

    /// <inheritdoc/>
    public void ApplyTheme(ThemeOption theme)
    {
        if (Application.Current is { } app)
        {
            app.UserAppTheme = theme switch
            {
                ThemeOption.Light => AppTheme.Light,
                ThemeOption.Dark => AppTheme.Dark,
                ThemeOption.System => AppTheme.Unspecified,
                _ => AppTheme.Unspecified,
            };
        }
    }

    /// <inheritdoc/>
    public void ApplyReadingFontSize(double fontSize)
    {
        if (Application.Current?.Resources is { } resources)
        {
            resources[ReadingFontSizeKey] = fontSize;
            resources[VerseNumberFontSizeKey] = fontSize * VerseNumberRatio;
        }
    }
}
