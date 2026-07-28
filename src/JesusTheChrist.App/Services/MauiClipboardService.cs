using JesusTheChrist.Presentation.Platform;

namespace JesusTheChrist.App.Services;

/// <summary>
/// Writes to the device clipboard through MAUI <see cref="Clipboard"/>, which the presentation
/// layer cannot reference directly.
/// </summary>
public sealed class MauiClipboardService : IClipboardService
{
    /// <inheritdoc/>
    public Task SetTextAsync(string text) => Clipboard.Default.SetTextAsync(text);
}
