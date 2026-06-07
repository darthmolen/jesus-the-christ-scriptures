using JesusTheChrist.Core.Models;
using JesusTheChrist.Presentation;

namespace JesusTheChrist.Presentation.Tests;

public class LanguageResolverTests
{
    [Theory]
    [InlineData("es")]
    [InlineData("es-MX")]
    [InlineData("ES")]
    public void Resolve_Spanish_ReturnsEs(string culture)
    {
        Assert.Equal(Language.Es, LanguageResolver.Resolve(culture));
    }

    [Theory]
    [InlineData("en")]
    [InlineData("en-US")]
    public void Resolve_English_ReturnsEn(string culture)
    {
        Assert.Equal(Language.En, LanguageResolver.Resolve(culture));
    }

    [Theory]
    [InlineData("fr")]
    [InlineData("")]
    [InlineData(null)]
    public void Resolve_UnknownOrEmpty_FallsBackToEn(string? culture)
    {
        Assert.Equal(Language.En, LanguageResolver.Resolve(culture));
    }
}
