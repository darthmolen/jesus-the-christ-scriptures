namespace JesusTheChrist.Presentation.Drawing;

/// <summary>
/// Pure geometry helpers for rendering a circular progress ring.
/// </summary>
public static class RingMath
{
    /// <summary>
    /// The angle, in degrees, at which a progress arc begins: the top of the circle (12 o'clock).
    /// </summary>
    public const float StartAngleDegrees = -90f;

    /// <summary>
    /// Converts a completion fraction into the sweep angle of the progress arc.
    /// </summary>
    /// <param name="fraction">The completion fraction; clamped to the range [0, 1].</param>
    /// <returns>The sweep angle in degrees, in the range [0, 360].</returns>
    public static float SweepDegrees(double fraction) => (float)(Math.Clamp(fraction, 0.0, 1.0) * 360.0);
}
