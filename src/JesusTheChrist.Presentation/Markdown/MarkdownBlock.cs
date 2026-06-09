namespace JesusTheChrist.Presentation.Markdown;

/// <summary>
/// A block-level markdown element (heading, paragraph, or list item) and its inline runs.
/// </summary>
public sealed class MarkdownBlock
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownBlock"/> class.
    /// </summary>
    /// <param name="kind">The block kind.</param>
    /// <param name="inlines">The inline runs that make up the block.</param>
    /// <param name="level">The heading level (1–6); zero for non-headings.</param>
    public MarkdownBlock(MarkdownBlockKind kind, IReadOnlyList<MarkdownInline> inlines, int level = 0)
    {
        this.Kind = kind;
        this.Inlines = inlines;
        this.Level = level;
    }

    /// <summary>
    /// Gets the block kind.
    /// </summary>
    public MarkdownBlockKind Kind { get; }

    /// <summary>
    /// Gets the heading level (1–6), or zero for non-headings.
    /// </summary>
    public int Level { get; }

    /// <summary>
    /// Gets the inline runs that make up the block.
    /// </summary>
    public IReadOnlyList<MarkdownInline> Inlines { get; }
}
