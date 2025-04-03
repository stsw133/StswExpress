using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;/// <summary>
/// A hierarchical tree view control for displaying structured data with expandable/collapsible nodes.
/// Supports selection binding, corner radius customization, and read-only mode.
/// </summary>
/// <remarks>
/// When <see cref="ItemsSource"/> contains items of type <see cref="IStswSelectionItem"/>, selection is automatically bound.
/// </remarks>
public class StswTreeView : TreeView, IStswCornerControl, IStswSelectionControl
{
    static StswTreeView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTreeView), new FrameworkPropertyMetadata(typeof(StswTreeView)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswTreeView), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswTreeViewItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswTreeViewItem;

    #region Events & methods
    /// <summary>
    /// Called when the <see cref="ItemsSource"/> property changes.
    /// Updates selection binding and handles any necessary item expansion.
    /// </summary>
    /// <param name="oldValue">The previous <see cref="ItemsSource"/> collection.</param>
    /// <param name="newValue">The new <see cref="ItemsSource"/> collection.</param>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        IStswSelectionControl.ItemsSourceChanged(this, newValue);
        base.OnItemsSourceChanged(oldValue, newValue);

        //var selectedItem = FindAllTreeItems(this).FirstOrDefault(x => x.IsSelected);
        //if (selectedItem != null)
        //    while (StswFn.FindVisualAncestor<StswTreeViewItem>(selectedItem) is StswTreeViewItem item)
        //    {
        //        item.IsExpanded = true;
        //        selectedItem = item;
        //    }
    }

    /// <summary>
    /// Called when the <see cref="ItemTemplate"/> property changes.
    /// Updates the selection control logic based on the new item template.
    /// </summary>
    /// <param name="oldItemTemplate">The previous data template for items.</param>
    /// <param name="newItemTemplate">The new data template for items.</param>
    protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
    {
        IStswSelectionControl.ItemTemplateChanged(this, newItemTemplate);
        base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
    }

    /// <summary>
    /// Occurs when the PreviewKeyDown event is triggered.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (!IStswSelectionControl.PreviewKeyDown(this, e)) return;
        base.OnPreviewKeyDown(e);
    }

    /// <summary>
    /// Handles changes in the selected item.
    /// If the control is in read-only mode, selection changes are prevented.
    /// Otherwise, selection binding is updated.
    /// </summary>
    /// <param name="e">Event data containing the old and new selected items.</param>
    protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
    {
        base.OnSelectedItemChanged(e);
        IStswSelectionControl.SelectionChanged(this, new List<object>() { e.NewValue }, new List<object>() { e.OldValue });
    }

    /// <summary>
    /// Prepares the specified element to display the given item.
    /// Ensures that the item container inherits the <see cref="IsReadOnly"/> property binding.
    /// </summary>
    /// <param name="element">The element used to display the specified item.</param>
    /// <param name="item">The data item to be displayed.</param>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is StswTreeViewItem listBoxItem)
        {
            listBoxItem.SetBinding(StswTreeViewItem.IsReadOnlyProperty, new Binding(nameof(IsReadOnly))
            {
                Source = this,
                Mode = BindingMode.OneWay
            });
        }
    }
    #endregion

    #region Logic properties
    /// <inheritdoc/>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(StswTreeView)
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
            typeof(StswTreeView),
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
            typeof(StswTreeView),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswTreeView ItemsSource="{Binding Categories}" IsReadOnly="True">
    <se:StswTreeViewItem Header="Category 1"/>
    <se:StswTreeViewItem Header="Category 2"/>
</se:StswTreeView>

*/
