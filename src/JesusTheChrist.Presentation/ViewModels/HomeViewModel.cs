using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JesusTheChrist.Core.Content;
using JesusTheChrist.Core.Models;
using JesusTheChrist.Data;
using JesusTheChrist.Presentation.Data;
using JesusTheChrist.Presentation.Navigation;

namespace JesusTheChrist.Presentation.ViewModels;

/// <summary>
/// Backs the Home / Challenge screen: the sub-topic list with per-topic progress
/// and the overall "X / Y references read" header.
/// </summary>
public partial class HomeViewModel : ObservableObject
{
    private readonly ContentService content;
    private readonly ReadMarkStore readMarks;
    private readonly SettingsStore settings;
    private readonly IDatabaseInitializer databaseInitializer;
    private readonly INavigationService navigation;
    private readonly AppEnvironment environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeViewModel"/> class.
    /// </summary>
    /// <param name="content">The content service that loads the Topical Guide.</param>
    /// <param name="readMarks">The read-mark store.</param>
    /// <param name="settings">The settings store (language preference).</param>
    /// <param name="databaseInitializer">Ensures the database schema before reads.</param>
    /// <param name="navigation">The navigation service.</param>
    /// <param name="environment">App scope and default language.</param>
    public HomeViewModel(
        ContentService content,
        ReadMarkStore readMarks,
        SettingsStore settings,
        IDatabaseInitializer databaseInitializer,
        INavigationService navigation,
        AppEnvironment environment)
    {
        this.content = content;
        this.readMarks = readMarks;
        this.settings = settings;
        this.databaseInitializer = databaseInitializer;
        this.navigation = navigation;
        this.environment = environment;
    }

    /// <summary>
    /// Gets or sets the overall challenge header text.
    /// </summary>
    [ObservableProperty]
    public partial string HeaderText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether content is being loaded.
    /// </summary>
    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    /// <summary>
    /// Gets the sub-topic rows shown on the Home screen.
    /// </summary>
    public ObservableCollection<TopicRowViewModel> Topics { get; } = new();

    [RelayCommand]
    private async Task LoadAsync()
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
            var readIds = await this.readMarks.GetReadIdsAsync();

            var overall = ProgressService.Overall(guide, readIds);
            var perTopic = ProgressService.PerSubTopic(guide, readIds);

            this.Topics.Clear();
            foreach (var subTopic in guide.SubTopics)
            {
                var progress = perTopic[subTopic.Key];
                this.Topics.Add(new TopicRowViewModel(
                    subTopic.Key,
                    subTopic.Title,
                    subTopic.ShortTitle,
                    progress.Read,
                    progress.Total));
            }

            this.HeaderText = $"{overall.Read} / {overall.Total} references read";
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task OpenTopicAsync(TopicRowViewModel? row)
    {
        if (row is null)
        {
            return;
        }

        await this.navigation.GoToAsync(
            NavigationRoutes.Topic,
            new Dictionary<string, object> { [NavigationRoutes.TopicKeyParameter] = row.Key });
    }

    private async Task<Language> ResolveLanguageAsync()
    {
        var saved = await this.settings.GetAsync(SettingKeys.Language);
        return string.IsNullOrWhiteSpace(saved)
            ? this.environment.DefaultLanguage
            : LanguageResolver.Resolve(saved);
    }
}
