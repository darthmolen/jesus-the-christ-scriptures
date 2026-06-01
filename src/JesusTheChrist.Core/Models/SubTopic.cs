namespace JesusTheChrist.Core.Models;

/// <summary>
/// A Topical Guide sub-topic and its references.
/// </summary>
/// <param name="Key">The language-invariant slug key (from the English short title).</param>
/// <param name="Title">The full display title, for example <c>Jesus Christ, Atonement through</c>.</param>
/// <param name="ShortTitle">The short title in the content language, for example <c>Atonement through</c>.</param>
/// <param name="References">The references under this sub-topic, in Topical Guide order.</param>
public record SubTopic(
    string Key,
    string Title,
    string ShortTitle,
    IReadOnlyList<Reference> References);
