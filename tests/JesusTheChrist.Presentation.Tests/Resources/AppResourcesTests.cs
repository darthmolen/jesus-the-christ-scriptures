using System.Globalization;
using JesusTheChrist.Presentation.Resources;

// AppResources.Culture and the CultureInfo statics are process-global; serialize the suite
// so culture-mutating tests don't race across parallel test classes.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace JesusTheChrist.Presentation.Tests.Resources;

public class AppResourcesTests
{
    [Fact]
    public void English_ResolvesNeutralStrings()
    {
        var prev = AppResources.Culture;
        try
        {
            AppResources.Culture = new CultureInfo("en");
            Assert.Equal("Settings", AppResources.SettingsTitle);
            Assert.Equal("+ Note", AppResources.CardAddNote);
            Assert.Equal("Begin", AppResources.ActionBegin);
            Assert.Equal("Show scriptural context", AppResources.CardShowContext);
        }
        finally
        {
            AppResources.Culture = prev;
        }
    }

    [Fact]
    public void Spanish_ResolvesTranslatedStrings()
    {
        var prev = AppResources.Culture;
        try
        {
            AppResources.Culture = new CultureInfo("es");
            Assert.Equal("Ajustes", AppResources.SettingsTitle);
            Assert.Equal("+ Nota", AppResources.CardAddNote);
            Assert.Equal("Comenzar", AppResources.ActionBegin);
            Assert.Equal("Idioma", AppResources.SettingsLanguage);
            Assert.Equal("La invitación", AppResources.InvitationTitle);
        }
        finally
        {
            AppResources.Culture = prev;
        }
    }

    [Fact]
    public void Attribution_StaysEnglishInSpanish()
    {
        var prev = AppResources.Culture;
        try
        {
            AppResources.Culture = new CultureInfo("en");
            var en = AppResources.SettingsAttribution;

            AppResources.Culture = new CultureInfo("es");
            var es = AppResources.SettingsAttribution;

            Assert.Equal(en, es);
            Assert.Contains("Intellectual Reserve", es, StringComparison.Ordinal);
        }
        finally
        {
            AppResources.Culture = prev;
        }
    }

    [Fact]
    public void ReferencesReadFormat_HasSpanishValueWithPlaceholders()
    {
        var prev = AppResources.Culture;
        try
        {
            AppResources.Culture = new CultureInfo("es");
            Assert.Equal("{0} / {1} referencias leídas", AppResources.HomeReferencesReadFormat);
        }
        finally
        {
            AppResources.Culture = prev;
        }
    }
}
