using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Core.Tests;

public sealed class LanguageTests
{
    [Theory]
    [InlineData(Language.En, "en")]
    [InlineData(Language.Es, "es")]
    public void Code_maps_language(Language l, string expected) =>
        Assert.Equal(expected, l.Code());
}
