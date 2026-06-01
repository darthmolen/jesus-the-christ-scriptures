using System.Collections.Generic;
using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Data;

public readonly record struct Progress(int Read, int Total)
{
    public double Fraction => Total == 0 ? 0.0 : (double)Read / Total;
}

public sealed class ProgressService
{
    public Progress Overall(TopicalGuide guide, IReadOnlySet<string> readIds)
    {
        int read = 0, total = 0;
        foreach (var st in guide.SubTopics)
            foreach (var r in st.References)
            {
                total++;
                if (readIds.Contains(r.Id(st.Key))) read++;
            }
        return new Progress(read, total);
    }

    public IReadOnlyDictionary<string, Progress> PerSubTopic(
        TopicalGuide guide, IReadOnlySet<string> readIds)
    {
        var map = new Dictionary<string, Progress>();
        foreach (var st in guide.SubTopics)
        {
            int read = 0, total = 0;
            foreach (var r in st.References)
            {
                total++;
                if (readIds.Contains(r.Id(st.Key))) read++;
            }
            map[st.Key] = new Progress(read, total);
        }
        return map;
    }
}
