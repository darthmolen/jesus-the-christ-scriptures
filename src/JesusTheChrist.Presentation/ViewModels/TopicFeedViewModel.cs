using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using JesusTheChrist.Core.Content;
using JesusTheChrist.Core.Models;
using JesusTheChrist.Data;
using JesusTheChrist.Presentation.Data;
using JesusTheChrist.Presentation.Navigation;

namespace JesusTheChrist.Presentation.ViewModels;

/// <summary>
/// Backs a single sub-topic's reference feed: the verse cards a reader studies,
/// each with its target text, gloss, ±context, and a persisted read checkmark.
/// </summary>
public partial class TopicFeedViewModel : ObservableObject
{
    private readonly ContentService content;
    private readonly ReadMarkStore readMarks;
    private readonly NoteStore notes;
    private readonly TopicPositionStore positions;
    private readonly SettingsStore settings;
    private readonly IDatabaseInitializer databaseInitializer;
    private readonly INavigationService navigation;
    private readonly AppEnvironment environment;
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
    /// <param name="settings">The settings store (language preference).</param>
    /// <param name="databaseInitializer">Ensures the database schema before reads.</param>
    /// <param name="navigation">The navigation service.</param>
    /// <param name="environment">App scope and default language.</param>
    public TopicFeedViewModel(
        ContentService content,
        ReadMarkStore readMarks,
        NoteStore notes,
        TopicPositionStore positions,
        SettingsStore settings,
        IDatabaseInitializer databaseInitializer,
        INavigationService navigation,
        AppEnvironment environment)
    {
        this.content = content;
        this.readMarks = readMarks;
        this.notes = notes;
        this.positions = positions;
        this.settings = settings;
        this.databaseInitializer = databaseInitializer;
        this.navigation = navigation;
        this.environment = environment;
    }

    /// <summary>
    /// Occurs when marking a reference read rolls its card up, so the view can re-anchor the
    /// feed's scroll position to the collapsed card (a tall card otherwise leaves the reader
    /// stranded below the next reference).
    /// </summary>
    public event EventHandler<ReferenceCardEventArgs>? CardCollapsedAfterRead;

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

            foreach (var reference in subTopic.References)
            {
                var id = reference.Id(key);
                var context = reference.Context
                    .Select(c => new ContextLineViewModel(c.Vs, c.Text, c.Target))
                    .ToList();

                this.References.Add(new ReferenceCardViewModel(
                    id,
                    reference.RefLabel,
                    reference.TargetText,
                    reference.ShowGloss ? reference.Note : null,
                    context,
                    readIds.Contains(id),
                    noteIds.Contains(id),
                    this.SetReadAsync,
                    this.OpenNoteAsync,
                    this.OnCardCollapsedAfterRead));
            }
        }
        finally
        {
            this.IsLoading = false;
        }
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
