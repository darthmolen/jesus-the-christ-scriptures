using System.Text;

namespace JesusTheChrist.Core.Models;

public static class Slug
{
    public static string Make(string text)
    {
        var sb = new StringBuilder(text.Length);
        var lastDash = false;
        foreach (var ch in text.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch)) { sb.Append(ch); lastDash = false; }
            else if (sb.Length > 0 && !lastDash) { sb.Append('-'); lastDash = true; }
        }
        return sb.ToString().Trim('-');
    }
}
