namespace JesusTheChrist.Presentation.Markdown;

/// <summary>
/// The kind of an inline run within a markdown block.
/// </summary>
public enum MarkdownInlineKind
{
    /// <summary>Plain text.</summary>
    Text,

    /// <summary>Bold text.</summary>
    Bold,

    /// <summary>Italic text.</summary>
    Italic,

    /// <summary>A hyperlink.</summary>
    Link,
}
