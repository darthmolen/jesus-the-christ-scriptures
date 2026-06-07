using JesusTheChrist.App.Services;

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

        var shell = this.services.GetRequiredService<AppShell>();
        return new Window(shell);
    }
}
