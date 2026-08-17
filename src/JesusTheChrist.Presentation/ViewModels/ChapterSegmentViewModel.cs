using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JesusTheChrist.Presentation.ViewModels;

/// <summary>
/// One chapter's worth of a reference's target verses. A single-chapter reference has exactly one
/// segment with no header; a cross-chapter reference has one per chapter, each with a tappable
/// header that lazily realizes its verses so large spans never build hundreds of views at once.
/// </summary>
public partial class ChapterSegmentViewModel : ObservableObject
{
    private static readonly IReadOnlyList<ContextLineViewModel> NoVerses = [];

    private readonly Func<Uri, Task> openLinkAsync;

    private Func<ChapterSegmentViewModel, Task>? onExpanded;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChapterSegmentViewModel"/> class.
    /// </summary>
    /// <param name="ch">The chapter number this segment covers.</param>
    /// <param name="chapterLabel">The chapter header text, for example "Matthew 10".</param>
    /// <param name="showHeader">Whether to show the chapter header (true only for spanning cards).</param>
    /// <param name="verses">The chapter's target verses, as numbered lines.</param>
    /// <param name="isExpanded">Whether the segment starts expanded.</param>
    /// <param name="studyUri">This chapter's churchofjesuschrist.org study link.</param>
    /// <param name="openLinkAsync">Opens an external link.</param>
    public ChapterSegmentViewModel(
        int ch,
        string chapterLabel,
        bool showHeader,
        IReadOnlyList<ContextLineViewModel> verses,
        bool isExpanded,
        Uri? studyUri,
        Func<Uri, Task> openLinkAsync)
    {
        this.Ch = ch;
        this.ChapterLabel = chapterLabel;
        this.ShowHeader = showHeader;
        this.Verses = verses;
        this.IsExpanded = isExpanded;
        this.StudyUri = studyUri;
        this.openLinkAsync = openLinkAsync;
    }

    /// <summary>
    /// Gets the chapter number this segment covers.
    /// </summary>
    public int Ch { get; }

    /// <summary>
    /// Gets the chapter header text, for example "Matthew 10".
    /// </summary>
    public string ChapterLabel { get; }

    /// <summary>
    /// Gets this chapter's study link on churchofjesuschrist.org, or <see langword="null"/> when
    /// one could not be built.
    /// </summary>
    public Uri? StudyUri { get; }

    /// <summary>
    /// Gets a value indicating whether this chapter has a study link to offer. Gates the link
    /// affordance, so a reference we cannot build a URL for simply shows no link.
    /// </summary>
    public bool HasStudyLink => this.ShowHeader && this.StudyUri is not null;

    /// <summary>
    /// Gets a value indicating whether the chapter header is shown. Single-chapter references hide
    /// it so they render exactly as before.
    /// </summary>
    public bool ShowHeader { get; }

    /// <summary>
    /// Gets this chapter's target verses, as numbered lines.
    /// </summary>
    public IReadOnlyList<ContextLineViewModel> Verses { get; }

    /// <summary>
    /// Gets the verse count, for the collapsed header affordance.
    /// </summary>
    public int VerseCount => this.Verses.Count;

    /// <summary>
    /// Gets or sets a value indicating whether this chapter's verses are shown.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VisibleVerses))]
    [NotifyPropertyChangedFor(nameof(ChevronGlyph))]
    public partial bool IsExpanded { get; set; }

    /// <summary>
    /// Gets the verses to bind. Collapsed segments expose an empty list so the bound layout builds
    /// no child views — this is what bounds view realization for large spans.
    /// </summary>
    public IReadOnlyList<ContextLineViewModel> VisibleVerses => this.IsExpanded ? this.Verses : NoVerses;

    /// <summary>
    /// Gets the chevron glyph signalling whether the header expands or collapses the chapter.
    /// </summary>
    public string ChevronGlyph => this.IsExpanded ? "▾" : "▸";

    /// <summary>
    /// Registers the owner notified when this chapter is expanded. The card uses it to collapse the
    /// other chapters and remember this one; a segment holds no reference to its siblings, so the
    /// card is the only place that can see all of them.
    /// </summary>
    /// <param name="listener">Invoked with this segment whenever it expands.</param>
    public void AttachExpansionListener(Func<ChapterSegmentViewModel, Task> listener) =>
        this.onExpanded = listener;

    [RelayCommand]
    private async Task ToggleExpandedAsync()
    {
        this.IsExpanded = !this.IsExpanded;

        // Only expanding is reported. Collapsing the open chapter is allowed — on a long span it
        // turns the card into a chapter index — and must not clear the remembered chapter.
        if (this.IsExpanded && this.onExpanded is not null)
        {
            await this.onExpanded(this);
        }
    }

    [RelayCommand]
    private Task OpenStudyAsync() =>
        this.StudyUri is null ? Task.CompletedTask : this.openLinkAsync(this.StudyUri);
}
