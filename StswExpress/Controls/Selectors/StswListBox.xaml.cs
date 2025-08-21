using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace StswExpress;
/// <summary>
/// A list box control for displaying a collection of selectable items in a vertical list.
/// Supports selection binding, corner radius customization, and read-only mode.
/// </summary>
/// <remarks>
/// ItemsSource with items of <see cref="IStswSelectionItem"/> type automatically bind selected items.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswListBox ItemsSource="{Binding Products}" IsReadOnly="True"&gt;
///     &lt;se:StswListBoxItem Content="Product A"/&gt;
///     &lt;se:StswListBoxItem Content="Product B"/&gt;
/// &lt;/se:StswListBox&gt;
/// </code>
/// </example>
[StswInfo("0.1.0")]
public class StswListBox : ListBox, IStswCornerControl, IStswSelectionControl
{
    static StswListBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswListBox), new FrameworkPropertyMetadata(typeof(StswListBox)));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswListBoxItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswListBoxItem;

    #region Events & methods
    /// <inheritdoc/>
    [StswInfo("0.20.0")]
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (ScrollToItemBehavior == StswScrollToItemBehavior.OnSelection && SelectedItem != null)
            Dispatcher.InvokeAsync(() => ScrollIntoView(SelectedItem), DispatcherPriority.Loaded);
    }

    /// <inheritdoc/>
    [StswInfo("0.20.0")]
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);

        if (ScrollToItemBehavior == StswScrollToItemBehavior.OnInsert && e.Action == NotifyCollectionChangedAction.Add && e.NewItems?.Count > 0)
            Dispatcher.InvokeAsync(() => ScrollIntoView(e.NewItems[^1]), DispatcherPriority.Background);
    }

    /// <inheritdoc/>
    [StswInfo("0.10.0")]
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        IStswSelectionControl.ItemsSourceChanged(this, newValue);
        base.OnItemsSourceChanged(oldValue, newValue);
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
    [StswInfo("0.10.0", "0.20.0")]
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        IStswSelectionControl.SelectionChanged(this, e.AddedItems, e.RemovedItems);

        if (ScrollToItemBehavior == StswScrollToItemBehavior.OnSelection && SelectedItem != null)
            Dispatcher.InvokeAsync(() => ScrollIntoView(SelectedItem), DispatcherPriority.Background);
    }

    /// <inheritdoc/>
    [StswInfo("0.14.0")]
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is StswListBoxItem listBoxItem)
        {
            listBoxItem.SetBinding(StswListBoxItem.IsReadOnlyProperty, new Binding(nameof(IsReadOnly))
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
            typeof(StswListBox)
        );

    /// <summary>
    /// Gets or sets the behavior for scrolling to an item when it is selected or inserted.
    /// </summary>
    [StswInfo("0.20.0")]
    public StswScrollToItemBehavior ScrollToItemBehavior
    {
        get => (StswScrollToItemBehavior)GetValue(ScrollToItemBehaviorProperty);
        set => SetValue(ScrollToItemBehaviorProperty, value);
    }
    public static readonly DependencyProperty ScrollToItemBehaviorProperty
        = DependencyProperty.Register(
            nameof(ScrollToItemBehavior),
            typeof(StswScrollToItemBehavior),
            typeof(StswListBox)
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
            typeof(StswListBox),
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
            typeof(StswListBox),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
