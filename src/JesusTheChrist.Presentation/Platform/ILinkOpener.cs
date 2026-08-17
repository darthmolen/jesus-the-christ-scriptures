namespace JesusTheChrist.Presentation.Platform;

/// <summary>
/// Opens an external link in whatever the platform considers its handler. The presentation layer has
/// no MAUI reference — which is what keeps its view models unit-testable on a desktop host — so the
/// platform launcher reaches them through this seam, in the same shape as the clipboard, navigation,
/// and appearance seams.
/// </summary>
public interface ILinkOpener
{
    /// <summary>
    /// Opens the given absolute URL. Implementations never throw: an absent handler or a launcher
    /// failure is a silent no-op, because a reader who taps a study link should never be shown an
    /// error for it.
    /// </summary>
    /// <param name="url">The absolute URL to open.</param>
    /// <returns>A task that completes once the platform has been asked to open the link.</returns>
    public Task OpenAsync(Uri url);
}
