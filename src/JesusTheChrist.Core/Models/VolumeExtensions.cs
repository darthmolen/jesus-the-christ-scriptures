namespace JesusTheChrist.Core.Models;

/// <summary>
/// Helpers for <see cref="Volume"/>.
/// </summary>
public static class VolumeExtensions
{
    /// <summary>
    /// Parses the <c>vol</c> identifier used in the bundled JSON into a <see cref="Volume"/>.
    /// </summary>
    /// <param name="raw">The volume identifier, for example <c>newtestament</c>.</param>
    /// <returns>The matching <see cref="Volume"/>.</returns>
    /// <exception cref="ArgumentException">The identifier is not a known volume.</exception>
    public static Volume Parse(string raw) => raw switch
    {
        "oldtestament" => Volume.OldTestament,
        "newtestament" => Volume.NewTestament,
        "bookofmormon" => Volume.BookOfMormon,
        "doctrineandcovenants" => Volume.DoctrineAndCovenants,
        "pearlofgreatprice" => Volume.PearlOfGreatPrice,
        _ => throw new ArgumentException($"Unknown volume '{raw}'.", nameof(raw)),
    };

    /// <summary>
    /// Gets a value indicating whether the volume is part of the Bible (Old or New Testament).
    /// </summary>
    /// <param name="volume">The volume.</param>
    /// <returns><see langword="true"/> if the volume is the Old or New Testament.</returns>
    public static bool IsBible(this Volume volume) =>
        volume is Volume.OldTestament or Volume.NewTestament;

    /// <summary>
    /// Gets the canonical sort index of the volume.
    /// </summary>
    /// <param name="volume">The volume.</param>
    /// <returns>The zero-based canonical order.</returns>
    public static int Order(this Volume volume) => (int)volume;
}
