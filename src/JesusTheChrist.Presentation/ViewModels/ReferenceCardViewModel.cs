using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JesusTheChrist.Presentation.ViewModels;

/// <summary>
/// One reference card in a topic feed: the reference label, its target verse text,
/// an optional gloss, the ±context window, and a persisted read state.
/// </summary>
public partial class ReferenceCardViewModel : ObservableObject
{
    /// <summary>
    /// How long the copy button confirms with "Copied" before reverting to its usual label.
    /// </summary>
    public static readonly TimeSpan CopiedFeedbackDuration = TimeSpan.FromSeconds(2);

    private readonly Func<string, bool, Task> setReadAsync;
    private readonly Func<ReferenceCardViewModel, Task> openNoteAsync;
    private readonly Action<ReferenceCardViewModel> onReadCollapsed;
    private readonly Func<string, Task> copyAsync;
    private readonly Func<string, Task> copyVerseAsync;
    private readonly Func<Uri, Task> openLinkAsync;
    private readonly Func<string, int, Task> saveChapterAsync;
    private readonly Func<TimeSpan, Task> delayAsync;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferenceCardViewModel"/> class.
    /// </summary>
    /// <param name="id">The stable reference id used to key read state.</param>
    /// <param name="refLabel">The reference label, for example "Heb. 7:25".</param>
    /// <param name="verseText">The joined target verse text.</param>
    /// <param name="gloss">The note gloss to show, or null when it adds nothing.</param>
    /// <param name="context">The ±context verse window.</param>
    /// <param name="segments">The target verses grouped into per-chapter segments for display.</param>
    /// <param name="isRead">Whether the reference is already marked read.</param>
    /// <param name="hasNote">Whether a saved note exists for the reference.</param>
    /// <param name="setReadAsync">Persists a new read state for the given id.</param>
    /// <param name="openNoteAsync">Opens the note editor for the given card.</param>
    /// <param name="onReadCollapsed">Notified when marking read rolls this card up, so the view can re-anchor scroll.</param>
    /// <param name="copyAsync">Places the given text on the system clipboard.</param>
    /// <param name="copyVerseAsync">Copies a single held verse, which the feed also confirms with a toast.</param>
    /// <param name="studyUri">The reference's churchofjesuschrist.org study link.</param>
    /// <param name="openLinkAsync">Opens an external link.</param>
    /// <param name="usesChapterMemory">
    /// Whether this card's chapters behave as an accordion and remember which one was open. True
    /// only for a spanning reference too large to show every chapter at once.
    /// </param>
    /// <param name="saveChapterAsync">Persists the chapter the reader opened within this reference.</param>
    /// <param name="delayAsync">Waits the given duration, so the copy confirmation can be paced.</param>
    public ReferenceCardViewModel(
        string id,
        string refLabel,
        string verseText,
        string? gloss,
        IReadOnlyList<ContextLineViewModel> context,
        IReadOnlyList<ChapterSegmentViewModel> segments,
        bool isRead,
        bool hasNote,
        Func<string, bool, Task> setReadAsync,
        Func<ReferenceCardViewModel, Task> openNoteAsync,
        Action<ReferenceCardViewModel> onReadCollapsed,
        Func<string, Task> copyAsync,
        Func<string, Task> copyVerseAsync,
        Uri? studyUri,
        Func<Uri, Task> openLinkAsync,
        bool usesChapterMemory,
        Func<string, int, Task> saveChapterAsync,
        Func<TimeSpan, Task> delayAsync)
    {
        // Guarded because the accordion wiring below enumerates it on the spot; its siblings are
        // only stored, which is why CA1062 singles this one out.
        ArgumentNullException.ThrowIfNull(segments);

        this.Id = id;
        this.RefLabel = refLabel;
        this.VerseText = verseText;
        this.Gloss = gloss;
        this.Context = context;
        this.Segments = segments;
        this.Verses = context.Where(c => c.IsTarget).ToList();
        this.setReadAsync = setReadAsync;
        this.openNoteAsync = openNoteAsync;
        this.onReadCollapsed = onReadCollapsed;
        this.copyAsync = copyAsync;
        this.copyVerseAsync = copyVerseAsync;
        this.StudyUri = studyUri;
        this.openLinkAsync = openLinkAsync;
        this.UsesChapterMemory = usesChapterMemory;
        this.saveChapterAsync = saveChapterAsync;
        this.delayAsync = delayAsync;

        if (usesChapterMemory)
        {
            foreach (var segment in segments)
            {
                segment.AttachExpansionListener(this.OnSegmentExpandedAsync);
            }
        }

        this.IsRead = isRead;
        this.HasNote = hasNote;
        this.IsExpanded = !isRead;
    }

