using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;
/// <summary>
/// A collapsible side panel that expands on mouse hover and hides when the cursor leaves.
/// Supports always-visible mode.
/// </summary>
[StswInfo("0.2.0")]
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
    [StswInfo("0.7.0")]
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
    public static void OnIsAlwaysVisibleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswSidePanel stsw)
            return;

        var newIsCollapsed = !stsw.IsAlwaysVisible;
        if (stsw.IsCollapsed != newIsCollapsed)
            stsw.IsCollapsed = newIsCollapsed;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the control is collapsed.
    /// When collapsed, the side panel is hidden until the mouse hovers over it, unless it is in always-visible mode.
    /// </summary>
    [StswInfo("0.7.0")]
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

/* usage:

<se:StswSidePanel IsAlwaysVisible="True">
    <StackPanel>
        <Button Content="Option 1"/>
        <Button Content="Option 2"/>
    </StackPanel>
</se:StswSidePanel>

*/
