using System.Text;

namespace JesusTheChrist.Core.Models;

/// <summary>
/// Produces language-invariant kebab-case slugs.
/// </summary>
public static class Slug
{
    /// <summary>
    /// Converts text to a lowercase, hyphen-separated slug (letters and digits only).
    /// </summary>
    /// <param name="text">The text to slugify.</param>
    /// <returns>The kebab-case slug.</returns>
    public static string Make(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var sb = new StringBuilder(text.Length);
        var lastDash = false;
        foreach (var ch in text.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
                lastDash = false;
            }
            else if (sb.Length > 0 && !lastDash)
            {
                sb.Append('-');
                lastDash = true;
            }
        }

        return sb.ToString().Trim('-');
    }
}
