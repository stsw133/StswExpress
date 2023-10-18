using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace StswExpress;

/// <summary>
/// Represents a custom border control that applies clipping to its child element with rounded corners.
/// </summary>
/// <remarks>
/// As a side effect <see cref="StswClippingBorder"/> will surpress any databinding or animation of 
/// its childs <see cref="UIElement.Clip"/> property until the child is removed from <see cref="StswClippingBorder"/>.
/// </remarks>
public class StswClippingBorder : Border
{
    static StswClippingBorder()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswClippingBorder), new FrameworkPropertyMetadata(typeof(StswClippingBorder)));
    }

    #region Events & methods
    private readonly RectangleGeometry _clipRect = new();
    private object? _oldClip;

    /// <summary>
    /// Called when the control is rendered.
    /// </summary>
    protected override void OnRender(DrawingContext dc)
    {
        if (DoClipping)
            OnApplyChildClip();
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
                if (Child != null)
                    Child.SetValue(ClipProperty, _oldClip);

                if (value != null)
                    _oldClip = value.ReadLocalValue(ClipProperty);
                else
                    _oldClip = null;

                base.Child = value;
            }
        }
    }

    /// <summary>
    /// Applies the clipping with rounded corners to the child element of the border control.
    /// </summary>
    protected virtual void OnApplyChildClip()
    {
        var child = Child;
        if (child != null)
        {
            _clipRect.RadiusX = _clipRect.RadiusY = Math.Max(0.0, CornerRadius.TopLeft - BorderThickness.Left * 0.5);
            _clipRect.Rect = new Rect(Child.RenderSize);
            child.Clip = _clipRect;
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// 
    /// </summary>
    public bool DoClipping
    {
        get => (bool)GetValue(DoClippingProperty);
        set => SetValue(DoClippingProperty, value);
    }
    public static readonly DependencyProperty DoClippingProperty
        = DependencyProperty.Register(
            nameof(DoClipping),
            typeof(bool),
            typeof(StswClippingBorder)
        );
    #endregion
}
