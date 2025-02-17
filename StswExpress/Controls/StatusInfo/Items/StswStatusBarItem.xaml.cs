﻿using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class StswStatusBarItem : StatusBarItem
{
    static StswStatusBarItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswStatusBarItem), new FrameworkPropertyMetadata(typeof(StswStatusBarItem)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswStatusBarItem), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

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
            typeof(StswStatusBarItem),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswStatusBarItem),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
