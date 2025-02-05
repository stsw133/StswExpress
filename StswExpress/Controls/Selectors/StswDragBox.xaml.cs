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
/// Custom ListBox that supports reordering items within the same list
/// and moving items between different lists by Drag and Drop.
/// </summary>
public class StswDragBox : ListBox, IStswCornerControl, IStswSelectionControl
{
    static StswDragBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDragBox), new FrameworkPropertyMetadata(typeof(StswDragBox)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswDragBox), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswDragBoxItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswDragBoxItem;

    #region Events & methods
    private object? _dragDropItem;
    private IList? _sourceList;

    /// <summary>
    /// 
    /// </summary>
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
    /// Occurs when the ItemsSource property value changes.
    /// </summary>
    /// <param name="oldValue">The old value of the ItemsSource property.</param>
    /// <param name="newValue">The new value of the ItemsSource property.</param>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        var type = newValue?.GetType();
        if (type != null && !(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ObservableCollection<>)))
            throw new InvalidOperationException($"{nameof(StswDragBox)} ItemsSource must be of {nameof(ObservableCollection<object>)} type!");

        IStswSelectionControl.ItemsSourceChanged(this, newValue);
        base.OnItemsSourceChanged(oldValue, newValue);
    }

    /// <summary>
    /// Occurs when the ItemTemplate property value changes.
    /// </summary>
    /// <param name="oldItemTemplate">The old value of the ItemTemplate property.</param>
    /// <param name="newItemTemplate">The new value of the ItemTemplate property.</param>
    protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
    {
        IStswSelectionControl.ItemTemplateChanged(this, newItemTemplate);
        base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        if (IsReadOnly)
        {
            e.Handled = true;
            return;
        }

        base.OnSelectionChanged(e);
        IStswSelectionControl.SelectionChanged(this, e.AddedItems, e.RemovedItems);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="element"></param>
    /// <param name="item"></param>
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

    #region Drag and Drop logic
    /// <summary>
    /// Reordering / swapping in the same list is triggered when we DragOver a specific item container.
    /// </summary>
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
    /// If we are dragging over the empty part of the ListBox (not on an item), or just to show a correct cursor.
    /// </summary>
    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    /// <summary>
    /// Final step: if it's a drop from a different list, remove from source and add to this list.
    /// </summary>
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
        var sourceList = e.Data.GetData("StswSourceList") as IList;

        if (draggedItem == null || sourceList == null)
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
    /// Starts the DragDrop operation when user moves the mouse with left button pressed on a ListBoxItem.
    /// </summary>
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
    /// Returns the index at which to insert an item, based on a Point (mouse position).
    /// If the mouse is outside the items, returns the Count (end of the list).
    /// If the mouse is over a specific item, returns that item's index in the list.
    /// </summary>
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
    /// Simple function to swap positions of two objects in an IList (used for reorder in same list).
    /// </summary>
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
    /// <summary>
    /// Gets or sets a value indicating whether control is in read-only mode.
    /// When set to <see langword="true"/>, the scroll with items is accessible, but all items within the scroll are unclickable.
    /// </summary>
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
            typeof(StswDragBox),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
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
            typeof(StswDragBox),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
