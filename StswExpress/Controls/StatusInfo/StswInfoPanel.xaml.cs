using System;
using System.Collections;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// Represents a panel control that displays information bars in a scrollable list, providing optional functionality for a close button.
/// </summary>
public class StswInfoPanel : ItemsControl, IStswCornerControl
{
    static StswInfoPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswInfoPanel), new FrameworkPropertyMetadata(typeof(StswInfoPanel)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswInfoPanel), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Button: copy all to clipboard
        if (GetTemplateChild("PART_ButtonCopyAllToClipboard") is ButtonBase btnCopyAllToClipboard)
            btnCopyAllToClipboard.Click += PART_ButtonCopyAllToClipboard_Click;
        /// Button: close all
        if (GetTemplateChild("PART_ButtonCloseAll") is ButtonBase btnCloseAll)
            btnCloseAll.Click += PART_ButtonCloseAll_Click;
    }

    /// <summary>
    /// 
    /// </summary>
    private void PART_ButtonCopyAllToClipboard_Click(object sender, RoutedEventArgs e)
    {
        var sb = new StringBuilder();
        foreach (var infoBar in StswFn.FindVisualChildren<StswInfoBar>(this))
            sb.AppendLine($"{infoBar.Title}{Environment.NewLine}{infoBar.Text}");

        Clipboard.SetText(sb.ToString());
    }

    /// <summary>
    /// Handles the click event of the function button, used for closing the info bar if it's placed within an StswInfoPanel.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_ButtonCloseAll_Click(object sender, RoutedEventArgs e)
    {
        if (ItemsSource is IList list)
            list.Clear();
        else
            Items?.Clear();
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the items are closable and have a close button.
    /// </summary>
    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }
    public static readonly DependencyProperty IsClosableProperty
        = DependencyProperty.Register(
            nameof(IsClosable),
            typeof(bool),
            typeof(StswInfoPanel)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the items are copyable and have a copy button.
    /// </summary>
    public bool IsCopyable
    {
        get => (bool)GetValue(IsCopyableProperty);
        set => SetValue(IsCopyableProperty, value);
    }
    public static readonly DependencyProperty IsCopyableProperty
        = DependencyProperty.Register(
            nameof(IsCopyable),
            typeof(bool),
            typeof(StswInfoPanel)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the items are expandable and have an expand button.
    /// </summary>
    public bool IsExpandable
    {
        get => (bool)GetValue(IsExpandableProperty);
        set => SetValue(IsExpandableProperty, value);
    }
    public static readonly DependencyProperty IsExpandableProperty
        = DependencyProperty.Register(
            nameof(IsExpandable),
            typeof(bool),
            typeof(StswInfoPanel)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the items are currently expanded.
    /// </summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }
    public static readonly DependencyProperty IsExpandedProperty
        = DependencyProperty.Register(
            nameof(IsExpanded),
            typeof(bool),
            typeof(StswInfoPanel)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the control panel is visible or not.
    /// </summary>
    public bool ShowControlPanel
    {
        get => (bool)GetValue(ShowControlPanelProperty);
        set => SetValue(ShowControlPanelProperty, value);
    }
    public static readonly DependencyProperty ShowControlPanelProperty
        = DependencyProperty.Register(
            nameof(ShowControlPanel),
            typeof(bool),
            typeof(StswInfoPanel)
        );
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
            typeof(StswInfoPanel),
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
            typeof(StswInfoPanel),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between panel and subs.
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
            typeof(StswInfoPanel),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
