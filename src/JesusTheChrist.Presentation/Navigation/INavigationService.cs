namespace JesusTheChrist.Presentation.Navigation;

/// <summary>
/// Abstracts page navigation so view models stay free of MAUI Shell types.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigates to the given route, optionally passing query parameters.
    /// </summary>
    /// <param name="route">The destination route.</param>
    /// <param name="parameters">Optional navigation parameters.</param>
    /// <returns>A task that completes when navigation has been requested.</returns>
    public Task GoToAsync(string route, IDictionary<string, object>? parameters = null);

    /// <summary>
    /// Navigates back to the previous page.
    /// </summary>
    /// <returns>A task that completes when navigation has been requested.</returns>
    public Task GoBackAsync();
}
