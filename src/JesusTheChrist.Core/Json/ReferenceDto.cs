using System.Text.Json.Serialization;

namespace JesusTheChrist.Core.Json;

internal sealed class ReferenceDto
{
    [JsonPropertyName("ref")]
    public string Ref { get; set; } = string.Empty;

    [JsonPropertyName("vol")]
    public string Vol { get; set; } = string.Empty;

    [JsonPropertyName("book")]
    public string Book { get; set; } = string.Empty;

    [JsonPropertyName("book_title")]
    public string BookTitle { get; set; } = string.Empty;

    [JsonPropertyName("ch")]
    public int Ch { get; set; }

    [JsonPropertyName("verses")]
    public List<int> Verses { get; set; } = [];

    [JsonPropertyName("context")]
    public List<ContextDto> Context { get; set; } = [];

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    /// <summary>
    /// Gets or sets the ending chapter for a cross-chapter reference, or <see langword="null"/>
    /// for a single-chapter reference.
    /// </summary>
    [JsonPropertyName("end_ch")]
    public int? EndCh { get; set; }
}
