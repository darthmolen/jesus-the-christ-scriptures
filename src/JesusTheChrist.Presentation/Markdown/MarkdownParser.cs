using System.Text.RegularExpressions;

namespace JesusTheChrist.Presentation.Markdown;

/// <summary>
/// Parses a small, fixed subset of markdown — headings, paragraphs, bulleted list
/// items, and the inline runs bold, italic, and links — into a block list for rendering.
/// </summary>
public static partial class MarkdownParser
{
    /// <summary>
    /// Parses markdown source into a list of blocks.
    /// </summary>
    /// <param name="markdown">The markdown source.</param>
    /// <returns>The parsed blocks in document order.</returns>
    public static IReadOnlyList<MarkdownBlock> Parse(string? markdown)
    {
        var blocks = new List<MarkdownBlock>();
        var lines = (markdown ?? string.Empty).Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n');

        var paragraph = new List<string>();
        List<string>? listItem = null;

        void FlushParagraph()
        {
            if (paragraph.Count > 0)
            {
                var text = string.Join(" ", paragraph).Trim();
                if (text.Length > 0)
                {
                    blocks.Add(new MarkdownBlock(MarkdownBlockKind.Paragraph, ParseInlines(text)));
                }

                paragraph.Clear();
            }
        }

        void FlushListItem()
        {
            if (listItem is not null)
            {
                var text = string.Join(" ", listItem).Trim();
                if (text.Length > 0)
                {
                    blocks.Add(new MarkdownBlock(MarkdownBlockKind.ListItem, ParseInlines(text)));
                }

                listItem = null;
            }
        }

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                FlushParagraph();
                FlushListItem();
                continue;
            }

            var heading = HeadingRegex().Match(line);
            if (heading.Success)
            {
                FlushParagraph();
                FlushListItem();
                blocks.Add(new MarkdownBlock(
                    MarkdownBlockKind.Heading,
                    ParseInlines(heading.Groups["text"].Value.Trim()),
                    heading.Groups["hashes"].Value.Length));
                continue;
            }

            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("- ", StringComparison.Ordinal))
            {
                FlushParagraph();
                FlushListItem();
                listItem = [trimmed[2..]];
                continue;
            }

            if (listItem is not null)
            {
                listItem.Add(trimmed);
                continue;
            }

            paragraph.Add(line.Trim());
        }

        FlushParagraph();
        FlushListItem();
        return blocks;
    }

    /// <summary>
    /// Parses the inline runs (bold, italic, link, plain text) within a single line of text.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <returns>The inline runs in order.</returns>
    public static IReadOnlyList<MarkdownInline> ParseInlines(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var inlines = new List<MarkdownInline>();
        var last = 0;

        foreach (Match match in InlineRegex().Matches(text))
        {
            if (match.Index > last)
            {
                inlines.Add(new MarkdownInline(MarkdownInlineKind.Text, text[last..match.Index]));
            }

            if (match.Groups["linktext"].Success)
            {
                inlines.Add(new MarkdownInline(MarkdownInlineKind.Link, match.Groups["linktext"].Value, match.Groups["linkurl"].Value));
            }
            else if (match.Groups["auto"].Success)
            {
                inlines.Add(new MarkdownInline(MarkdownInlineKind.Link, match.Groups["auto"].Value, match.Groups["auto"].Value));
            }
            else if (match.Groups["bold"].Success)
            {
                inlines.Add(new MarkdownInline(MarkdownInlineKind.Bold, match.Groups["bold"].Value));
            }
            else if (match.Groups["italic"].Success)
            {
                inlines.Add(new MarkdownInline(MarkdownInlineKind.Italic, match.Groups["italic"].Value));
            }

            last = match.Index + match.Length;
        }

        if (last < text.Length)
        {
            inlines.Add(new MarkdownInline(MarkdownInlineKind.Text, text[last..]));
        }

        if (inlines.Count == 0)
        {
            inlines.Add(new MarkdownInline(MarkdownInlineKind.Text, text));
        }

        return inlines;
    }

    [GeneratedRegex(@"^(?<hashes>#{1,6})\s+(?<text>.*)$")]
    private static partial Regex HeadingRegex();

    [GeneratedRegex(@"\[(?<linktext>[^\]]+)\]\((?<linkurl>[^)]+)\)|<(?<auto>https?://[^>]+)>|\*\*(?<bold>[^*]+)\*\*|\*(?<italic>[^*]+)\*")]
    private static partial Regex InlineRegex();
}
