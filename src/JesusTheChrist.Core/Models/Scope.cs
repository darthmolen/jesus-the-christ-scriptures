namespace JesusTheChrist.Core.Models;

/// <summary>
/// The set of standard works a build flavor exposes.
/// </summary>
public enum Scope
{
    /// <summary>
    /// Only references whose verses are in the Old or New Testament (the Bible).
    /// </summary>
    BibleOnly,

    /// <summary>
    /// References across all standard works.
    /// </summary>
    Full,
}
