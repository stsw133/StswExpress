using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace StswExpress;
/// <summary>
/// Represents a custom border control that applies clipping to its child element with rounded corners.
/// </summary>
/// <remarks>
/// As a side effect <see cref="StswBorder"/> will surpress any databinding or animation of 
/// its childs <see cref="UIElement.Clip"/> property until the child is removed from <see cref="StswBorder"/>.
/// </remarks>
public class StswBorder : Border, IStswCornerControl
{
    static StswBorder()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswBorder), new FrameworkPropertyMetadata(typeof(StswBorder)));
    }

    #region Events & methods
    private readonly RectangleGeometry _clipRect = new();
    private object? _oldClip;

    /// <summary>
    /// Called when the control is rendered.
    /// </summary>
    protected override void OnRender(DrawingContext dc)
    {
        if (CornerClipping && Child is UIElement child)
            OnApplyChildClip(child);
        base.OnRender(dc);
    }

    /// <summary>
    /// Gets or sets the child element of the border control and applies clipping with rounded corners.
    /// </summary>
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
    /// Applies the clipping with rounded corners to the child element of the border control.
    /// </summary>
    protected virtual void OnApplyChildClip(UIElement child)
    {
        _clipRect.RadiusX = _clipRect.RadiusY = Math.Max(0.0, CornerRadius.TopLeft - BorderThickness.Left * 0.5);
        _clipRect.Rect = new Rect(0, 0, child.RenderSize.Width, child.RenderSize.Height);
        child.Clip = _clipRect;
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
    /// </summary>
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
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
