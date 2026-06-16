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
    private readonly SettingsStore settings;
    private readonly IDatabaseInitializer databaseInitializer;
    private readonly INavigationService navigation;
    private readonly AppEnvironment environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="TopicFeedViewModel"/> class.
    /// </summary>
    /// <param name="content">The content service that loads the Topical Guide.</param>
    /// <param name="readMarks">The read-mark store.</param>
    /// <param name="notes">The note store.</param>
    /// <param name="settings">The settings store (language preference).</param>
    /// <param name="databaseInitializer">Ensures the database schema before reads.</param>
    /// <param name="navigation">The navigation service.</param>
    /// <param name="environment">App scope and default language.</param>
    public TopicFeedViewModel(
        ContentService content,
        ReadMarkStore readMarks,
        NoteStore notes,
        SettingsStore settings,
        IDatabaseInitializer databaseInitializer,
        INavigationService navigation,
        AppEnvironment environment)
    {
        this.content = content;
        this.readMarks = readMarks;
        this.notes = notes;
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

    private void OnCardCollapsedAfterRead(ReferenceCardViewModel card) =>
        this.CardCollapsedAfterRead?.Invoke(this, new ReferenceCardEventArgs(card));

    private Task OpenNoteAsync(ReferenceCardViewModel card) =>
        this.navigation.GoToAsync(
            NavigationRoutes.Note,
            new Dictionary<string, object>
            {
                [NavigationRoutes.NoteRefIdParameter] = card.Id,
                [NavigationRoutes.NoteRefLabelParameter] = card.RefLabel,
                [NavigationRoutes.NoteVerseTextParameter] = card.VerseText,
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
