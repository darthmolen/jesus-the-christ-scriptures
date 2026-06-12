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
        // Apply mutates several process-wide culture statics; snapshot and restore them all so
        // this test can't leak an "es" culture into later tests in the (serialized) suite.
        var prevCulture = CultureInfo.CurrentCulture;
        var prevUiCulture = CultureInfo.CurrentUICulture;
        var prevDefaultCulture = CultureInfo.DefaultThreadCurrentCulture;
        var prevDefaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;
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
            CultureInfo.CurrentUICulture = prevUiCulture;
            CultureInfo.DefaultThreadCurrentCulture = prevDefaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = prevDefaultUiCulture;
            AppResources.Culture = prevResource;
        }
    }
}
