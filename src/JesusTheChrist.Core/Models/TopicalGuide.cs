namespace JesusTheChrist.Core.Models;

/// <summary>
/// A Topical Guide topic extract (the "Jesus Christ" entry) and its sub-topics.
/// </summary>
/// <param name="Topic">The topic name.</param>
/// <param name="Language">The content language code.</param>
/// <param name="SubTopics">The sub-topics, in source order.</param>
public record TopicalGuide(
    string Topic,
    string Language,
    IReadOnlyList<SubTopic> SubTopics);
