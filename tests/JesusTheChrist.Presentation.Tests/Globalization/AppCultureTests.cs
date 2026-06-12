using System.Globalization;
using JesusTheChrist.Core.Models;
using JesusTheChrist.Presentation.Globalization;
using JesusTheChrist.Presentation.Resources;

namespace JesusTheChrist.Presentation.Tests.Globalization;

public class AppCultureTests
{
    [Fact]
    public void Apply_SetsCurrentAndResourceCultureToLanguage()
    {
        var prevCulture = CultureInfo.CurrentUICulture;
        var prevResource = AppResources.Culture;
        try
        {
            AppCulture.Apply(Language.Es);

            Assert.Equal("es", CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
            Assert.Equal("es", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            Assert.Equal("es", AppResources.Culture?.TwoLetterISOLanguageName);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevCulture;
            AppResources.Culture = prevResource;
        }
    }
}
