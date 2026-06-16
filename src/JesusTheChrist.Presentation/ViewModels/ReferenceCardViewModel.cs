using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JesusTheChrist.Presentation.ViewModels;

/// <summary>
/// One reference card in a topic feed: the reference label, its target verse text,
/// an optional gloss, the ±context window, and a persisted read state.
/// </summary>
public partial class ReferenceCardViewModel : ObservableObject
{
    private readonly Func<string, bool, Task> setReadAsync;
    private readonly Func<ReferenceCardViewModel, Task> openNoteAsync;
    private readonly Action<ReferenceCardViewModel> onReadCollapsed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferenceCardViewModel"/> class.
    /// </summary>
    /// <param name="id">The stable reference id used to key read state.</param>
    /// <param name="refLabel">The reference label, for example "Heb. 7:25".</param>
    /// <param name="verseText">The joined target verse text.</param>
    /// <param name="gloss">The note gloss to show, or null when it adds nothing.</param>
    /// <param name="context">The ±context verse window.</param>
    /// <param name="isRead">Whether the reference is already marked read.</param>
    /// <param name="hasNote">Whether a saved note exists for the reference.</param>
    /// <param name="setReadAsync">Persists a new read state for the given id.</param>
    /// <param name="openNoteAsync">Opens the note editor for the given card.</param>
    /// <param name="onReadCollapsed">Notified when marking read rolls this card up, so the view can re-anchor scroll.</param>
    public ReferenceCardViewModel(
        string id,
        string refLabel,
        string verseText,
        string? gloss,
        IReadOnlyList<ContextLineViewModel> context,
        bool isRead,
        bool hasNote,
        Func<string, bool, Task> setReadAsync,
        Func<ReferenceCardViewModel, Task> openNoteAsync,
        Action<ReferenceCardViewModel> onReadCollapsed)
    {
        this.Id = id;
        this.RefLabel = refLabel;
        this.VerseText = verseText;
        this.Gloss = gloss;
        this.Context = context;
        this.Verses = context.Where(c => c.IsTarget).ToList();
        this.setReadAsync = setReadAsync;
        this.openNoteAsync = openNoteAsync;
        this.onReadCollapsed = onReadCollapsed;
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
    /// Gets the joined target verse text (no verse numbers).
    /// </summary>
    public string VerseText { get; }

    /// <summary>
    /// Gets the target verses as individual numbered lines, for scripture-style display.
    /// </summary>
    public IReadOnlyList<ContextLineViewModel> Verses { get; }

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
}
