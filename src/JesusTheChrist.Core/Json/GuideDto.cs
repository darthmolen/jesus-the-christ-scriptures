using System.Text.Json.Serialization;

namespace JesusTheChrist.Core.Json;

internal sealed class GuideDto
{
    [JsonPropertyName("topic")]
    public string Topic { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("subtopics")]
    public List<SubTopicDto> SubTopics { get; set; } = [];
}