    /// <summary>
    /// Gets the stable reference id.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the reference label.
    /// </summary>
    public string RefLabel { get; }

    /// <summary>
    /// Gets the reference's study link on churchofjesuschrist.org, or <see langword="null"/> when
    /// one could not be built. A cross-chapter reference links to the first chapter of its span;
    /// each chapter header carries its own link.
    /// </summary>
    public Uri? StudyUri { get; }

    /// <summary>
    /// Gets a value indicating whether the reference has a study link to offer. Gates the link
    /// affordance, so a reference we cannot build a URL for simply shows no link.
    /// </summary>
    public bool HasStudyLink => this.StudyUri is not null;

    /// <summary>
    /// Gets a value indicating whether this card's chapters behave as an accordion — at most one
    /// open at a time — and remember which one the reader was in across visits. True only for a
    /// spanning reference too large to open every chapter at once, since forcing an accordion on a
    /// short span would take away a passage the reader can otherwise read straight through.
    /// </summary>
    public bool UsesChapterMemory { get; }

    /// <summary>
    /// Gets the joined target verse text (no verse numbers).
    /// </summary>
    public string VerseText { get; }

    /// <summary>
    /// Gets the target verses as individual numbered lines, for scripture-style display.
    /// </summary>
    public IReadOnlyList<ContextLineViewModel> Verses { get; }

    /// <summary>
    /// Gets the target verses grouped into per-chapter segments. A single-chapter reference has one
    /// header-less segment; a cross-chapter reference has one per chapter with tappable headers.
    /// </summary>
    public IReadOnlyList<ChapterSegmentViewModel> Segments { get; }

    /// <summary>
    /// Gets the note gloss, or null when it adds nothing beyond the verse.
    /// </summary>
    public string? Gloss { get; }

    /// <summary>
    /// Gets a value indicating whether a gloss should be shown.
    /// </summary>
    public bool HasGloss => !string.IsNullOrWhiteSpace(this.Gloss);

    /// <summary>
    /// Gets the ±context verse window.
    /// </summary>
    public IReadOnlyList<ContextLineViewModel> Context { get; }

    /// <summary>
    /// Gets a value indicating whether the reference has any context verses.
    /// </summary>
    public bool HasContext => this.Context.Count > 0;

