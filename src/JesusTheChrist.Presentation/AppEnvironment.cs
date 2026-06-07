using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Presentation;

/// <summary>
/// Carries app-wide context resolved at startup: the content scope (build flavor)
/// and the default content language (from the device locale).
/// </summary>
public sealed class AppEnvironment
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppEnvironment"/> class.
    /// </summary>
    /// <param name="scope">The content scope for this build flavor.</param>
    /// <param name="defaultLanguage">The default content language.</param>
    public AppEnvironment(Scope scope, Language defaultLanguage)
    {
        this.Scope = scope;
        this.DefaultLanguage = defaultLanguage;
    }

    /// <summary>
    /// Gets the content scope (Bible-only or full) for this build flavor.
    /// </summary>
    public Scope Scope { get; }

    /// <summary>
    /// Gets the default content language used when the user has no saved preference.
    /// </summary>
    public Language DefaultLanguage { get; }
}
