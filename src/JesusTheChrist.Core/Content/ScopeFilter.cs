using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Core.Content;

/// <summary>
/// Filters a Topical Guide by build <see cref="Scope"/>.
/// </summary>
public static class ScopeFilter
{
    /// <summary>
    /// Applies the scope, dropping out-of-scope references and any sub-topic left empty.
    /// </summary>
    /// <param name="guide">The guide to filter.</param>
    /// <param name="scope">The scope to apply.</param>
    /// <returns>The filtered guide (the same instance when <paramref name="scope"/> is
    /// <see cref="Scope.Full"/>).</returns>
    public static TopicalGuide Apply(TopicalGuide guide, Scope scope)
    {
        ArgumentNullException.ThrowIfNull(guide);

        if (scope == Scope.Full)
        {
            return guide;
        }

        var kept = guide.SubTopics
            .Select(t => t with { References = t.References.Where(r => r.Vol.IsBible()).ToList() })
            .Where(t => t.References.Count > 0)
            .ToList();

        return guide with { SubTopics = kept };
    }
}
