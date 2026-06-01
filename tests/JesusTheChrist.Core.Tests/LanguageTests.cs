using JesusTheChrist.Core.Models;
using Xunit;

public class LanguageTests
{
    [Theory]
    [InlineData(Language.En, "en")]
    [InlineData(Language.Es, "es")]
    public void Code_maps_language(Language l, string expected)
        => Assert.Equal(expected, l.Code());
}