    /// <summary>
    /// Gets or sets a value indicating whether the reference is marked read.
    /// </summary>
    [ObservableProperty]
    public partial bool IsRead { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the context window is expanded.
    /// </summary>
    [ObservableProperty]
    public partial bool IsContextVisible { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a saved note exists for this reference.
    /// </summary>
    [ObservableProperty]
    public partial bool HasNote { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the card is currently confirming a copy, so the
    /// copy button can read "Copied" for a moment.
    /// </summary>
    [ObservableProperty]
    public partial bool JustCopied { get; set; }

    /// <summary>
    /// Gets the clipboard form of the whole reference: its label, then one numbered line per target
    /// verse, with a chapter heading before each chapter of a cross-chapter reference.
    /// </summary>
    /// <remarks>
    /// Built on first use and cached. Every card in a sub-topic is constructed up front by
    /// <c>LoadAsync</c> — a card can hold hundreds of verses — so joining this in the constructor
    /// would charge every reader the cost of copying references they never copy.
    /// </remarks>
    public string CopyText => field ??= this.BuildCopyText();

    /// <summary>
    /// Gets or sets a value indicating whether the card body is expanded. When false, only
    /// the heading shows so the feed reads as a progress checklist; tapping the heading or
    /// un-reading the card expands it again.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChevronGlyph))]
    [NotifyPropertyChangedFor(nameof(IsCollapsed))]
    public partial bool IsExpanded { get; set; }

    /// <summary>
    /// Gets a value indicating whether the card body is rolled up to its heading. The note and
    /// read actions live in the heading while collapsed and move into the footer while expanded,
    /// so a reader never has to scroll back up to mark a long passage read or start a note.
    /// </summary>
    public bool IsCollapsed => !this.IsExpanded;

    /// <summary>
    /// Gets the chevron glyph that signals whether the heading expands or collapses the body.
    /// </summary>
    public string ChevronGlyph => this.IsExpanded ? "▾" : "▸";

    [RelayCommand]
    private async Task ToggleReadAsync()
    {
        var next = !this.IsRead;
        await this.setReadAsync(this.Id, next);
        this.IsRead = next;

        // Marking read rolls the card up to its heading; un-reading rolls it back out.
        this.IsExpanded = !next;

        // When a card taller than the screen rolls up, the feed keeps its old scroll offset
        // and strands the reader below the next card. Let the view re-anchor on collapse.
        if (next)
        {
            this.onReadCollapsed(this);
        }
    }

    [RelayCommand]
    private void ToggleExpanded() => this.IsExpanded = !this.IsExpanded;

    [RelayCommand]
    private void ToggleContext() => this.IsContextVisible = !this.IsContextVisible;

    [RelayCommand]
    private Task OpenNoteAsync() => this.openNoteAsync(this);

    [RelayCommand]
    private Task OpenStudyAsync() =>
        this.StudyUri is null ? Task.CompletedTask : this.openLinkAsync(this.StudyUri);

    /// <summary>
    /// Collapses the other chapters and remembers this one. A reader working through a passage the
    /// size of 3 Nephi 11–26 otherwise reopens at its first chapter every visit, having to re-find
    /// where they were.
    /// </summary>
    /// <param name="expanded">The chapter the reader just opened.</param>
    /// <returns>A task that completes once the chapter has been persisted.</returns>
    private Task OnSegmentExpandedAsync(ChapterSegmentViewModel expanded)
    {
        foreach (var segment in this.Segments)
        {
            if (!ReferenceEquals(segment, expanded))
            {
                segment.IsExpanded = false;
            }
        }

        return this.saveChapterAsync(this.Id, expanded.Ch);
    }

    [RelayCommand]
    private async Task CopyAsync()
    {
        await this.copyAsync(this.CopyText);

        // The generated AsyncRelayCommand reports CanExecute false while it runs, so the button
        // is also disabled for as long as it reads "Copied".
        this.JustCopied = true;
        await this.delayAsync(CopiedFeedbackDuration);
        this.JustCopied = false;
    }

    [RelayCommand]
    private Task CopyVerseAsync(ContextLineViewModel? verse) =>
        verse is null ? Task.CompletedTask : this.copyVerseAsync(this.BuildVerseCopyText(verse));

    /// <summary>
    /// Joins a single verse into its clipboard form: its book, chapter and verse as a heading, then
    /// the verse text on the next line. The number is not repeated at the head of the prose.
    /// </summary>
    /// <param name="verse">The verse line the reader held.</param>
    /// <returns>The verse's heading followed by its text.</returns>
    private string BuildVerseCopyText(ContextLineViewModel verse) =>
        $"{this.ChapterLabelFor(verse)}:{verse.Verse}\n{verse.Text}";

    /// <summary>
    /// Finds the localized book-and-chapter label a verse line belongs to. Verse lines carry no
    /// chapter of their own, so the owning segment is found by identity — a scan that costs nothing
    /// until a reader actually holds a verse.
    /// </summary>
    /// <param name="verse">The verse line to attribute.</param>
    /// <returns>The chapter label, for example "Matthew 10".</returns>
    private string ChapterLabelFor(ContextLineViewModel verse)
    {
        foreach (var segment in this.Segments)
        {
            if (segment.Verses.Any(v => ReferenceEquals(v, verse)))
            {
                return segment.ChapterLabel;
            }
        }

        // The line came from the ±context window, which materializes its own instances and so never
        // matches above. Those verses belong to the reference's own chapter: of the whole corpus only
        // three references span chapters, and none of them carries a non-target context verse. Should
        // that ever change, a spanning reference's context would need its chapter carried through.
        return this.Segments.Count > 0 ? this.Segments[0].ChapterLabel : this.RefLabel;
    }

    /// <summary>
    /// Joins the reference into its clipboard form. Built from the per-chapter segments rather than
    /// the context window: segments carry the chapter labels, exclude the ±context verses, and hold
    /// every target verse even for a chapter the reader has collapsed on screen — a copy carries the
    /// whole passage, not whatever happens to be realized.
    /// </summary>
    /// <returns>The reference label followed by its numbered verses.</returns>
    private string BuildCopyText()
    {
        // "\n" rather than Environment.NewLine so the text is identical on the test host and on device.
        var lines = new List<string> { this.RefLabel };
        foreach (var segment in this.Segments)
        {
            if (segment.ShowHeader)
            {
                lines.Add(segment.ChapterLabel);
            }

            lines.AddRange(segment.Verses.Select(verse => $"{verse.Verse} {verse.Text}"));
        }

        return string.Join('\n', lines);
    }
}
