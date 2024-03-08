using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a custom border control that enable user to zoom and move content.
/// </summary>
public class StswSidePanel : ContentControl
{
    static StswSidePanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSidePanel), new FrameworkPropertyMetadata(typeof(StswSidePanel)));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_ExpandBorder") is Border expandBorder)
            expandBorder.MouseEnter += (s, e) =>
            {
                if (!IsAlwaysVisible && IsCollapsed)
                    IsCollapsed = false;
            };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (!IsAlwaysVisible && !IsCollapsed)
            IsCollapsed = true;
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets whether the control is always visible.
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
    public static void OnIsAlwaysVisibleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswSidePanel stsw)
        {
            stsw.IsCollapsed = !stsw.IsAlwaysVisible;
        }
    }

    /// <summary>
    /// Gets or sets whether the control is collapsed.
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
