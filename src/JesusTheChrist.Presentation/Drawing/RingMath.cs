namespace JesusTheChrist.Presentation.Drawing;

/// <summary>
/// Pure geometry helpers for rendering a circular progress ring.
/// </summary>
public static class RingMath
{
    /// <summary>
    /// Converts a completion fraction into the sweep angle of the progress arc.
    /// </summary>
    /// <param name="fraction">The completion fraction; clamped to the range [0, 1].</param>
    /// <returns>The sweep angle in degrees, in the range [0, 360].</returns>
    public static float SweepDegrees(double fraction) => (float)(Math.Clamp(fraction, 0.0, 1.0) * 360.0);

    /// <summary>
    /// Gets a value indicating whether the fraction fills the ring completely, in which case it
    /// must be drawn as a closed circle: an arc whose sweep is a full 360 degrees has identical
    /// start and end angles and paints nothing.
    /// </summary>
    /// <param name="fraction">The completion fraction; clamped to the range [0, 1].</param>
    /// <returns><see langword="true"/> when the ring is full; otherwise <see langword="false"/>.</returns>
    public static bool IsComplete(double fraction) => SweepDegrees(fraction) >= 360f;
}
