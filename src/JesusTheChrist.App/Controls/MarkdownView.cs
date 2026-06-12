using JesusTheChrist.Presentation.Markdown;

namespace JesusTheChrist.App.Controls;

/// <summary>
/// Renders a small subset of markdown (headings, paragraphs, list items, with bold,
/// italic, and tappable links) into a stacked view.
/// </summary>
public sealed class MarkdownView : ContentView
{
    /// <summary>
    /// Bindable backing store for the <see cref="Source"/> property.
    /// </summary>
    public static readonly BindableProperty SourceProperty = BindableProperty.Create(
        nameof(Source),
        typeof(string),
        typeof(MarkdownView),
        default(string),
        propertyChanged: OnSourceChanged);

    private static readonly Color LinkColor = Color.FromArgb("#4D8DFF");

    /// <summary>
    /// Gets or sets the markdown source to render.
    /// </summary>
    public string? Source
    {
        get => (string?)this.GetValue(SourceProperty);
        set => this.SetValue(SourceProperty, value);
    }

    private static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((MarkdownView)bindable).Render((string?)newValue);
    }

    private void Render(string? markdown)
    {
        var layout = new VerticalStackLayout { Spacing = 16 };

        foreach (var block in MarkdownParser.Parse(markdown))
        {
            View view = block.Kind switch
            {
                MarkdownBlockKind.Heading => HeadingLabel(block),
                MarkdownBlockKind.ListItem => ListItemView(block),
                MarkdownBlockKind.Paragraph => ParagraphLabel(block),
                _ => ParagraphLabel(block),
            };

            AddBlockLinkTap(view, block);
            layout.Add(view);
        }

        this.Content = layout;
    }

    /// <summary>
    /// Makes the whole block tappable when it contains exactly one link, so the link opens
    /// reliably even where per-span tap gestures are unreliable (notably Android).
    /// </summary>
    private static void AddBlockLinkTap(View view, MarkdownBlock block)
    {
        var links = block.Inlines
            .Where(inline => inline.Kind == MarkdownInlineKind.Link && !string.IsNullOrEmpty(inline.Url))
            .ToList();

        if (links.Count == 1)
        {
            var url = links[0].Url!;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => OpenUrl(url);
            view.GestureRecognizers.Add(tap);
        }
    }

    private static void OpenUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            _ = Launcher.Default.OpenAsync(uri);
        }
    }

    private static Label HeadingLabel(MarkdownBlock block)
    {
        var label = BuildLabel(block);
        label.FontAttributes = FontAttributes.Bold;
        label.FontSize = block.Level <= 1 ? 24 : block.Level == 2 ? 19 : 16;
        return label;
    }

    private static Label ParagraphLabel(MarkdownBlock block)
    {
        var label = BuildLabel(block);
        label.FontSize = 16;
        return label;
    }

    private static HorizontalStackLayout ListItemView(MarkdownBlock block)
    {
        var label = BuildLabel(block);
        label.FontSize = 16;
        return new HorizontalStackLayout
        {
            Spacing = 8,
            Children =
            {
                new Label { Text = "•", FontSize = 16 },
                label,
            },
        };
    }

    private static Label BuildLabel(MarkdownBlock block)
    {
        var formatted = new FormattedString();
        foreach (var inline in block.Inlines)
        {
            formatted.Spans.Add(BuildSpan(inline));
        }

        return new Label { FormattedText = formatted, LineHeight = 1.35 };
    }

    private static Span BuildSpan(MarkdownInline inline)
    {
        var span = new Span { Text = inline.Text };
        switch (inline.Kind)
        {
            case MarkdownInlineKind.Bold:
                span.FontAttributes = FontAttributes.Bold;
                break;
            case MarkdownInlineKind.Italic:
                span.FontAttributes = FontAttributes.Italic;
                break;
            case MarkdownInlineKind.Link:
                span.TextColor = LinkColor;
                span.TextDecorations = TextDecorations.Underline;
                if (!string.IsNullOrEmpty(inline.Url))
                {
                    var linkUrl = inline.Url;
                    var tap = new TapGestureRecognizer();
                    tap.Tapped += (_, _) => OpenUrl(linkUrl);
                    span.GestureRecognizers.Add(tap);
                }

                break;
            case MarkdownInlineKind.Text:
                break;
            default:
                break;
        }

        return span;
    }
}
