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
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswTreeView ItemsSource="{Binding Categories}" IsReadOnly="True"&gt;
///     &lt;se:StswTreeViewItem Header="Category 1"/&gt;
///     &lt;se:StswTreeViewItem Header="Category 2"/&gt;
/// &lt;/se:StswTreeView&gt;
/// </code>
/// </example>
[StswInfo("0.3.0")]
public class StswTreeView : TreeView, IStswCornerControl, IStswSelectionControl
{
    static StswTreeView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTreeView), new FrameworkPropertyMetadata(typeof(StswTreeView)));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswTreeViewItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswTreeViewItem;

    #region Events & methods
    /// <inheritdoc/>
    [StswInfo("0.10.0")]
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

    /// <inheritdoc/>
    [StswInfo("0.10.0")]
    protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
    {
        IStswSelectionControl.ItemTemplateChanged(this, newItemTemplate);
        base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
    }

    /// <inheritdoc/>
    [StswInfo("0.17.0")]
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (!IStswSelectionControl.PreviewKeyDown(this, e)) return;
        base.OnPreviewKeyDown(e);
    }

    /// <inheritdoc/>
    [StswInfo("0.10.0")]
    protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
    {
        base.OnSelectedItemChanged(e);
        IStswSelectionControl.SelectionChanged(this, new List<object>() { e.NewValue }, new List<object>() { e.OldValue });
    }

    /// <inheritdoc/>
    [StswInfo("0.14.0")]
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is StswTreeViewItem treeViewItem)
        {
            treeViewItem.SetBinding(StswTreeViewItem.IsReadOnlyProperty, new Binding(nameof(IsReadOnly))
            {
                Source = this,
                Mode = BindingMode.OneWay
            });
        }
    }
    #endregion

    #region Logic properties
    /// <inheritdoc/>
    [StswInfo("0.15.0")]
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
