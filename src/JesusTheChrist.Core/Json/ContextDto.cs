using System.Text.Json.Serialization;

namespace JesusTheChrist.Core.Json;

internal sealed class ContextDto
{
    [JsonPropertyName("vs")]
    public int Vs { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("target")]
    public bool Target { get; set; }
}
