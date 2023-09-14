using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a custom border control that enable user to zoom and move content.
/// </summary>
public class StswSidePanel : UserControl
{
    static StswSidePanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSidePanel), new FrameworkPropertyMetadata(typeof(StswSidePanel)));
    }

    #region Events & methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        IsExpanded = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        IsExpanded = false;
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the degree to which the corners of the control are rounded.
    /// </summary>
    public StswHidingVariant HidingVariant
    {
        get => (StswHidingVariant)GetValue(HidingVariantProperty);
        set => SetValue(HidingVariantProperty, value);
    }
    public static readonly DependencyProperty HidingVariantProperty
        = DependencyProperty.Register(
            nameof(HidingVariant),
            typeof(StswHidingVariant),
            typeof(StswSidePanel)
        );

    /// <summary>
    /// Gets or sets the degree to which the corners of the control are rounded.
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
            typeof(StswSidePanel)
        );
    #endregion
}
