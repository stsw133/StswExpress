using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;
/// <summary>
/// A collapsible side panel that expands on mouse hover and hides when the cursor leaves.
/// Supports always-visible mode.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswSidePanel IsAlwaysVisible="True"&gt;
///     &lt;StackPanel&gt;
///         &lt;Button Content="Option 1"/&gt;
///         &lt;Button Content="Option 2"/&gt;
///     &lt;/StackPanel&gt;
/// &lt;/se:StswSidePanel&gt;
/// </code>
/// </example>
public class StswSidePanel : ContentControl
{
    static StswSidePanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSidePanel), new FrameworkPropertyMetadata(typeof(StswSidePanel)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_ExpandBorder") is Border expandBorder)
            expandBorder.MouseEnter += (_, _) =>
            {
                if (!IsAlwaysVisible && IsCollapsed)
                    IsCollapsed = false;
            };
    }

    /// <inheritdoc/>
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (!IsAlwaysVisible && !IsCollapsed)
            IsCollapsed = true;
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the side panel is always visible.
    /// If true, the panel remains expanded and doesn't collapse, overriding mouse hover behavior.
    /// </summary>
    public bool IsAlwaysVisible
    {
        get => (bool)GetValue(IsAlwaysVisibleProperty);
        set => SetValue(IsAlwaysVisibleProperty, value);
    }
    public static readonly DependencyProperty IsAlwaysVisibleProperty
        = DependencyProperty.Register(
            nameof(IsAlwaysVisible),
            typeof(bool),
            typeof(StswSidePanel),
            new PropertyMetadata(default(bool), OnIsAlwaysVisibleChanged)
        );
    public static void OnIsAlwaysVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswSidePanel stsw)
            return;

        var newIsCollapsed = !stsw.IsAlwaysVisible;
        if (stsw.IsCollapsed != newIsCollapsed)
            stsw.IsCollapsed = newIsCollapsed;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the control is collapsed.
    /// When collapsed, the side panel is hidden until the mouse hovers over it, unless it is in always-visible mode.
    /// </summary>
    internal bool IsCollapsed
    {
        get => (bool)GetValue(IsCollapsedProperty);
        set => SetValue(IsCollapsedProperty, value);
    }
    internal static readonly DependencyProperty IsCollapsedProperty
        = DependencyProperty.Register(
            nameof(IsCollapsed),
            typeof(bool),
            typeof(StswSidePanel),
            new PropertyMetadata(true)
        );
    #endregion
}
