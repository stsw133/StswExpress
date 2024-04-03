using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a control that displays a collection of items in a hierarchical list.
/// ItemsSource with items of <see cref="IStswSelectionItem"/> type automatically bind selected items.
/// </summary>
public class StswTreeView : TreeView, IStswCornerControl, IStswScrollableControl
{
    static StswTreeView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTreeView), new FrameworkPropertyMetadata(typeof(StswTreeView)));
    }

    #region Events & methods
    /// <summary>
    /// Gets a <see cref="StswScrollViewer"/> of the control.
    /// </summary>
    public StswScrollViewer GetScrollViewer() => (StswScrollViewer)GetTemplateChild("PART_ScrollViewer");

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        GetScrollViewer()?.InitAttachedProperties(this);
    }

    /// <summary>
    /// Occurs when the ItemsSource property value changes.
    /// </summary>
    /// <param name="oldValue">The old value of the ItemsSource property.</param>
    /// <param name="newValue">The new value of the ItemsSource property.</param>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        if (newValue?.GetType()?.IsListType(out var innerType) == true)
        {
            UsesSelectionItems = innerType?.IsAssignableTo(typeof(IStswSelectionItem)) == true;
            if (innerType?.IsAssignableTo(typeof(StswComboItem)) == true)
            {
                if (string.IsNullOrEmpty(DisplayMemberPath))
                    DisplayMemberPath = nameof(StswComboItem.Display);
                if (string.IsNullOrEmpty(SelectedValuePath))
                    SelectedValuePath = nameof(StswComboItem.Value);
            }
        }
        base.OnItemsSourceChanged(oldValue, newValue);

        //var selectedItem = FindAllTreeItems(this).FirstOrDefault(x => x.IsSelected);
        //if (selectedItem != null)
        //    while (StswFn.FindVisualAncestor<TreeViewItem>(selectedItem) is TreeViewItem item)
        //    {
        //        item.IsExpanded = true;
        //        selectedItem = item;
        //    }
    }

    /// <summary>
    /// Occurs when the ItemTemplate property value changes.
    /// </summary>
    /// <param name="oldItemTemplate">The old value of the ItemTemplate property.</param>
    /// <param name="newItemTemplate">The new value of the ItemTemplate property.</param>
    protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
    {
        if (newItemTemplate != null && !string.IsNullOrEmpty(DisplayMemberPath))
            DisplayMemberPath = string.Empty;
        base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the control uses selection items that implement
    /// the <see cref="IStswSelectionItem"/> interface to enable advanced selection features.
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
            typeof(StswTreeView)
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
            typeof(StswTreeView)
        );
    #endregion
}
