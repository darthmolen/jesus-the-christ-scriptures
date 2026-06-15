using JesusTheChrist.Core.Models;
using JesusTheChrist.Presentation.Globalization;

namespace JesusTheChrist.Presentation.Tests.Globalization;

public class LanguageCatalogTests
{
    [Fact]
    public void All_CoversEveryLanguageExactlyOnce()
    {
        var catalog = LanguageCatalog.All.Select(o => o.Language).ToList();
        var declared = Enum.GetValues<Language>();

        Assert.Equal(declared.Length, catalog.Count);
        Assert.Equal(catalog.Count, catalog.Distinct().Count());
        foreach (var language in declared)
        {
            Assert.Contains(language, catalog);
        }
    }

    [Fact]
    public void Autonyms_MatchAllInCountAndAreNonEmpty()
    {
        Assert.Equal(LanguageCatalog.All.Count, LanguageCatalog.Autonyms.Count);
        Assert.All(LanguageCatalog.Autonyms, name => Assert.False(string.IsNullOrWhiteSpace(name)));
    }

    [Fact]
    public void All_CodesMatchLanguageCode()
    {
        Assert.All(LanguageCatalog.All, option => Assert.Equal(option.Language.Code(), option.Code));
    }

    [Theory]
    [InlineData(Language.En)]
    [InlineData(Language.Es)]
    public void IndexOf_And_At_RoundTrip(Language language)
    {
        var index = LanguageCatalog.IndexOf(language);
        Assert.Equal(language, LanguageCatalog.At(index));
    }
}
