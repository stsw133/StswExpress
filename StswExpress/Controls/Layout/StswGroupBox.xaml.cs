﻿using System.Windows;
using System.Windows.Controls;

namespace StswExpress;
/// <summary>
/// A container control that groups related UI elements under a common header.
/// </summary>
public class StswGroupBox : GroupBox, IStswCornerControl
{
    static StswGroupBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswGroupBox), new FrameworkPropertyMetadata(typeof(StswGroupBox)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswGroupBox), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

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
            typeof(StswGroupBox),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswGroupBox),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between the header and the content.
    /// </summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }
    public static readonly DependencyProperty SeparatorThicknessProperty
        = DependencyProperty.Register(
            nameof(SeparatorThickness),
            typeof(double),
            typeof(StswGroupBox),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswGroupBox Header="Settings">
    <CheckBox Content="Enable dark mode"/>
</se:StswGroupBox>

*/
