using JesusTheChrist.Presentation.Platform;

namespace JesusTheChrist.Presentation.Tests.Fakes;

/// <summary>
/// Records clipboard writes for assertion in tests.
/// </summary>
public sealed class FakeClipboard : IClipboardService
{
    /// <summary>
    /// Gets the recorded clipboard writes in order.
    /// </summary>
    public List<string> Writes { get; } = new();

    /// <summary>
    /// Gets the most recent clipboard write, or null when nothing has been written.
    /// </summary>
    public string? LastText => this.Writes.Count == 0 ? null : this.Writes[^1];

    /// <inheritdoc/>
    public Task SetTextAsync(string text)
    {
        this.Writes.Add(text);
        return Task.CompletedTask;
    }
}
