using JesusTheChrist.App.Drawing;

namespace JesusTheChrist.App.Controls;

/// <summary>
/// A bindable progress ring backed by a <see cref="ProgressRingDrawable"/>,
/// suitable for use inside a CollectionView item template.
/// </summary>
public sealed class ProgressRingView : GraphicsView
{
    /// <summary>
    /// Bindable backing store for the <see cref="Fraction"/> property.
    /// </summary>
    public static readonly BindableProperty FractionProperty = BindableProperty.Create(
        nameof(Fraction),
        typeof(double),
        typeof(ProgressRingView),
        0.0,
        propertyChanged: OnFractionChanged);

    /// <summary>
    /// Bindable backing store for the <see cref="TrackColor"/> property.
    /// </summary>
    public static readonly BindableProperty TrackColorProperty = BindableProperty.Create(
        nameof(TrackColor),
        typeof(Color),
        typeof(ProgressRingView),
        ProgressRingDrawable.DefaultTrackColor,
        propertyChanged: (b, _, n) => Apply(b, n, static (d, c) => d.TrackColor = c));

    /// <summary>
    /// Bindable backing store for the <see cref="ProgressColor"/> property.
    /// </summary>
    public static readonly BindableProperty ProgressColorProperty = BindableProperty.Create(
        nameof(ProgressColor),
        typeof(Color),
        typeof(ProgressRingView),
        ProgressRingDrawable.DefaultProgressColor,
        propertyChanged: (b, _, n) => Apply(b, n, static (d, c) => d.ProgressColor = c));

    /// <summary>
    /// Bindable backing store for the <see cref="CompleteColor"/> property.
    /// </summary>
    public static readonly BindableProperty CompleteColorProperty = BindableProperty.Create(
        nameof(CompleteColor),
        typeof(Color),
        typeof(ProgressRingView),
        ProgressRingDrawable.DefaultCompleteColor,
        propertyChanged: (b, _, n) => Apply(b, n, static (d, c) => d.CompleteColor = c));

    private readonly ProgressRingDrawable drawable = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressRingView"/> class.
    /// </summary>
    public ProgressRingView()
    {
        this.Drawable = this.drawable;
    }

    /// <summary>
    /// Gets or sets the completion fraction in the range [0, 1].
    /// </summary>
    public double Fraction
    {
        get => (double)this.GetValue(FractionProperty);
        set => this.SetValue(FractionProperty, value);
    }

    /// <summary>
    /// Gets or sets the colour of the unfilled track.
    /// </summary>
    public Color TrackColor
    {
        get => (Color)this.GetValue(TrackColorProperty);
        set => this.SetValue(TrackColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the colour of the filled progress arc.
    /// </summary>
    public Color ProgressColor
    {
        get => (Color)this.GetValue(ProgressColorProperty);
        set => this.SetValue(ProgressColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the colour of the ring once every reference has been read.
    /// </summary>
    public Color CompleteColor
    {
        get => (Color)this.GetValue(CompleteColorProperty);
        set => this.SetValue(CompleteColorProperty, value);
    }

    private static void OnFractionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (ProgressRingView)bindable;
        view.drawable.Fraction = (double)newValue;
        view.Invalidate();
    }

    private static void Apply(BindableObject bindable, object newValue, Action<ProgressRingDrawable, Color> set)
    {
        // Each property defaults to the drawable's own colour, so this only fires when something
        // explicitly binds null; keep the current colour rather than painting with a null stroke.
        if (newValue is not Color color)
        {
            return;
        }

        var view = (ProgressRingView)bindable;
        set(view.drawable, color);
        view.Invalidate();
    }
}
