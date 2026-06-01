using System.IO;
using System.Linq;
using System.Text.Json;
using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Core.Json;

public static class TopicalGuideLoader
{
    public static TopicalGuide Load(Stream json)
    {
        var dto = JsonSerializer.Deserialize<GuideDto>(json)
                  ?? throw new InvalidDataException("Empty Topical Guide JSON.");

        var subtopics = dto.SubTopics.Select(s => new SubTopic(
            Slug.Make(s.ShortEn ?? s.Short),
            s.Title, s.Short,
            s.References.Select(r => new Reference(
                r.Ref, VolumeExtensions.Parse(r.Vol), r.Book, r.BookTitle, r.Ch,
                r.Verses,
                r.Context.Select(c => new ContextVerse(c.Vs, c.Text, c.Target)).ToList(),
                r.Note)).ToList()
        )).ToList();

        return new TopicalGuide(dto.Topic, dto.Language, subtopics);
    }
}
