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
public record Reference(
    string RefLabel,
    Volume Vol,
    string Book,
    string BookTitle,
    int Ch,
    IReadOnlyList<int> Verses,
    IReadOnlyList<ContextVerse> Context,
    string? Note)
{
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
        && !Normalize(this.TargetText).Contains(Normalize(this.Note!), StringComparison.Ordinal);

    /// <summary>
    /// Builds the stable, language-invariant identifier for this reference within a sub-topic.
    /// </summary>
    /// <param name="subTopicKey">The owning sub-topic's language-invariant key.</param>
    /// <returns>The reference identifier, for example <c>advocate:newtestament/john/3/16</c>.</returns>
    public string Id(string subTopicKey)
    {
        var lo = this.Verses.Count > 0 ? this.Verses[0] : 0;
        var hi = this.Verses.Count > 0 ? this.Verses[^1] : 0;
        var span = lo == hi ? $"{lo}" : $"{lo}-{hi}";
        return $"{subTopicKey}:{this.Vol.ToString().ToLowerInvariant()}/{this.Book}/{this.Ch}/{span}";
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
