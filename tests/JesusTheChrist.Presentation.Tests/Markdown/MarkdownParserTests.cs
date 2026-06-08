using JesusTheChrist.Presentation.Markdown;

namespace JesusTheChrist.Presentation.Tests.Markdown;

public class MarkdownParserTests
{
    [Fact]
    public void Heading_ParsesLevelAndText()
    {
        var blocks = MarkdownParser.Parse("# The Invitation");

        var block = Assert.Single(blocks);
        Assert.Equal(MarkdownBlockKind.Heading, block.Kind);
        Assert.Equal(1, block.Level);
        Assert.Equal("The Invitation", Assert.Single(block.Inlines).Text);
    }

    [Fact]
    public void Subheading_ParsesLevelTwo()
    {
        var block = Assert.Single(MarkdownParser.Parse("## The challenge"));
        Assert.Equal(2, block.Level);
    }

    [Fact]
    public void BlankLines_SeparateParagraphs()
    {
        var blocks = MarkdownParser.Parse("First paragraph.\n\nSecond paragraph.");

        Assert.Equal(2, blocks.Count);
        Assert.All(blocks, b => Assert.Equal(MarkdownBlockKind.Paragraph, b.Kind));
    }

    [Fact]
    public void AdjacentLines_JoinIntoOneParagraph()
    {
        var block = Assert.Single(MarkdownParser.Parse("one\ntwo"));
        Assert.Equal("one two", Assert.Single(block.Inlines).Text);
    }

    [Fact]
    public void Bold_ParsesInline()
    {
        var inlines = MarkdownParser.ParseInlines("a **strong** word");

        Assert.Collection(
            inlines,
            i => Assert.Equal(MarkdownInlineKind.Text, i.Kind),
            i =>
            {
                Assert.Equal(MarkdownInlineKind.Bold, i.Kind);
                Assert.Equal("strong", i.Text);
            },
            i => Assert.Equal(MarkdownInlineKind.Text, i.Kind));
    }

    [Fact]
    public void Italic_ParsesInline()
    {
        var inlines = MarkdownParser.ParseInlines("an *emphatic* word");
        Assert.Contains(inlines, i => i.Kind == MarkdownInlineKind.Italic && i.Text == "emphatic");
    }

    [Fact]
    public void Autolink_ParsesUrlAsLink()
    {
        var inline = Assert.Single(MarkdownParser.ParseInlines("<https://example.org/x>"));
        Assert.Equal(MarkdownInlineKind.Link, inline.Kind);
        Assert.Equal("https://example.org/x", inline.Url);
        Assert.Equal("https://example.org/x", inline.Text);
    }

    [Fact]
    public void Link_ParsesTextAndUrl()
    {
        var inline = Assert.Single(MarkdownParser.ParseInlines("[the talk](https://example.org/talk)"));
        Assert.Equal(MarkdownInlineKind.Link, inline.Kind);
        Assert.Equal("the talk", inline.Text);
        Assert.Equal("https://example.org/talk", inline.Url);
    }

    [Fact]
    public void ListItem_WithContinuationLine_JoinsAndParsesLink()
    {
        var markdown = "- **Source** — a talk:\n  <https://example.org/talk>";

        var block = Assert.Single(MarkdownParser.Parse(markdown));
        Assert.Equal(MarkdownBlockKind.ListItem, block.Kind);
        Assert.Contains(block.Inlines, i => i.Kind == MarkdownInlineKind.Bold && i.Text == "Source");
        Assert.Contains(block.Inlines, i => i.Kind == MarkdownInlineKind.Link && i.Url == "https://example.org/talk");
    }

    [Fact]
    public void TwoListItems_ParseSeparately()
    {
        var blocks = MarkdownParser.Parse("- first\n- second");

        Assert.Equal(2, blocks.Count);
        Assert.All(blocks, b => Assert.Equal(MarkdownBlockKind.ListItem, b.Kind));
    }

    [Fact]
    public void PlainText_ReturnsSingleTextInline()
    {
        var inline = Assert.Single(MarkdownParser.ParseInlines("just words"));
        Assert.Equal(MarkdownInlineKind.Text, inline.Kind);
        Assert.Equal("just words", inline.Text);
    }
}
