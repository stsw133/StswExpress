﻿using System.Windows;
using System.Windows.Controls;

namespace StswExpress;
/// <summary>
/// A collapsible panel control that displays a header and allows the user to expand or collapse its content.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswExpander Header="Details" IsExpanded="True"&gt;
///     &lt;TextBlock Text="Here are additional details..."/&gt;
/// &lt;/se:StswExpander&gt;
/// </code>
/// </example>
[StswInfo("0.1.0", Changes = StswPlannedChanges.VisualChanges)]
public class StswExpander : Expander, IStswCornerControl
{
    static StswExpander()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswExpander), new FrameworkPropertyMetadata(typeof(StswExpander)));
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
            typeof(StswExpander),
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
            typeof(StswExpander),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between the header and the content of the expander.
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
            typeof(StswExpander),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
