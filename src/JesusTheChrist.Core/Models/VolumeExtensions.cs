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

    /// <summary>
    /// Gets the volume code used in a churchofjesuschrist.org study URL.
    /// </summary>
    /// <param name="volume">The volume.</param>
    /// <returns>The site volume code, for example <c>nt</c> or <c>dc-testament</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The volume is not a known volume.</exception>
    public static string SiteCode(this Volume volume) => volume switch
    {
        Volume.OldTestament => "ot",
        Volume.NewTestament => "nt",
        Volume.BookOfMormon => "bofm",
        Volume.DoctrineAndCovenants => "dc-testament",
        Volume.PearlOfGreatPrice => "pgp",
        _ => throw new ArgumentOutOfRangeException(nameof(volume)),
    };
}
