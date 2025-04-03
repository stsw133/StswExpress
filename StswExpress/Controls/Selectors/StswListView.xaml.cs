using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;/// <summary>
/// A customizable list view control for displaying a collection of selectable items with optional details.
/// Supports selection binding, corner radius customization, and read-only mode.
/// </summary>
/// <remarks>
/// When <see cref="ItemsSource"/> contains items of type <see cref="IStswSelectionItem"/>, selection is automatically bound.
/// </remarks>
public class StswListView : ListView, IStswCornerControl, IStswSelectionControl
{
    static StswListView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswListView), new FrameworkPropertyMetadata(typeof(StswListView)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswListView), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswListViewItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswListViewItem;

    #region Events & methods
    /// <summary>
    /// Called when the <see cref="ItemsSource"/> property changes.
    /// Updates the selection state and propagates changes to the selection control logic.
    /// </summary>
    /// <param name="oldValue">The previous <see cref="ItemsSource"/> collection.</param>
    /// <param name="newValue">The new <see cref="ItemsSource"/> collection.</param>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        IStswSelectionControl.ItemsSourceChanged(this, newValue);
        base.OnItemsSourceChanged(oldValue, newValue);
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
    /// Handles selection changes within the list view.
    /// If the control is in read-only mode, the selection change is prevented.
    /// Otherwise, the selection state is updated accordingly.
    /// </summary>
    /// <param name="e">Provides data for the <see cref="SelectionChanged"/> event.</param>
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        IStswSelectionControl.SelectionChanged(this, e.AddedItems, e.RemovedItems);
    }

    /// <summary>
    /// Prepares the specified element to display the given item.
    /// Ensures that the item container (list item) inherits the <see cref="IsReadOnly"/> property binding.
    /// </summary>
    /// <param name="element">The element used to display the specified item.</param>
    /// <param name="item">The data item to be displayed.</param>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is StswListViewItem listBoxItem)
        {
            listBoxItem.SetBinding(StswListViewItem.IsReadOnlyProperty, new Binding(nameof(IsReadOnly))
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
            typeof(StswListView)
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
            typeof(StswListView),
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
            typeof(StswListView),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswListView ItemsSource="{Binding Logs}" IsReadOnly="True">
    <se:StswListViewItem Content="Log Entry 1"/>
    <se:StswListViewItem Content="Log Entry 2"/>
</se:StswListView>

*/
