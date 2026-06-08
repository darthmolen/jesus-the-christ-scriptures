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

    /// <inheritdoc/>
    public void ApplyTheme(ThemeOption theme)
    {
        if (Application.Current is { } app)
        {
            app.UserAppTheme = theme switch
            {
                ThemeOption.Light => AppTheme.Light,
                ThemeOption.Dark => AppTheme.Dark,
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
        }
    }
}
