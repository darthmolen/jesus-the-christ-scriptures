using JesusTheChrist.Presentation.Appearance;

namespace JesusTheChrist.Presentation.Tests.Fakes;

/// <summary>
/// Records appearance application for assertion in tests.
/// </summary>
public sealed class FakeAppearanceApplier : IAppearanceApplier
{
    /// <summary>
    /// Gets the last theme applied, if any.
    /// </summary>
    public ThemeOption? LastTheme { get; private set; }

    /// <summary>
    /// Gets the last reading font size applied, if any.
    /// </summary>
    public double? LastFontSize { get; private set; }

    /// <inheritdoc/>
    public void ApplyTheme(ThemeOption theme) => this.LastTheme = theme;

    /// <inheritdoc/>
    public void ApplyReadingFontSize(double fontSize) => this.LastFontSize = fontSize;
}
