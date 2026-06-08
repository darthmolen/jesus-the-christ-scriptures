using System.Globalization;
using JesusTheChrist.App.Services;
using JesusTheChrist.App.Views;
using JesusTheChrist.Core.Content;
using JesusTheChrist.Core.Models;
using JesusTheChrist.Data;
using JesusTheChrist.Presentation;
using JesusTheChrist.Presentation.Data;
using JesusTheChrist.Presentation.Navigation;
using JesusTheChrist.Presentation.ViewModels;
using Microsoft.Extensions.Logging;

namespace JesusTheChrist.App;

/// <summary>
/// Configures and builds the MAUI application.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates and configures the <see cref="MauiApp"/>.
    /// </summary>
    /// <returns>The configured <see cref="MauiApp"/> instance.</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        RegisterServices(builder.Services);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        // Content (read-only, bundled). Shared state -> singletons.
        services.AddSingleton<IAssetSource, MauiAssetSource>();
        services.AddSingleton<ContentService>();

        // On-device data. The SQLite connection is shared, so singletons.
        services.AddSingleton(_ => new AppDatabase(Path.Combine(FileSystem.AppDataDirectory, "app.db")));
        services.AddSingleton<DatabaseInitializer>();
        services.AddSingleton<IDatabaseInitializer>(sp => sp.GetRequiredService<DatabaseInitializer>());
        services.AddSingleton<ReadMarkStore>();
        services.AddSingleton<NoteStore>();
        services.AddSingleton<SettingsStore>();
        services.AddSingleton<StreakStore>();

        // App context: full-scope flavor; default language from the device locale.
        services.AddSingleton(new AppEnvironment(
            Scope.Full,
            LanguageResolver.Resolve(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)));

        // Navigation seam (Shell-backed).
        services.AddSingleton<INavigationService, ShellNavigationService>();

        // Shell + pages + view models. Pages/VMs are fresh per navigation -> transient.
        services.AddSingleton<AppShell>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<HomePage>();
        services.AddTransient<TopicFeedViewModel>();
        services.AddTransient<TopicFeedPage>();
    }
}
