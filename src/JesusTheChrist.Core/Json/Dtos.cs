using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JesusTheChrist.Core.Json;

internal sealed class GuideDto
{
    [JsonPropertyName("topic")] public string Topic { get; set; } = "";
    [JsonPropertyName("language")] public string Language { get; set; } = "";
    [JsonPropertyName("subtopics")] public List<SubTopicDto> SubTopics { get; set; } = new();
}

internal sealed class SubTopicDto
{
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("short")] public string Short { get; set; } = "";
    [JsonPropertyName("short_en")] public string? ShortEn { get; set; }
    [JsonPropertyName("references")] public List<ReferenceDto> References { get; set; } = new();
}

internal sealed class ReferenceDto
{
    [JsonPropertyName("ref")] public string Ref { get; set; } = "";
    [JsonPropertyName("vol")] public string Vol { get; set; } = "";
    [JsonPropertyName("book")] public string Book { get; set; } = "";
    [JsonPropertyName("book_title")] public string BookTitle { get; set; } = "";
    [JsonPropertyName("ch")] public int Ch { get; set; }
    [JsonPropertyName("verses")] public List<int> Verses { get; set; } = new();
    [JsonPropertyName("context")] public List<ContextDto> Context { get; set; } = new();
    [JsonPropertyName("note")] public string? Note { get; set; }
}

internal sealed class ContextDto
{
    [JsonPropertyName("vs")] public int Vs { get; set; }
    [JsonPropertyName("text")] public string Text { get; set; } = "";
    [JsonPropertyName("target")] public bool Target { get; set; }
}
