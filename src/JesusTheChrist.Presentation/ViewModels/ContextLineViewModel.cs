namespace JesusTheChrist.Presentation.ViewModels;

/// <summary>
/// One verse line within a reference's ±context window.
/// </summary>
public sealed class ContextLineViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContextLineViewModel"/> class.
    /// </summary>
    /// <param name="verse">The verse number.</param>
    /// <param name="text">The verse text.</param>
    /// <param name="isTarget">Whether this verse is one of the reference's target verses.</param>
    public ContextLineViewModel(int verse, string text, bool isTarget)
    {
        this.Verse = verse;
        this.Text = text;
        this.IsTarget = isTarget;
    }

    /// <summary>
    /// Gets the verse number.
    /// </summary>
    public int Verse { get; }

    /// <summary>
    /// Gets the verse text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets a value indicating whether this verse is targeted by the reference.
    /// </summary>
    public bool IsTarget { get; }
}
