namespace JesusTheChrist.Presentation.Platform;

/// <summary>
/// Writes text to the system clipboard. The presentation layer has no MAUI reference — which is what
/// keeps its view models unit-testable on a desktop host — so the platform clipboard reaches them
/// through this seam, in the same shape as the navigation and appearance seams.
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// Places the given text on the system clipboard, replacing its contents.
    /// </summary>
    /// <param name="text">The text to place on the clipboard.</param>
    /// <returns>A task that completes when the clipboard has been written.</returns>
    public Task SetTextAsync(string text);
}
