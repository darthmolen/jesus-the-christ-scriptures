using JesusTheChrist.Presentation.Platform;

namespace JesusTheChrist.App.Services;

/// <summary>
/// Opens external links through the MAUI <see cref="Launcher"/>, which the presentation layer
/// cannot reference directly. On Android a churchofjesuschrist.org link is claimed by the Gospel
/// Library app when it is installed, and falls back to the browser when it is not.
/// </summary>
public sealed class MauiLinkOpener : ILinkOpener
{
    /// <inheritdoc/>
    /// <remarks>
    /// Failure is deliberately silent. A reader who taps a study link and has nothing able to open
    /// it is better served by nothing happening than by an error they cannot act on, so both a
    /// <see langword="false"/> result and a thrown launcher are swallowed here.
    /// </remarks>
    public async Task OpenAsync(Uri url)
    {
        try
        {
            await Launcher.Default.OpenAsync(url);
        }
        catch (Exception)
        {
            // Nothing on the device could open the link. See the remarks above.
        }
    }
}
