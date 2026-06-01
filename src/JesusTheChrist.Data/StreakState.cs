namespace JesusTheChrist.Data;

/// <summary>
/// Daily reading streak state.
/// </summary>
/// <param name="Current">The current consecutive-day streak length.</param>
/// <param name="Best">The best streak length achieved.</param>
/// <param name="LastReadDate">The last date a reference was read, or <see langword="null"/>.</param>
public readonly record struct StreakState(int Current, int Best, DateOnly? LastReadDate);
