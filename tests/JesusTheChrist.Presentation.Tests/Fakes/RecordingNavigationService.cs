using JesusTheChrist.Presentation.Navigation;

namespace JesusTheChrist.Presentation.Tests.Fakes;

/// <summary>
/// Records navigation calls for assertion in tests.
/// </summary>
public sealed class RecordingNavigationService : INavigationService
{
    /// <summary>
    /// Gets the recorded navigations in order.
    /// </summary>
    public List<(string Route, IDictionary<string, object>? Parameters)> Calls { get; } = new();

    /// <inheritdoc/>
    public Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        this.Calls.Add((route, parameters));
        return Task.CompletedTask;
    }
}
