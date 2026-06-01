using System.Collections.Generic;
using System.Linq;

namespace JesusTheChrist.Core.Models;

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
    // subtopicKey is the language-invariant slug from SubTopic.Key.
    public string Id(string subtopicKey)
    {
        var lo = Verses.Count > 0 ? Verses[0] : 0;
        var hi = Verses.Count > 0 ? Verses[^1] : 0;
        var span = lo == hi ? $"{lo}" : $"{lo}-{hi}";
        return $"{subtopicKey}:{Vol.ToString().ToLowerInvariant()}/{Book}/{Ch}/{span}";
    }

    public string TargetText =>
        string.Join(" ", Context.Where(c => c.Target).Select(c => c.Text));

    public bool ShowGloss =>
        !string.IsNullOrWhiteSpace(Note) && !Normalize(TargetText).Contains(Normalize(Note!));

    private static string Normalize(string s)
    {
        var sb = new System.Text.StringBuilder(s.Length);
        foreach (var ch in s.ToLowerInvariant())
            if (char.IsLetterOrDigit(ch)) sb.Append(ch);
            else if (char.IsWhiteSpace(ch)) sb.Append(' ');
        return string.Join(' ', sb.ToString().Split(' ', System.StringSplitOptions.RemoveEmptyEntries));
    }
}
