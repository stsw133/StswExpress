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
public class StswBorder : Border, IStswCornerControl
{
    private readonly RectangleGeometry _clipRect = new();
    private object? _oldClip;

    static StswBorder()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswBorder), new FrameworkPropertyMetadata(typeof(StswBorder)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswBorder), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    /// <summary>
    /// Called when the control is rendered. If <see cref="CornerClipping"/> is enabled and a child element is present, 
    /// applies rounded clipping to the child.
    /// </summary>
    /// <param name="dc">The drawing context for rendering the control.</param>
    protected override void OnRender(DrawingContext dc)
    {
        if (CornerClipping && Child is UIElement child)
            OnApplyChildClip(child);

        base.OnRender(dc);
    }

    /// <summary>
    /// Overrides the <see cref="Border.Child"/> property to apply rounded clipping and restore the previous Clip value 
    /// when changing the child.
    /// </summary>
    /// <remarks>
    /// Stores the original Clip value of the child and restores it when the child is removed.
    /// </remarks>
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
