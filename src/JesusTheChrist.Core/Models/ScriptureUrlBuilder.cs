using System.Globalization;
using System.Text;

namespace JesusTheChrist.Core.Models;

/// <summary>
/// Builds churchofjesuschrist.org study links for a scripture reference, so a reader can open the
/// passage on the Church's site — which on Android hands off to the Gospel Library app when it is
/// installed, and falls back to the browser when it is not.
/// </summary>
/// <remarks>
/// Link construction only: no API call and no scraping. The study-site URL scheme is documented in
/// <c>docs/DEEP-LINKING-TO-LDS-WEBSITE.md</c> and is stable. The bundled corpus already stores the
/// Church's own book codes (<c>heb</c>, <c>1-jn</c>, <c>w-of-m</c>, <c>dc</c>, …), so the book
/// segment needs no translation table — only the volume and language do.
/// </remarks>
public static class ScriptureUrlBuilder
{
    private const string Base = "https://www.churchofjesuschrist.org/study/scriptures";

    /// <summary>
    /// Builds the study link for a reference, targeting the first chapter it covers.
    /// </summary>
    /// <param name="reference">The reference to link.</param>
    /// <param name="language">The reader's language, which selects the <c>lang</c> parameter.</param>
    /// <returns>The absolute study URL.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="reference"/> is <see langword="null"/>.</exception>
    public static string Build(Reference reference, Language language)
    {
        ArgumentNullException.ThrowIfNull(reference);

        var segments = reference.TargetSegments();
        return segments.Count > 0
            ? Build(reference.Vol, reference.Book, segments[0].Ch, Numbers(segments[0]), language)
            : Build(reference.Vol, reference.Book, reference.Ch, reference.Verses, language);
    }

    /// <summary>
    /// Builds the study link for one chapter of a reference, highlighting only that chapter's
    /// target verses. Used by the per-chapter headers of a cross-chapter card.
    /// </summary>
    /// <param name="reference">The reference to link.</param>
    /// <param name="language">The reader's language, which selects the <c>lang</c> parameter.</param>
    /// <param name="chapter">The chapter within the reference's span to target.</param>
    /// <returns>The absolute study URL.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="reference"/> is <see langword="null"/>.</exception>
    public static string Build(Reference reference, Language language, int chapter)
    {
        ArgumentNullException.ThrowIfNull(reference);

        var segment = reference.TargetSegments().FirstOrDefault(s => s.Ch == chapter);
        return Build(
            reference.Vol,
            reference.Book,
            chapter,
            segment is null ? [] : Numbers(segment),
            language);
    }

    /// <summary>
    /// Builds a study link from its parts.
    /// </summary>
    /// <param name="volume">The volume the book belongs to.</param>
    /// <param name="book">The Church book code, for example <c>john</c> or <c>1-ne</c>. For the
    /// Doctrine and Covenants this is <c>dc</c> and <paramref name="chapter"/> is the section.</param>
    /// <param name="chapter">The chapter (or, for the Doctrine and Covenants, the section) number.</param>
    /// <param name="verses">The verses to highlight, ascending. Empty links the whole chapter.</param>
    /// <param name="language">The reader's language, which selects the <c>lang</c> parameter.</param>
    /// <returns>The absolute study URL.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="verses"/> is <see langword="null"/>.</exception>
    public static string Build(
        Volume volume,
        string book,
        int chapter,
        IReadOnlyList<int> verses,
        Language language)
    {
        ArgumentNullException.ThrowIfNull(verses);

        var url = string.Create(
            CultureInfo.InvariantCulture,
            $"{Base}/{volume.SiteCode()}/{book}/{chapter}?lang={language.SiteCode()}");

        if (verses.Count == 0)
        {
            return url;
        }

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{url}&id={VerseIds(verses)}#p{verses[0]}");
    }

    /// <summary>
    /// Formats the <c>id</c> parameter: a single verse as <c>p16</c>, an unbroken ascending run as
    /// <c>p7-p8</c>, and anything else as the comma list <c>p7,p9,p11</c>.
    /// </summary>
    /// <param name="verses">The verses to highlight, ascending.</param>
    /// <returns>The formatted verse identifiers.</returns>
    private static string VerseIds(IReadOnlyList<int> verses)
    {
        if (verses.Count == 1)
        {
            return $"p{verses[0]}";
        }

        if (IsContiguous(verses))
        {
            return $"p{verses[0]}-p{verses[^1]}";
        }

        var sb = new StringBuilder(verses.Count * 4);
        for (var i = 0; i < verses.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }

            sb.Append('p').Append(verses[i]);
        }

        return sb.ToString();
    }

    private static bool IsContiguous(IReadOnlyList<int> verses)
    {
        for (var i = 1; i < verses.Count; i++)
        {
            if (verses[i] != verses[i - 1] + 1)
            {
                return false;
            }
        }

        return true;
    }

    private static List<int> Numbers(ChapterSegment segment) =>
        segment.Verses.Select(v => v.Vs).ToList();
}
