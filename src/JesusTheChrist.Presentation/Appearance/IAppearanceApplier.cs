namespace JesusTheChrist.Presentation.Appearance;

/// <summary>
/// Applies appearance choices app-wide (platform-specific; implemented in the App).
/// </summary>
public interface IAppearanceApplier
{
    /// <summary>
    /// Applies the colour theme.
    /// </summary>
    /// <param name="theme">The theme to apply.</param>
    public void ApplyTheme(ThemeOption theme);

    /// <summary>
    /// Applies the reading font size to the scripture text.
    /// </summary>
    /// <param name="fontSize">The font size in device-independent units.</param>
    public void ApplyReadingFontSize(double fontSize);
}
