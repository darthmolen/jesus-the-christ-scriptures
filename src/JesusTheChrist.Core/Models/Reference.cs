using System.Text;

namespace JesusTheChrist.Core.Models;

/// <summary>
/// A single scripture reference within a sub-topic, with its verse context.
/// </summary>
/// <param name="RefLabel">The display label, for example <c>Luke 1:26–38</c>.</param>
/// <param name="Vol">The volume the reference belongs to.</param>
/// <param name="Book">The church book code, for example <c>luke</c> or <c>1-ne</c>.</param>
/// <param name="BookTitle">The localized book title, for example <c>Luke</c>.</param>
/// <param name="Ch">The chapter number.</param>
/// <param name="Verses">The targeted verse numbers, in ascending order.</param>
/// <param name="Context">The verse window (targets plus surrounding context).</param>
/// <param name="Note">An optional Topical Guide gloss, or <see langword="null"/>.</param>
/// <param name="EndCh">
/// The ending chapter for a cross-chapter reference, or <see langword="null"/> for a
/// single-chapter reference.
/// </param>
public record Reference(
    string RefLabel,
    Volume Vol,
    string Book,
    string BookTitle,
    int Ch,
    IReadOnlyList<int> Verses,
    IReadOnlyList<ContextVerse> Context,
    string? Note,
    int? EndCh = null)
{
    /// <summary>
    /// Gets a value indicating whether this reference spans more than one chapter.
    /// </summary>
    public bool SpansChapters => this.EndCh is int e && e != this.Ch;

    /// <summary>
    /// Gets the joined text of the targeted verses.
    /// </summary>
    public string TargetText =>
        string.Join(" ", this.Context.Where(c => c.Target).Select(c => c.Text));

    /// <summary>
    /// Gets a value indicating whether the note adds information beyond the verse text
    /// and should therefore be shown.
    /// </summary>
    public bool ShowGloss =>
        !string.IsNullOrWhiteSpace(this.Note)
        && !Normalize(this.TargetText).Contains(Normalize(this.Note), StringComparison.Ordinal);

    /// <summary>
    /// Groups the target verses into contiguous per-chapter segments, in source order. A
    /// single-chapter reference yields exactly one segment; a cross-chapter reference yields one
    /// segment per chapter it crosses, each carrying a display label such as <c>Matthew 10</c>.
    /// </summary>
    /// <returns>The ordered chapter segments covering the targeted verses.</returns>
    public IReadOnlyList<ChapterSegment> TargetSegments()
    {
        var builders = new List<(int Ch, List<ContextVerse> Verses)>();
        foreach (var verse in this.Context.Where(c => c.Target))
        {
            var ch = verse.Ch ?? this.Ch;
            if (builders.Count == 0 || builders[^1].Ch != ch)
            {
                builders.Add((ch, []));
            }

            builders[^1].Verses.Add(verse);
        }

        return builders
            .Select(b => new ChapterSegment(b.Ch, $"{this.BookTitle} {b.Ch}", b.Verses))
            .ToList();
    }

    /// <summary>
    /// Gets the chapters inside the reference's span that contribute no target verses, in
    /// ascending order. These are the chapters an interstitial "continues through chapter N"
    /// header would represent. A single-chapter reference returns an empty list.
    /// </summary>
    /// <returns>The chapters in <c>[Ch..EndCh]</c> that have no target verses.</returns>
    public IReadOnlyList<int> MissingChapters()
    {
        if (this.EndCh is not int end || end <= this.Ch)
        {
            return [];
        }

        var present = this.TargetSegments().Select(s => s.Ch).ToHashSet();
        var missing = new List<int>();
        for (var ch = this.Ch; ch <= end; ch++)
        {
            if (!present.Contains(ch))
            {
                missing.Add(ch);
            }
        }

        return missing;
    }

    /// <summary>
    /// Builds the stable, language-invariant identifier for this reference within a sub-topic.
    /// </summary>
    /// <param name="subTopicKey">The owning sub-topic's language-invariant key.</param>
    /// <returns>
    /// The reference identifier, for example <c>advocate:newtestament/john/3/16</c>, or
    /// <c>summary:newtestament/matt/9:35-11:1</c> for a cross-chapter reference.
    /// </returns>
    public string Id(string subTopicKey)
    {
        var vol = this.Vol.ToString().ToLowerInvariant();

        if (this.SpansChapters)
        {
            var segments = this.TargetSegments();
            if (segments.Count > 0)
            {
                var first = segments[0];
                var last = segments[^1];
                var startVs = first.Verses.Count > 0 ? first.Verses[0].Vs : 0;
                var endVs = last.Verses.Count > 0 ? last.Verses[^1].Vs : 0;
                return $"{subTopicKey}:{vol}/{this.Book}/{first.Ch}:{startVs}-{last.Ch}:{endVs}";
            }
        }

        var lo = this.Verses.Count > 0 ? this.Verses[0] : 0;
        var hi = this.Verses.Count > 0 ? this.Verses[^1] : 0;
        var span = lo == hi ? $"{lo}" : $"{lo}-{hi}";
        return $"{subTopicKey}:{vol}/{this.Book}/{this.Ch}/{span}";
    }

    private static string Normalize(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var ch in value.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
            }
            else if (char.IsWhiteSpace(ch))
            {
                sb.Append(' ');
            }
        }

        return string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
