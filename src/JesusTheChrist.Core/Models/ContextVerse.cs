namespace JesusTheChrist.Core.Models;

/// <summary>
/// A single verse within a reference's context window.
/// </summary>
/// <param name="Vs">The verse number.</param>
/// <param name="Text">The verse text.</param>
/// <param name="Target">A value indicating whether this verse is one the reference targets.</param>
/// <param name="Ch">
/// The chapter this verse belongs to, or <see langword="null"/> when it shares the reference's
/// chapter. Only cross-chapter references populate this per verse.
/// </param>
public record ContextVerse(int Vs, string Text, bool Target, int? Ch = null);
