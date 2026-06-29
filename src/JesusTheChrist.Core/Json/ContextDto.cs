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

    /// <summary>
    /// Gets or sets the chapter this verse belongs to, or <see langword="null"/> when it shares
    /// the reference's chapter. Only cross-chapter references set this per verse.
    /// </summary>
    [JsonPropertyName("ch")]
    public int? Ch { get; set; }
}
