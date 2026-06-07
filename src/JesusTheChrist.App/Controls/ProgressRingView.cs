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

    private static void OnFractionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (ProgressRingView)bindable;
        view.drawable.Fraction = (double)newValue;
        view.Invalidate();
    }
}
