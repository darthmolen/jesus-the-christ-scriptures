using JesusTheChrist.Presentation.Globalization;

namespace JesusTheChrist.Presentation.Tests.Fakes;

/// <summary>
/// In-memory <see cref="ILanguagePreference"/> that records the saved language code.
/// </summary>
public sealed class FakeLanguagePreference : ILanguagePreference
{
    private string? code;

    /// <inheritdoc/>
    public string? GetCode() => this.code;

    /// <inheritdoc/>
    public void SetCode(string code) => this.code = code;
}
