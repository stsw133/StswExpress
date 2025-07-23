using System;
using System.Collections;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// A container for displaying multiple information bars in a scrollable list.
/// Supports batch closing, copying messages, and expandable notifications.
/// </summary>
/// <remarks>
/// This control provides a structured way to display multiple notifications, allowing bulk operations like 
/// closing all notifications at once or copying their content to the clipboard.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswInfoPanel ShowControlPanel="True"&gt;
///     &lt;se:StswInfoBar Title="Success" Text="Operation completed!" Type="Success"/&gt;
///     &lt;se:StswInfoBar Title="Error" Text="Something went wrong." Type="Error"/&gt;
/// &lt;/se:StswInfoPanel&gt;
/// </code>
/// </example>
[StswInfo("0.1.0")]
public class StswInfoPanel : ItemsControl, IStswCornerControl
{
    static StswInfoPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswInfoPanel), new FrameworkPropertyMetadata(typeof(StswInfoPanel)));
    }

    #region Events & methods
    /// <inheritdoc/>
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
    /// Copies the content of all displayed information bars (titles and texts) to the clipboard.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    [StswInfo("0.9.0")]
    private void PART_ButtonCopyAllToClipboard_Click(object sender, RoutedEventArgs e)
    {
        var sb = new StringBuilder();
        foreach (var infoBar in StswFnUI.FindVisualChildren<StswInfoBar>(this))
            sb.AppendLine($"{infoBar.Title}{Environment.NewLine}{infoBar.Text}");

        Clipboard.SetText(sb.ToString());
    }

    /// <summary>
    /// Handles the close-all button click event.
    /// Clears all items from the panel, either from the <see cref="ItemsSource"/> collection or the internal <see cref="Items"/> list.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
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
    /// Gets or sets a value indicating whether each information bar can be closed individually.
    /// When enabled, each item will have a close button.
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
    /// Gets or sets a value indicating whether each information bar can be copied to the clipboard individually.
    /// When enabled, each item will have a copy button.
    /// </summary>
    [StswInfo("0.13.0")]
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
    /// Gets or sets a value indicating whether each information bar can be expanded for more details.
    /// When enabled, an expand button will be displayed for each item.
    /// </summary>
    [StswInfo("0.13.0")]
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
    /// Gets or sets a value indicating whether the information bars are currently expanded.
    /// </summary>
    [StswInfo("0.13.0")]
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
    /// Gets or sets a value indicating whether the control panel (containing batch operations) is visible.
    /// </summary>
    [StswInfo("0.9.0")]
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
            typeof(StswInfoPanel),
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
            typeof(StswInfoPanel),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between the control panel and the information bars.
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
