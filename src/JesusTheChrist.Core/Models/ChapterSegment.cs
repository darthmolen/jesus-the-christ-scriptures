namespace JesusTheChrist.Core.Models;

/// <summary>
/// A contiguous run of a reference's target verses that share one chapter. A single-chapter
/// reference yields exactly one segment; a cross-chapter reference yields one per chapter it spans.
/// </summary>
/// <param name="Ch">The chapter number for this run of verses.</param>
/// <param name="ChapterLabel">The display label for the chapter, for example <c>Matthew 10</c>.</param>
/// <param name="Verses">The target verses in this chapter, in ascending order.</param>
public record ChapterSegment(int Ch, string ChapterLabel, IReadOnlyList<ContextVerse> Verses);
