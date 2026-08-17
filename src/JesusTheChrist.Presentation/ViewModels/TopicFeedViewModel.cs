using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using JesusTheChrist.Core.Content;
using JesusTheChrist.Core.Models;
using JesusTheChrist.Data;
using JesusTheChrist.Presentation.Data;
using JesusTheChrist.Presentation.Navigation;
using JesusTheChrist.Presentation.Platform;

namespace JesusTheChrist.Presentation.ViewModels;

/// <summary>
/// Backs a single sub-topic's reference feed: the verse cards a reader studies,
/// each with its target text, gloss, ±context, and a persisted read checkmark.
/// </summary>
public partial class TopicFeedViewModel : ObservableObject
{
    /// <summary>
    /// Cross-chapter cards at or below this target-verse count start fully expanded; larger spans
    /// open only their first chapter so the view never realizes hundreds of verses at once.
    /// </summary>
    private const int EagerVerseLimit = 60;

    /// <summary>
    /// Paces the cards' copy confirmation. Static so the whole app shares one delegate instance.
    /// </summary>
    private static readonly Func<TimeSpan, Task> DelayAsync = Task.Delay;

    private readonly ContentService content;
    private readonly ReadMarkStore readMarks;
    private readonly NoteStore notes;
    private readonly TopicPositionStore positions;
    private readonly ChapterPositionStore chapterPositions;
    private readonly SettingsStore settings;
    private readonly IDatabaseInitializer databaseInitializer;
    private readonly INavigationService navigation;
    private readonly AppEnvironment environment;

    // Hoisted so the feed hands every card the same delegate. Unlike the method groups below, this
    // one's receiver is a field, so the compiler cannot cache it and each card would otherwise
    // allocate its own.
    private readonly Func<string, Task> copyAsync;

    // Hoisted for the same reason as copyAsync: a field receiver defeats the compiler's
    // method-group caching, so every card would otherwise allocate its own delegate.
    private readonly Func<Uri, Task> openLinkAsync;
    private string topicKey = string.Empty;
    private string? resumeRefId;
    private string? currentRefId;

    /// <summary>
    /// Initializes a new instance of the <see cref="TopicFeedViewModel"/> class.
    /// </summary>
    /// <param name="content">The content service that loads the Topical Guide.</param>
    /// <param name="readMarks">The read-mark store.</param>
    /// <param name="notes">The note store.</param>
    /// <param name="positions">The per-topic reading-position store.</param>
    /// <param name="chapterPositions">The per-reference chapter-position store, for long spans.</param>
    /// <param name="settings">The settings store (language preference).</param>
    /// <param name="databaseInitializer">Ensures the database schema before reads.</param>
    /// <param name="navigation">The navigation service.</param>
    /// <param name="environment">App scope and default language.</param>
    /// <param name="clipboard">The system clipboard, for the cards' copy action.</param>
    /// <param name="links">The platform link opener, for the cards' study links.</param>
    public TopicFeedViewModel(
        ContentService content,
        ReadMarkStore readMarks,
        NoteStore notes,
        TopicPositionStore positions,
        ChapterPositionStore chapterPositions,
        SettingsStore settings,
        IDatabaseInitializer databaseInitializer,
        INavigationService navigation,
        AppEnvironment environment,
        IClipboardService clipboard,
        ILinkOpener links)
    {
        ArgumentNullException.ThrowIfNull(clipboard);
        ArgumentNullException.ThrowIfNull(links);

        this.content = content;
        this.readMarks = readMarks;
        this.notes = notes;
        this.positions = positions;
        this.chapterPositions = chapterPositions;
        this.settings = settings;
        this.databaseInitializer = databaseInitializer;
        this.navigation = navigation;
        this.environment = environment;
        this.copyAsync = clipboard.SetTextAsync;
        this.openLinkAsync = links.OpenAsync;
    }

    /// <summary>
    /// Occurs when marking a reference read rolls its card up, so the view can re-anchor the
    /// feed's scroll position to the collapsed card (a tall card otherwise leaves the reader
    /// stranded below the next reference).
    /// </summary>
    public event EventHandler<ReferenceCardEventArgs>? CardCollapsedAfterRead;

    /// <summary>
    /// Occurs when a reader holds a single verse to copy it, so the view can confirm with a toast.
    /// The card's own copy button confirms in place instead and does not raise this.
    /// </summary>
    public event EventHandler? VerseCopied;

    /// <summary>
    /// Occurs when a reader opens a chapter inside a long span, so the view can re-anchor the feed
    /// to that card. Expanding one chapter collapses another, which changes the card's height above
    /// the reader and would otherwise leave the chapter they just chose partway down the viewport.
    /// </summary>
    public event EventHandler<ReferenceCardEventArgs>? ChapterExpanded;

    /// <summary>
    /// Gets or sets the sub-topic title shown at the top of the feed.
    /// </summary>
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether content is being loaded.
    /// </summary>
    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    /// <summary>
    /// Gets the reference cards in the sub-topic, in Topical Guide order.
    /// </summary>
    public ObservableCollection<ReferenceCardViewModel> References { get; } = new();

