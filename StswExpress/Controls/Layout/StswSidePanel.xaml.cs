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
                if (ExpandMode == StswToolbarMode.Collapsed)
                    ExpandMode = StswToolbarMode.Compact;
            };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (ExpandMode == StswToolbarMode.Compact)
            ExpandMode = StswToolbarMode.Collapsed;
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the degree to which the corners of the control are rounded.
    /// </summary>
    public StswToolbarMode ExpandMode
    {
        get => (StswToolbarMode)GetValue(ExpandModeProperty);
        set => SetValue(ExpandModeProperty, value);
    }
    public static readonly DependencyProperty ExpandModeProperty
        = DependencyProperty.Register(
            nameof(ExpandMode),
            typeof(StswToolbarMode),
            typeof(StswSidePanel)
        );
    #endregion
}
