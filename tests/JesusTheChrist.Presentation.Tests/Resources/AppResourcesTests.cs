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
            Assert.Equal("Copy", AppResources.CardCopy);
            Assert.Equal("✓ Copied", AppResources.CardCopied);
            Assert.Equal("Copy this reference to the clipboard", AppResources.A11yCopyVerse);
            Assert.Equal("Verse copied", AppResources.ToastVerseCopied);
            Assert.Equal("Hold to copy this verse", AppResources.A11yHoldToCopyVerse);
            Assert.Equal("Open on churchofjesuschrist.org", AppResources.A11yOpenOnChurchSite);
            Assert.Equal("Go to this chapter", AppResources.A11yJumpToChapter);
            Assert.Equal("↑ To top", AppResources.CardBackToTop);
            Assert.Equal("Back to the chapter list", AppResources.A11yBackToTop);
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
            Assert.Equal("Copiar", AppResources.CardCopy);
            Assert.Equal("✓ Copiado", AppResources.CardCopied);
            Assert.Equal("Copiar esta referencia al portapapeles", AppResources.A11yCopyVerse);
            Assert.Equal("Versículo copiado", AppResources.ToastVerseCopied);
            Assert.Equal("Mantén presionado para copiar este versículo", AppResources.A11yHoldToCopyVerse);
            Assert.Equal("Abrir en churchofjesuschrist.org", AppResources.A11yOpenOnChurchSite);
            Assert.Equal("Ir a este capítulo", AppResources.A11yJumpToChapter);
            Assert.Equal("↑ Al inicio", AppResources.CardBackToTop);
            Assert.Equal("Volver a la lista de capítulos", AppResources.A11yBackToTop);
        }
        finally
        {
            AppResources.Culture = prev;
        }
    }

    [Fact]
    public void Attribution_IsTranslatedInSpanish()
    {
        var prev = AppResources.Culture;
        try
        {
            AppResources.Culture = new CultureInfo("en");
            var en = AppResources.SettingsAttribution;

            AppResources.Culture = new CultureInfo("es");
            var es = AppResources.SettingsAttribution;

            // The Spanish block is now a real translation, not the English placeholder.
            Assert.NotEqual(en, es);

            // The rights holder is named in both languages.
            Assert.Contains("Intellectual Reserve", en, StringComparison.Ordinal);
            Assert.Contains("Intellectual Reserve", es, StringComparison.Ordinal);

            // English scopes the notes to the Topical Guide, distinct from the reader's own notes.
            Assert.Contains("structure and notes of the Topical Guide", en, StringComparison.Ordinal);

            // Spanish uses the localized name of the Topical Guide.
            Assert.Contains("Guía para el Estudio de las Escrituras", es, StringComparison.Ordinal);
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
