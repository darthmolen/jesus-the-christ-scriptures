using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Presentation.Globalization;

/// <summary>
/// One language the app offers: pairs a <see cref="JesusTheChrist.Core.Models.Language"/> with
/// its two-letter content code and its autonym (its name written in its own language, shown in
/// the picker).
/// </summary>
public sealed class LanguageOption
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageOption"/> class.
    /// </summary>
    /// <param name="language">The language this option represents.</param>
    /// <param name="autonym">The language's name written in its own language.</param>
    public LanguageOption(Language language, string autonym)
    {
        this.Language = language;
        this.Autonym = autonym;
    }

    /// <summary>
    /// Gets the language this option represents.
    /// </summary>
    public Language Language { get; }

    /// <summary>
    /// Gets the two-letter content code (for example <c>en</c>), used in bundled asset names.
    /// </summary>
    public string Code => this.Language.Code();

    /// <summary>
    /// Gets the autonym — the language's name written in its own language.
    /// </summary>
    public string Autonym { get; }
}
