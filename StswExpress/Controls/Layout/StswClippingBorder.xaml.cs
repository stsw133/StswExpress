﻿using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace StswExpress;

/// <Remarks>
/// As a side effect <see cref="StswClippingBorder"/> will surpress any databinding or animation of 
/// its childs <see cref="UIElement.Clip"/> property until the child is removed from <see cref="StswClippingBorder"/>.
/// </Remarks>
public class StswClippingBorder : Border
{
    static StswClippingBorder()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswClippingBorder), new FrameworkPropertyMetadata(typeof(StswClippingBorder)));
    }

    #region Events
    private readonly RectangleGeometry _clipRect = new();
    private object? _oldClip;

    /// OnRender
    protected override void OnRender(DrawingContext dc)
    {
        OnApplyChildClip();
        base.OnRender(dc);
    }

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
}