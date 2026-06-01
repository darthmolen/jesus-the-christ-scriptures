using System.Linq;
using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Core.Content;

public static class ScopeFilter
{
    public static TopicalGuide Apply(TopicalGuide guide, Scope scope)
    {
        if (scope == Scope.Full) return guide;

        var kept = guide.SubTopics
            .Select(t => t with { References = t.References.Where(r => r.Vol.IsBible()).ToList() })
            .Where(t => t.References.Count > 0)
            .ToList();

        return guide with { SubTopics = kept };
    }
}
