using JesusTheChrist.App.Services;
using JesusTheChrist.Presentation;
using JesusTheChrist.Presentation.Globalization;
using JesusTheChrist.Presentation.ViewModels;

namespace JesusTheChrist.App;

/// <summary>
/// The MAUI application root.
/// </summary>
public partial class App : Application
{
    private readonly IServiceProvider services;

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="services">The application service provider.</param>
    public App(IServiceProvider services)
    {
        this.services = services;
        this.InitializeComponent();
    }

    /// <inheritdoc/>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Container is fully built here (safe to resolve — see DI XAML-parse-timing gotcha).
        // Kick off idempotent schema creation; data screens await the same task before reading.
        _ = this.services.GetRequiredService<DatabaseInitializer>().EnsureInitializedAsync();

        // Apply the saved UI language synchronously, before the shell (and its first page) is
        // built, so a non-default language isn't briefly shown in English on cold start. The
        // settings store is async-only, so the language is mirrored to a synchronous preference.
        var languageCode = this.services.GetRequiredService<ILanguagePreference>().GetCode();
        if (!string.IsNullOrWhiteSpace(languageCode))
        {
            AppCulture.Apply(LanguageResolver.Resolve(languageCode));
        }

        // Apply the persisted theme + reading font size at launch (also re-mirrors the language).
        _ = this.services.GetRequiredService<SettingsViewModel>().LoadAsync();

        var shell = this.services.GetRequiredService<AppShell>();
        return new Window(shell);
    }
}
