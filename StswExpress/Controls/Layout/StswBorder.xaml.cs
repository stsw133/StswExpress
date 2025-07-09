using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// A custom border control that applies clipping to its child element with rounded corners.
/// </summary>
/// <remarks>
/// As a side effect <see cref="StswBorder"/> will surpress any databinding or animation of 
/// its childs <see cref="UIElement.Clip"/> property until the child is removed from <see cref="StswBorder"/>.
/// </remarks>
[Stsw(null)]
public class StswBorder : Border, IStswCornerControl
{
    private readonly RectangleGeometry _clipRect = new();
    private object? _oldClip;

    static StswBorder()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswBorder), new FrameworkPropertyMetadata(typeof(StswBorder)));
    }

    #region Events & methods
    /// <inheritdoc/>
    protected override void OnRender(DrawingContext dc)
    {
        if (CornerClipping && Child is UIElement child)
            OnApplyChildClip(child);

        base.OnRender(dc);
    }

    /// <inheritdoc/>
    public override UIElement Child
    {
        get => base.Child;
        set
        {
            if (Child != value)
            {
                Child?.SetValue(ClipProperty, _oldClip);
                _oldClip = value?.ReadLocalValue(ClipProperty);
                base.Child = value;
            }
        }
    }

    /// <summary>
    /// Applies a rounded clipping region to the child element based on <see cref="CornerRadius"/> and <see cref="BorderThickness"/>.
    /// </summary>
    /// <param name="child">The child element to apply clipping to.</param>
    protected virtual void OnApplyChildClip(UIElement child)
    {
        _clipRect.RadiusX = _clipRect.RadiusY = Math.Max(0.0, CornerRadius.TopLeft - BorderThickness.Left * 0.5);
        _clipRect.Rect = new Rect(0, 0, child.RenderSize.Width, child.RenderSize.Height);
        child.Clip = _clipRect;
    }
    #endregion

    #region Style properties
    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswBorder),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswBorder CornerClipping="True" CornerRadius="20">
    <Image Source="example.jpg"/>
</se:StswBorder>

*/
