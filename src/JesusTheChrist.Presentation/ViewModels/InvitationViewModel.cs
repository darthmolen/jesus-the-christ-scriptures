using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JesusTheChrist.Core.Content;
using JesusTheChrist.Core.Models;
using JesusTheChrist.Data;
using JesusTheChrist.Presentation.Data;
using JesusTheChrist.Presentation.Navigation;

namespace JesusTheChrist.Presentation.ViewModels;

/// <summary>
/// Backs the first-run invitation screen. Loads the bundled invitation markdown for
/// the current language (falling back to English) and records when it has been seen.
/// </summary>
public partial class InvitationViewModel : ObservableObject
{
    private readonly IAssetSource assets;
    private readonly SettingsStore settings;
    private readonly IDatabaseInitializer databaseInitializer;
    private readonly INavigationService navigation;
    private readonly AppEnvironment environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvitationViewModel"/> class.
    /// </summary>
    /// <param name="assets">The bundled asset source.</param>
    /// <param name="settings">The settings store.</param>
    /// <param name="databaseInitializer">Ensures the database schema before writes.</param>
    /// <param name="navigation">The navigation service.</param>
    /// <param name="environment">App scope and default language.</param>
    public InvitationViewModel(
        IAssetSource assets,
        SettingsStore settings,
        IDatabaseInitializer databaseInitializer,
        INavigationService navigation,
        AppEnvironment environment)
    {
        this.assets = assets;
        this.settings = settings;
        this.databaseInitializer = databaseInitializer;
        this.navigation = navigation;
        this.environment = environment;
    }

    /// <summary>
    /// Gets or sets the invitation body as markdown source.
    /// </summary>
    [ObservableProperty]
    public partial string BodyMarkdown { get; set; } = string.Empty;

    /// <summary>
    /// Loads the invitation markdown for the current language, falling back to English.
    /// </summary>
    /// <returns>A task that completes when the markdown has been loaded.</returns>
    public async Task LoadAsync()
    {
        var language = await this.ResolveLanguageAsync();
        this.BodyMarkdown = await this.ReadInvitationAsync(language);
    }

    private async Task<string> ReadInvitationAsync(Language language)
    {
        try
        {
            return await this.ReadAssetAsync($"invitation.{language.Code()}.md");
        }
        catch (FileNotFoundException)
        {
            return await this.ReadAssetAsync($"invitation.{Language.En.Code()}.md");
        }
    }

    private async Task<string> ReadAssetAsync(string name)
    {
        await using var stream = await this.assets.OpenAsync(name);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private async Task<Language> ResolveLanguageAsync()
    {
        var saved = await this.settings.GetAsync(SettingKeys.Language);
        return string.IsNullOrWhiteSpace(saved)
            ? this.environment.DefaultLanguage
            : LanguageResolver.Resolve(saved);
    }

    [RelayCommand]
    private async Task AcknowledgeAsync()
    {
        await this.databaseInitializer.EnsureInitializedAsync();
        await this.settings.SetBoolAsync(SettingKeys.InvitationSeen, true);
        await this.navigation.GoBackAsync();
    }
}
