using System.Text.Json;
using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Core.Json;

/// <summary>
/// Loads a Topical Guide extract from its bundled JSON stream.
/// </summary>
public static class TopicalGuideLoader
{
    /// <summary>
    /// Deserializes a Topical Guide extract, preserving the source order of sub-topics and
    /// references and computing each sub-topic's language-invariant key.
    /// </summary>
    /// <param name="json">The JSON stream to read.</param>
    /// <returns>The parsed <see cref="TopicalGuide"/>.</returns>
    /// <exception cref="InvalidDataException">The stream did not contain a guide object.</exception>
    public static TopicalGuide Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);

        var dto = JsonSerializer.Deserialize<GuideDto>(json)
                  ?? throw new InvalidDataException("Empty Topical Guide JSON.");

        var subtopics = dto.SubTopics.Select(s => new SubTopic(
            Slug.Make(s.ShortEn ?? s.Short),
            s.Title,
            s.Short,
            s.References.Select(r => new Reference(
                r.Ref,
                VolumeExtensions.Parse(r.Vol),
                r.Book,
                r.BookTitle,
                r.Ch,
                r.Verses,
                r.Context.Select(c => new ContextVerse(c.Vs, c.Text, c.Target)).ToList(),
                r.Note)).ToList())).ToList();

        return new TopicalGuide(dto.Topic, dto.Language, subtopics);
    }
}
