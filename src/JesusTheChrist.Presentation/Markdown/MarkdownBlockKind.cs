namespace JesusTheChrist.Presentation.Markdown;

/// <summary>
/// The kind of a markdown block.
/// </summary>
public enum MarkdownBlockKind
{
    /// <summary>A heading (see <see cref="MarkdownBlock.Level"/>).</summary>
    Heading,

    /// <summary>A paragraph.</summary>
    Paragraph,

    /// <summary>A bulleted list item.</summary>
    ListItem,
}
