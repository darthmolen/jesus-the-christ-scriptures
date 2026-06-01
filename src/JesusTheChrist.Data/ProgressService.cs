using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Data;

/// <summary>
/// Computes reading progress from a Topical Guide and the set of read reference ids.
/// </summary>
public static class ProgressService
{
    /// <summary>
    /// Computes overall progress across the whole guide.
    /// </summary>
    /// <param name="guide">The Topical Guide.</param>
    /// <param name="readIds">The set of read reference identifiers.</param>
    /// <returns>The overall read/total progress.</returns>
    public static Progress Overall(TopicalGuide guide, IReadOnlySet<string> readIds)
    {
        ArgumentNullException.ThrowIfNull(guide);
        ArgumentNullException.ThrowIfNull(readIds);

        int read = 0, total = 0;
        foreach (var st in guide.SubTopics)
        {
            foreach (var r in st.References)
            {
                total++;
                if (readIds.Contains(r.Id(st.Key)))
                {
                    read++;
                }
            }
        }

        return new Progress(read, total);
    }

    /// <summary>
    /// Computes per-sub-topic progress, keyed by each sub-topic's language-invariant key.
    /// </summary>
    /// <param name="guide">The Topical Guide.</param>
    /// <param name="readIds">The set of read reference identifiers.</param>
    /// <returns>A map from sub-topic key to its read/total progress.</returns>
    public static IReadOnlyDictionary<string, Progress> PerSubTopic(
        TopicalGuide guide, IReadOnlySet<string> readIds)
    {
        ArgumentNullException.ThrowIfNull(guide);
        ArgumentNullException.ThrowIfNull(readIds);

        var map = new Dictionary<string, Progress>();
        foreach (var st in guide.SubTopics)
        {
            int read = 0, total = 0;
            foreach (var r in st.References)
            {
                total++;
                if (readIds.Contains(r.Id(st.Key)))
                {
                    read++;
                }
            }

            map[st.Key] = new Progress(read, total);
        }

        return map;
    }
}