    /// <summary>
    /// Loads the references for the given sub-topic key.
    /// </summary>
    /// <param name="key">The sub-topic's language-invariant key.</param>
    /// <returns>A task that completes when the feed is populated.</returns>
    public async Task LoadAsync(string key)
    {
        if (this.IsLoading)
        {
            return;
        }

        this.IsLoading = true;
        try
        {
            await this.databaseInitializer.EnsureInitializedAsync();

            this.topicKey = key;
            this.resumeRefId = await this.positions.GetAsync(key);

            // Start the live pointer at the resume position so leaving without scrolling
            // re-persists the same reference rather than wiping it.
            this.currentRefId = this.resumeRefId;

            var language = await this.ResolveLanguageAsync();
            var guide = await this.content.LoadAsync(language, this.environment.Scope);
            var subTopic = guide.SubTopics.FirstOrDefault(s => s.Key == key);

            this.References.Clear();
            if (subTopic is null)
            {
                this.Title = string.Empty;
                return;
            }

            this.Title = subTopic.Title;
            var readIds = await this.readMarks.GetReadIdsAsync();
            var noteIds = await this.notes.GetNoteIdsAsync();
            var savedChapters = await this.chapterPositions.GetAllAsync();

            foreach (var reference in subTopic.References)
            {
                var id = reference.Id(key);
                var context = reference.Context
                    .Select(c => new ContextLineViewModel(c.Vs, c.Text, c.Target))
                    .ToList();

                // Group the target verses once. Every card is built up front, so each extra pass
                // over a reference is paid for the whole sub-topic — and the segments already hold
                // exactly the target verses, so the threshold can be counted straight off them.
                var segments = reference.TargetSegments();
                var expandAll = segments.Sum(s => s.Verses.Count) <= EagerVerseLimit;

                // Only a span too large to open at once gets the accordion and its remembered
                // chapter: on a short span that would take away a passage the reader can otherwise
                // read straight through.
                var usesChapterMemory = reference.SpansChapters && !expandAll;

                this.References.Add(new ReferenceCardViewModel(
                    id,
                    reference.RefLabel,
                    reference.TargetText,
                    reference.ShowGloss ? reference.Note : null,
                    context,
                    BuildSegments(
                        reference,
                        segments,
                        expandAll,
                        language,
                        this.openLinkAsync,
                        savedChapters.TryGetValue(id, out var savedCh) ? savedCh : null),
                    readIds.Contains(id),
                    noteIds.Contains(id),
                    this.SetReadAsync,
                    this.OpenNoteAsync,
                    this.OnCardCollapsedAfterRead,
                    this.copyAsync,
                    this.CopyVerseAsync,
                    StudyUri(ScriptureUrlBuilder.Build(reference, language)),
                    this.openLinkAsync,
                    usesChapterMemory,
                    this.SaveChapterAsync,
                    DelayAsync));
            }
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    /// <summary>
    /// Parses a built study URL. A URL we cannot parse yields <see langword="null"/> rather than
    /// throwing: a bad book code somewhere in the corpus should cost that card its link, not break
    /// the whole feed's load.
    /// </summary>
    /// <param name="url">The URL built by <see cref="ScriptureUrlBuilder"/>.</param>
    /// <returns>The absolute URI, or <see langword="null"/> when it does not parse.</returns>
    private static Uri? StudyUri(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri : null;

    /// <summary>
    /// Builds a card's per-chapter segments. A single-chapter reference yields one header-less
    /// segment shown in full. A cross-chapter reference yields one segment per chapter with a
    /// header; a card with chapter memory opens exactly one of them, and otherwise every chapter
    /// past the first opens only when the whole passage is small enough to realize up front.
    /// </summary>
    /// <param name="reference">The reference to lay out.</param>
    /// <param name="segments">The reference's chapter segments, grouped once by the caller.</param>
    /// <param name="expandAll">Whether the passage is small enough to open every chapter at once.</param>
    /// <param name="language">The reader's language, for the chapters' study links.</param>
    /// <param name="openLinkAsync">Opens an external link.</param>
    /// <param name="savedCh">The chapter the reader was last in, or <see langword="null"/>.</param>
    /// <returns>The chapter segments backing the card.</returns>
    private static List<ChapterSegmentViewModel> BuildSegments(
        Reference reference,
        IReadOnlyList<ChapterSegment> segments,
        bool expandAll,
        Language language,
        Func<Uri, Task> openLinkAsync,
        int? savedCh)
    {
        var spans = reference.SpansChapters;

        // The same condition as the card's chapter memory: a long span navigates by strip, so its
        // closed chapters draw nothing and the open one sits directly under the strip.
        var inStrip = spans && !expandAll;

        // A card with chapter memory opens exactly one chapter: the one the reader was last in,
        // or the first when nothing is remembered (or the remembered chapter no longer exists,
        // which a corpus revision could cause).
        var openCh = !inStrip
            ? (int?)null
            : savedCh is int saved && segments.Any(s => s.Ch == saved) ? saved : segments[0].Ch;

        var cards = new List<ChapterSegmentViewModel>(segments.Count);
        for (var i = 0; i < segments.Count; i++)
        {
            var verses = segments[i].Verses
                .Select(v => new ContextLineViewModel(v.Vs, v.Text, v.Target))
                .ToList();
            var isExpanded = openCh is int only ? segments[i].Ch == only : !spans || i == 0 || expandAll;
            cards.Add(new ChapterSegmentViewModel(
                segments[i].Ch,
                segments[i].ChapterLabel,
                spans,
                verses,
                isExpanded,
                StudyUri(ScriptureUrlBuilder.Build(reference, segments[i], language)),
                openLinkAsync,
                inStrip));
        }

        return cards;
    }

    /// <summary>
    /// Refreshes each card's note indicator (call after returning from the note editor).
    /// </summary>
    /// <returns>A task that completes when the indicators are refreshed.</returns>
    public async Task RefreshNotesAsync()
    {
        var noteIds = await this.notes.GetNoteIdsAsync();
        foreach (var card in this.References)
        {
            card.HasNote = noteIds.Contains(card.Id);
        }
    }

    /// <summary>
    /// The card to restore to the top of the feed on entry — the reference last seen there when
    /// the reader left this topic — or <see langword="null"/> when there is no saved position
    /// (or it no longer matches a loaded card).
    /// </summary>
    /// <returns>The card to scroll to, or <see langword="null"/>.</returns>
    public ReferenceCardViewModel? ResumeCard() =>
        string.IsNullOrEmpty(this.resumeRefId)
            ? null
            : this.References.FirstOrDefault(c => c.Id == this.resumeRefId);

    /// <summary>
    /// Records the reference currently at the top of the feed as the reader scrolls, so it can
    /// be persisted as the resume position. Held in memory; <see cref="SavePositionAsync"/>
    /// commits it.
    /// </summary>
    /// <param name="refId">The reference identifier at the top of the viewport.</param>
    public void RecordVisible(string refId)
    {
        if (!string.IsNullOrEmpty(refId))
        {
            this.currentRefId = refId;
        }
    }

    /// <summary>
    /// Persists the current top-of-feed reference as this topic's resume position. No-op until a
    /// topic has been loaded and a position is known.
    /// </summary>
    /// <returns>A task that completes when the position is saved.</returns>
    public Task SavePositionAsync() =>
        string.IsNullOrEmpty(this.topicKey) || string.IsNullOrEmpty(this.currentRefId)
            ? Task.CompletedTask
            : this.positions.SaveAsync(this.topicKey, this.currentRefId);

    private void OnCardCollapsedAfterRead(ReferenceCardViewModel card) =>
        this.CardCollapsedAfterRead?.Invoke(this, new ReferenceCardEventArgs(card));

    /// <summary>
    /// Copies a held verse and asks the view to confirm it. The card knows only that it is copying a
    /// verse; whether that draws a toast is the feed's business.
    /// </summary>
    /// <param name="text">The verse's clipboard form.</param>
    /// <returns>A task that completes once the clipboard has been written.</returns>
    private async Task CopyVerseAsync(string text)
    {
        await this.copyAsync(text);
        this.VerseCopied?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Remembers the chapter a reader opened and asks the view to re-anchor to its card. The card
    /// knows only that it changed chapters; whether the feed scrolls is the feed's business, which
    /// is why this wraps the store rather than handing the card the store's method directly.
    /// </summary>
    /// <param name="refId">The reference whose chapter changed.</param>
    /// <param name="ch">The chapter the reader opened.</param>
    /// <returns>A task that completes once the chapter has been persisted.</returns>
    private async Task SaveChapterAsync(string refId, int ch)
    {
        await this.chapterPositions.SaveAsync(refId, ch);

        var card = this.References.FirstOrDefault(c => c.Id == refId);
        if (card is not null)
        {
            this.ChapterExpanded?.Invoke(this, new ReferenceCardEventArgs(card));
        }
    }

    private Task OpenNoteAsync(ReferenceCardViewModel card) =>
        this.navigation.GoToAsync(
            NavigationRoutes.Note,
            new Dictionary<string, object>
            {
                [NavigationRoutes.NoteRefIdParameter] = card.Id,
                [NavigationRoutes.NoteRefLabelParameter] = card.RefLabel,
                [NavigationRoutes.NoteVersesParameter] = card.Verses,
            });

    private Task SetReadAsync(string id, bool isRead) =>
        isRead ? this.readMarks.MarkReadAsync(id) : this.readMarks.UnmarkAsync(id);

    private async Task<Language> ResolveLanguageAsync()
    {
        var saved = await this.settings.GetAsync(SettingKeys.Language);
        return string.IsNullOrWhiteSpace(saved)
            ? this.environment.DefaultLanguage
            : LanguageResolver.Resolve(saved);
    }
}
