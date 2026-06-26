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

    /// <summary>
    /// The parameter key carrying the reference's display label (for example "Heb. 7:25")
    /// to the note editor's scripture pane.
    /// </summary>
    public const string NoteRefLabelParameter = "refLabel";

    /// <summary>
    /// The parameter key carrying the target verses (numbered lines) to the note editor's
    /// scripture pane.
    /// </summary>
    public const string NoteVersesParameter = "verses";

    /// <summary>
    /// The route for the first-run invitation screen.
    /// </summary>
    public const string Invitation = "invitation";
}
