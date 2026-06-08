namespace JesusTheChrist.Presentation.Navigation;

/// <summary>
/// Well-known navigation route names and parameter keys.
/// </summary>
public static class NavigationRoutes
{
    /// <summary>
    /// The route for a single sub-topic's reference feed.
    /// </summary>
    public const string Topic = "topic";

    /// <summary>
    /// The parameter key carrying a sub-topic's language-invariant key.
    /// </summary>
    public const string TopicKeyParameter = "key";

    /// <summary>
    /// The route for the settings screen.
    /// </summary>
    public const string Settings = "settings";

    /// <summary>
    /// The route for the per-reference note editor.
    /// </summary>
    public const string Note = "note";

    /// <summary>
    /// The parameter key carrying a reference id to the note editor.
    /// </summary>
    public const string NoteRefIdParameter = "refId";
}
