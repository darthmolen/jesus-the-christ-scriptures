using JesusTheChrist.Presentation.Drawing;

namespace JesusTheChrist.App.Drawing;

/// <summary>
/// Draws a circular progress ring: a full track circle with a foreground arc
/// whose sweep is driven by <see cref="RingMath"/>.
/// </summary>
public sealed class ProgressRingDrawable : IDrawable
{
    // Microsoft.Maui.Graphics measures angles in degrees with 0 at 3 o'clock and
    // positive angles going counter-clockwise; 90 is the top of the circle.
    private const float TopAngle = 90f;

    /// <summary>
    /// The default colour of the unfilled track.
    /// </summary>
    public static readonly Color DefaultTrackColor = Color.FromArgb("#E0E0E0");

    /// <summary>
    /// The default colour of the filled progress arc.
    /// </summary>
    public static readonly Color DefaultProgressColor = Color.FromArgb("#512BD4");

    /// <summary>
    /// The default colour of a fully-read ring.
    /// </summary>
    public static readonly Color DefaultCompleteColor = Color.FromArgb("#B8860B");

    /// <summary>
    /// Gets or sets the completion fraction in the range [0, 1].
    /// </summary>
    public double Fraction { get; set; }

    /// <summary>
    /// Gets or sets the stroke thickness of the ring.
    /// </summary>
    public float StrokeWidth { get; set; } = 6f;

    /// <summary>
    /// Gets or sets the colour of the unfilled track.
    /// </summary>
    public Color TrackColor { get; set; } = DefaultTrackColor;

    /// <summary>
    /// Gets or sets the colour of the filled progress arc.
    /// </summary>
    public Color ProgressColor { get; set; } = DefaultProgressColor;

    /// <summary>
    /// Gets or sets the colour of the ring once every reference has been read.
    /// </summary>
    public Color CompleteColor { get; set; } = DefaultCompleteColor;

    /// <inheritdoc/>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        var inset = this.StrokeWidth / 2f;
        var bounds = new RectF(
            dirtyRect.X + inset,
            dirtyRect.Y + inset,
            dirtyRect.Width - this.StrokeWidth,
            dirtyRect.Height - this.StrokeWidth);

        canvas.StrokeSize = this.StrokeWidth;
        canvas.StrokeLineCap = LineCap.Round;

        // A full ring is a closed circle, not an arc: DrawArc takes start and end angles, so a
        // 360-degree sweep ends exactly where it starts and paints nothing at all. Drawn in place
        // of the track rather than over it, so there is no wasted paint.
        if (RingMath.IsComplete(this.Fraction))
        {
            canvas.StrokeColor = this.CompleteColor;
            canvas.DrawEllipse(bounds);
            return;
        }

        canvas.StrokeColor = this.TrackColor;
        canvas.DrawEllipse(bounds);

        var sweep = RingMath.SweepDegrees(this.Fraction);
        if (sweep <= 0f)
        {
            return;
        }

        canvas.StrokeColor = this.ProgressColor;
        canvas.DrawArc(
            bounds.X,
            bounds.Y,
            bounds.Width,
            bounds.Height,
            TopAngle,
            TopAngle - sweep,
            clockwise: true,
            closed: false);
    }
}
