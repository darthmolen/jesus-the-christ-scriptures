using System.Diagnostics.CodeAnalysis;

namespace JesusTheChrist.Presentation.Markdown;

/// <summary>
/// An inline run of text within a markdown block.
/// </summary>
public sealed class MarkdownInline
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownInline"/> class.
    /// </summary>
    /// <param name="kind">The inline kind.</param>
    /// <param name="text">The display text.</param>
    /// <param name="url">The link target, for <see cref="MarkdownInlineKind.Link"/>.</param>
    [SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "Markdown link targets are carried as raw source text and handed to the renderer.")]
    public MarkdownInline(MarkdownInlineKind kind, string text, string? url = null)
    {
        this.Kind = kind;
        this.Text = text;
        this.Url = url;
    }

    /// <summary>
    /// Gets the inline kind.
    /// </summary>
    public MarkdownInlineKind Kind { get; }

    /// <summary>
    /// Gets the display text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the link target, or null when this is not a link.
    /// </summary>
    [SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "Markdown link targets are carried as raw source text and handed to the renderer.")]
    public string? Url { get; }
}
