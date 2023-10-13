using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a control that displays a collection of items in a hierarchical list.
/// ItemsSource with items of <see cref="IStswSelectionItem"/> type automatically binds selected items.
/// </summary>
public class StswTreeView : TreeView
{
    static StswTreeView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTreeView), new FrameworkPropertyMetadata(typeof(StswTreeView)));
    }

    #region Events & methods
    /// <summary>
    /// 
    /// </summary>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        base.OnItemsSourceChanged(oldValue, newValue);
        UsesSelectionItems = ItemsSource?.OfType<IStswSelectionItem>() != null;
    }
    #endregion

    #region Main properties
    /// <summary>
    /// 
    /// </summary>
    internal bool UsesSelectionItems
    {
        get => (bool)GetValue(UsesSelectionItemsProperty);
        set => SetValue(UsesSelectionItemsProperty, value);
    }
    public static readonly DependencyProperty UsesSelectionItemsProperty
        = DependencyProperty.Register(
            nameof(UsesSelectionItems),
            typeof(bool),
            typeof(StswTreeView)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the degree to which the corners of the control are rounded.
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
            typeof(StswTreeView)
        );
    #endregion
}
