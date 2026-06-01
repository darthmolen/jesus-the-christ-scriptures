using System.Text.Json.Serialization;

namespace JesusTheChrist.Core.Json;

internal sealed class SubTopicDto
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("short")]
    public string Short { get; set; } = string.Empty;

    [JsonPropertyName("short_en")]
    public string? ShortEn { get; set; }

    [JsonPropertyName("references")]
    public List<ReferenceDto> References { get; set; } = [];
}
