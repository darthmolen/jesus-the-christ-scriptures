using JesusTheChrist.Presentation.Navigation;

namespace JesusTheChrist.App.Services;

/// <summary>
/// Implements <see cref="INavigationService"/> over MAUI Shell navigation.
/// </summary>
public sealed class ShellNavigationService : INavigationService
{
    /// <inheritdoc/>
    public Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        if (parameters is null || parameters.Count == 0)
        {
            return Shell.Current.GoToAsync(route);
        }

        var query = new ShellNavigationQueryParameters();
        foreach (var pair in parameters)
        {
            query.Add(pair.Key, pair.Value);
        }

        return Shell.Current.GoToAsync(route, query);
    }

    /// <inheritdoc/>
    public Task GoBackAsync() => Shell.Current.GoToAsync("..");
}
