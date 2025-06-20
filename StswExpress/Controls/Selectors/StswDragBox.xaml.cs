using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// A <see cref="ListBox"/> that supports drag-and-drop reordering of items within the same list
/// and transferring items between different lists. Also supports selection binding and read-only mode.
/// </summary>
/// <remarks>
/// When <see cref="ItemsSource"/> contains items of type <see cref="IStswSelectionItem"/>, selection is automatically bound.
/// </remarks>
public class StswDragBox : ListBox, IStswCornerControl, IStswSelectionControl
{
    private object? _dragDropItem;
    private IList? _sourceList;

    static StswDragBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDragBox), new FrameworkPropertyMetadata(typeof(StswDragBox)));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswDragBoxItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswDragBoxItem;

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        AllowDrop = true;
        DragOver += OnDragOver;
        Drop += OnDrop;

        ItemContainerStyle = new Style(typeof(StswDragBoxItem));
        ItemContainerStyle.Setters.Add(new EventSetter(MouseMoveEvent, new MouseEventHandler(OnItemMouseMove)));
        ItemContainerStyle.Setters.Add(new EventSetter(DragOverEvent, new DragEventHandler(OnItemDragOver)));
    }

    /// <summary>
    /// Called when the <see cref="ItemsSource"/> property changes.
    /// Ensures that the new <see cref="ItemsSource"/> is an <see cref="ObservableCollection{T}"/> and 
    /// updates selection binding accordingly.
    /// </summary>
    /// <param name="oldValue">The previous <see cref="ItemsSource"/> collection.</param>
    /// <param name="newValue">The new <see cref="ItemsSource"/> collection.</param>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        var type = newValue?.GetType();
        if (type != null && !(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ObservableCollection<>)))
            throw new InvalidOperationException($"{nameof(StswDragBox)} ItemsSource must be of {nameof(ObservableCollection<object>)} type!");

        IStswSelectionControl.ItemsSourceChanged(this, newValue);
        base.OnItemsSourceChanged(oldValue, newValue);
    }

    /// <summary>
    /// Called when the <see cref="ItemTemplate"/> property changes.
    /// Updates selection control logic based on the new item template.
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
    /// Handles selection changes within the drag box.
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
    /// Ensures that the item container inherits the <see cref="IsReadOnly"/> property binding.
    /// </summary>
    /// <param name="element">The element used to display the specified item.</param>
    /// <param name="item">The data item to be displayed.</param>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is StswDragBoxItem listBoxItem)
        {
            listBoxItem.SetBinding(StswDragBoxItem.IsReadOnlyProperty, new Binding(nameof(IsReadOnly))
            {
                Source = this,
                Mode = BindingMode.OneWay
            });
        }
    }
    #endregion

    #region Drag & Drop logic
    /// <summary>
    /// Handles the drag-over event for individual list items, allowing item reordering within the same list.
    /// </summary>
    /// <param name="sender">The item being dragged over.</param>
    /// <param name="e">Drag event arguments.</param>
    private void OnItemDragOver(object sender, DragEventArgs e)
    {
        if (sender is FrameworkElement targetElement && ItemsSource is IList currentList)
        {
            var draggedItem = e.Data.GetData("StswDraggedItem");
            var sourceList = e.Data.GetData("StswSourceList") as IList;
            var targetItem = targetElement.DataContext;

            if (draggedItem != null && targetItem != null && ReferenceEquals(sourceList, currentList))
                SwapInList(currentList, draggedItem, targetItem);
        }
    }

    /// <summary>
    /// Handles the drag-over event on the empty space of the list.
    /// Ensures the correct cursor appearance and move effect during the operation.
    /// </summary>
    /// <param name="sender">The drag box receiving the event.</param>
    /// <param name="e">Drag event arguments.</param>
    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    /// <summary>
    /// Handles the drop event, moving items between lists.
    /// Removes the dragged item from the source list and inserts it into the target list at the correct position.
    /// </summary>
    /// <param name="sender">The drop target list box.</param>
    /// <param name="e">Drag event arguments.</param>
    private void OnDrop(object sender, DragEventArgs e)
    {
        if (IsReadOnly)
        {
            e.Handled = true;
            return;
        }

        if (ItemsSource is not IList targetList)
            return;

        var draggedItem = e.Data.GetData("StswDraggedItem");
        if (draggedItem == null || e.Data.GetData("StswSourceList") is not IList sourceList)
            return;

        if (!ReferenceEquals(sourceList, targetList))
        {
            if (sourceList.Contains(draggedItem))
                sourceList.Remove(draggedItem);

            var insertIndex = GetIndexFromPoint(e.GetPosition(this));
            if (insertIndex < 0 || insertIndex > targetList.Count)
                insertIndex = targetList.Count;

            if (!targetList.Contains(draggedItem))
                targetList.Insert(insertIndex, draggedItem);
        }
    }

    /// <summary>
    /// Detects when an item is being dragged and starts the drag-and-drop operation.
    /// </summary>
    /// <param name="sender">The item being dragged.</param>
    /// <param name="e">Mouse event arguments.</param>
    private void OnItemMouseMove(object sender, MouseEventArgs e)
    {
        if (IsReadOnly)
        {
            e.Handled = true;
            return;
        }

        if (e.LeftButton == MouseButtonState.Pressed && sender is FrameworkElement element)
        {
            _dragDropItem = element.DataContext;
            _sourceList = ItemsSource as IList;

            if (_dragDropItem == null || _sourceList == null)
                return;

            var data = new DataObject();
            data.SetData("StswDraggedItem", _dragDropItem);
            data.SetData("StswSourceList", _sourceList);

            DragDrop.DoDragDrop(element, data, DragDropEffects.Move);

            _dragDropItem = null;
            _sourceList = null;
        }
    }

    /// <summary>
    /// Determines the index at which to insert a dropped item based on the cursor position.
    /// Returns the appropriate index or the end of the list if the mouse is outside any item.
    /// </summary>
    /// <param name="point">The point where the drop occurs.</param>
    /// <returns>The index for inserting the dropped item.</returns>
    private int GetIndexFromPoint(Point point)
    {
        var hitResult = VisualTreeHelper.HitTest(this, point);
        if (hitResult == null)
            return Items.Count;

        DependencyObject obj = hitResult.VisualHit;
        while (obj != null)
        {
            if (obj is ListBoxItem item)
            {
                var index = ItemContainerGenerator.IndexFromContainer(item);
                if (index >= 0)
                    return index;
            }
            obj = VisualTreeHelper.GetParent(obj);
        }

        return Items.Count;
    }

    /// <summary>
    /// Swaps the positions of two objects in a list, used for reordering items within the same list.
    /// </summary>
    /// <param name="list">The list containing the items.</param>
    /// <param name="obj1">The first item to swap.</param>
    /// <param name="obj2">The second item to swap.</param>
    private void SwapInList(IList list, object obj1, object obj2)
    {
        var index1 = list.IndexOf(obj1);
        var index2 = list.IndexOf(obj2);

        if (index1 == -1 || index2 == -1 || index1 == index2)
            return;

        (list[index2], list[index1]) = (list[index1], list[index2]);
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
            typeof(StswDragBox)
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
            typeof(StswDragBox),
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
            typeof(StswDragBox),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswDragBox ItemsSource="{Binding Employees}" IsReadOnly="True">
    <se:StswDragBoxItem Content="John Doe"/>
    <se:StswDragBoxItem Content="Jane Smith"/>
</se:StswDragBox>

*/
