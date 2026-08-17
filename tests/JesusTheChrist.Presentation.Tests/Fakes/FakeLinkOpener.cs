using JesusTheChrist.Presentation.Platform;

namespace JesusTheChrist.Presentation.Tests.Fakes;

/// <summary>
/// Records opened links for assertion in tests.
/// </summary>
public sealed class FakeLinkOpener : ILinkOpener
{
    /// <summary>
    /// Gets the URLs the view models asked to open, in order.
    /// </summary>
    public List<Uri> Opened { get; } = new();

    /// <summary>
    /// Gets the most recently opened link in string form, or null when nothing has been opened.
    /// </summary>
    public string? LastOpened => this.Opened.Count == 0 ? null : this.Opened[^1].ToString();

    /// <inheritdoc/>
    public Task OpenAsync(Uri url)
    {
        this.Opened.Add(url);
        return Task.CompletedTask;
    }
}
