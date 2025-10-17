using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;
/// <summary>
/// A tab control with extended functionality, including dynamic tab creation, 
/// visibility toggling, and command-based item management.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswTabControl AreTabsVisible="True" NewItemButtonVisibility="Visible"&gt;
///     &lt;se:StswTabItem Header="Home"/&gt;
///     &lt;se:StswTabItem Header="Settings"/&gt;
/// &lt;/se:StswTabControl&gt;
/// </code>
/// </example>
public class StswTabControl : TabControl
{
    public StswTabControl()
    {
        _previewMouseLeftButtonDownHandler = OnTabPreviewMouseLeftButtonDown;
        _mouseMoveHandler = OnTabMouseMove;
        _mouseLeftButtonUpHandler = OnTabMouseLeftButtonUp;
        _dropHandler = OnTabDrop;
        _dragOverHandler = OnTabDragOver;
    }

    static StswTabControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTabControl), new FrameworkPropertyMetadata(typeof(StswTabControl)));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswTabItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswTabItem;

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        NewItemCommand = new StswCommand(CreateItem);
        UpdateReorderHandlers(CanReorder);
        UpdateTabItemsAllowDrop();
    }

    /// <summary>
    /// Creates a new tab item and adds it to the tab control. 
    /// Supports both bound item sources and direct tab item collections.
    /// </summary>
    private void CreateItem()
    {
        if (ItemsSource is IList list)
        {
            var itemType = list.GetType().GenericTypeArguments.FirstOrDefault() ?? typeof(object);
            NewItem = Activator.CreateInstance(itemType);
            list.Add(NewItem);
            SelectedIndex = list.Count - 1;
        }
        else if (Items != null)
        {
            NewItem = new StswTabItem();
            Items.Add(NewItem);
            SelectedIndex = Items.Count - 1;
        }
    }

    /// <inheritdoc/>
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);

        if (CanReorder)
            UpdateTabItemsAllowDrop();
    }

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is TabItem tabItem)
            tabItem.AllowDrop = CanReorder;
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the tab headers are visible in the tab control.
    /// </summary>
    public bool AreTabsVisible
    {
        get => (bool)GetValue(AreTabsVisibleProperty);
        set => SetValue(AreTabsVisibleProperty, value);
    }
    public static readonly DependencyProperty AreTabsVisibleProperty
        = DependencyProperty.Register(
            nameof(AreTabsVisible),
            typeof(bool),
            typeof(StswTabControl)
        );

    /// <summary>
    /// Gets or sets a value indicating whether tab items can be reordered via drag and drop.
    /// </summary>
    public bool CanReorder
    {
        get => (bool)GetValue(CanReorderProperty);
        set => SetValue(CanReorderProperty, value);
    }
    public static readonly DependencyProperty CanReorderProperty
        = DependencyProperty.Register(
            nameof(CanReorder),
            typeof(bool),
            typeof(StswTabControl),
            new PropertyMetadata(false, OnCanReorderChanged)
        );

    /// <summary>
    /// Gets or sets the newly created tab item when a new tab is added.
    /// </summary>
    public object? NewItem
    {
        get => (object?)GetValue(NewItemProperty);
        set => SetValue(NewItemProperty, value);
    }
    public static readonly DependencyProperty NewItemProperty
        = DependencyProperty.Register(
            nameof(NewItem),
            typeof(object),
            typeof(StswTabControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

    /// <summary>
    /// Gets or sets the command responsible for creating a new tab item in the tab control.
    /// </summary>
    public ICommand? NewItemCommand
    {
        get => (ICommand?)GetValue(NewItemCommandProperty);
        set => SetValue(NewItemCommandProperty, value);
    }
    public static readonly DependencyProperty NewItemCommandProperty
        = DependencyProperty.Register(
            nameof(NewItemCommand),
            typeof(ICommand),
            typeof(StswTabControl)
        );

    /// <summary>
    /// Gets or sets the visibility of the button used for adding new tab items.
    /// </summary>
    public Visibility NewItemButtonVisibility
    {
        get => (Visibility)GetValue(NewItemButtonVisibilityProperty);
        set => SetValue(NewItemButtonVisibilityProperty, value);
    }
    public static readonly DependencyProperty NewItemButtonVisibilityProperty
        = DependencyProperty.Register(
            nameof(NewItemButtonVisibility),
            typeof(Visibility),
            typeof(StswTabControl)
        );
    #endregion

    #region Drag & drop logic
    private readonly MouseButtonEventHandler _previewMouseLeftButtonDownHandler;
    private readonly MouseEventHandler _mouseMoveHandler;
    private readonly MouseButtonEventHandler _mouseLeftButtonUpHandler;
    private readonly DragEventHandler _dropHandler;
    private readonly DragEventHandler _dragOverHandler;

    private Point _dragStartPoint;
    private StswTabItem? _draggedItem;
    private bool _isDragging;
    private bool _reorderHandlersAttached;

    /// <summary>
    /// Called when the CanReorder property changes to attach or detach drag-and-drop event handlers.
    /// </summary>
    /// <param name="d">The dependency object where the property changed.</param>
    /// <param name="e">The event data.</param>
    private static void OnCanReorderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StswTabControl control)
        {
            control.UpdateReorderHandlers((bool)e.NewValue);
            control.UpdateTabItemsAllowDrop();
        }
    }

    /// <summary>
    /// Attaches or detaches drag-and-drop event handlers based on the CanReorder property.
    /// </summary>
    /// <param name="enable">If set to <see langword="true"/>, attaches the event handlers; otherwise, detaches them.</param>
    private void UpdateReorderHandlers(bool enable)
    {
        if (enable && !_reorderHandlersAttached)
        {
            AddHandler(PreviewMouseLeftButtonDownEvent, _previewMouseLeftButtonDownHandler, true);
            AddHandler(MouseMoveEvent, _mouseMoveHandler, true);
            AddHandler(MouseLeftButtonUpEvent, _mouseLeftButtonUpHandler, true);
            AddHandler(DropEvent, _dropHandler, true);
            AddHandler(DragOverEvent, _dragOverHandler, true);
            _reorderHandlersAttached = true;
        }
        else if (!enable && _reorderHandlersAttached)
        {
            RemoveHandler(PreviewMouseLeftButtonDownEvent, _previewMouseLeftButtonDownHandler);
            RemoveHandler(MouseMoveEvent, _mouseMoveHandler);
            RemoveHandler(MouseLeftButtonUpEvent, _mouseLeftButtonUpHandler);
            RemoveHandler(DropEvent, _dropHandler);
            RemoveHandler(DragOverEvent, _dragOverHandler);
            _reorderHandlersAttached = false;
        }
    }

    /// <summary>
    /// Updates the AllowDrop property of each tab item based on the CanReorder property.
    /// </summary>
    private void UpdateTabItemsAllowDrop()
    {
        foreach (var item in Items)
            if (ItemContainerGenerator.ContainerFromItem(item) is StswTabItem tabItem)
                tabItem.AllowDrop = CanReorder;
    }

    /// <summary>
    /// Handles the PreviewMouseLeftButtonDown event to initiate drag-and-drop operations for tab reordering.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    private void OnTabPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!CanReorder)
            return;

        if (e.OriginalSource is not DependencyObject originalSource)
            return;

        _dragStartPoint = e.GetPosition(this);
        _draggedItem = StswFnUI.FindVisualAncestor<StswTabItem>(originalSource);
        _isDragging = false;
    }

    /// <summary>
    /// Handles the MouseMove event to perform drag-and-drop operations for tab reordering.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    private void OnTabMouseMove(object sender, MouseEventArgs e)
    {
        if (!CanReorder)
        {
            _draggedItem = null;
            return;
        }

        if (e.LeftButton != MouseButtonState.Pressed)
        {
            ResetDrag();
            return;
        }

        if (_draggedItem == null)
            return;

        var currentPosition = e.GetPosition(this);

        if (!_isDragging)
        {
            if (Math.Abs(currentPosition.X - _dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance
             || Math.Abs(currentPosition.Y - _dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                _isDragging = true;
                CaptureMouse();
            }
            else return;
        }

        if (InputHitTest(currentPosition) is not DependencyObject hitElement)
            return;

        var targetItem = StswFnUI.FindVisualAncestor<StswTabItem>(hitElement);
        if (targetItem == null || targetItem == _draggedItem)
            return;

        ReorderTabItems(_draggedItem, targetItem);
        e.Handled = true;
    }

    /// <summary>
    /// Handles the MouseLeftButtonUp event to finalize drag operations and release mouse capture.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    private void OnTabMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!CanReorder)
            return;

        if (_draggedItem != null || _isDragging)
            ResetDrag();
    }

    /// <summary>
    /// Handles the DragOver event to provide visual feedback during drag-and-drop operations for tab reordering.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    private void OnTabDragOver(object sender, DragEventArgs e)
    {
        if (CanReorder && e.Data.GetDataPresent(typeof(StswTabItem)))
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }
    }

    /// <summary>
    /// Handles the Drop event to finalize drag-and-drop operations for tab reordering.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    private void OnTabDrop(object sender, DragEventArgs e)
    {
        if (!CanReorder)
            return;

        var sourceTabItem = e.Data.GetDataPresent(typeof(StswTabItem))
            ? e.Data.GetData(typeof(StswTabItem)) as StswTabItem
            : _draggedItem;

        if (sourceTabItem == null || e.OriginalSource is not DependencyObject originalSource)
        {
            ResetDrag();
            return;
        }

        var targetContainer = StswFnUI.FindVisualAncestor<StswTabItem>(originalSource);
        var sourceIndex = ItemContainerGenerator.IndexFromContainer(sourceTabItem);
        var targetIndex = targetContainer != null
            ? ItemContainerGenerator.IndexFromContainer(targetContainer)
            : Items.Count - 1;

        if (sourceIndex < 0 || targetIndex < 0)
        {
            ResetDrag();
            return;
        }

        MoveItem(sourceIndex, targetIndex);

        e.Handled = true;
        ResetDrag();
    }

    /// <summary>
    /// Resets the drag state and releases mouse capture if necessary.
    /// </summary>
    private void ResetDrag()
    {
        if (_isDragging)
            ReleaseMouseCapture();

        _isDragging = false;
        _draggedItem = null;
    }

    /// <summary>
    /// Reorders the tab items by moving the source tab item to the position of the target tab item.
    /// </summary>
    /// <param name="sourceTabItem">The tab item being dragged.</param>
    /// <param name="targetTabItem">The tab item where the source item is dropped.</param>
    private void ReorderTabItems(StswTabItem sourceTabItem, StswTabItem targetTabItem)
    {
        var sourceIndex = ItemContainerGenerator.IndexFromContainer(sourceTabItem);
        var targetIndex = ItemContainerGenerator.IndexFromContainer(targetTabItem);

        if (sourceIndex < 0 || targetIndex < 0)
            return;

        MoveItem(sourceIndex, targetIndex);
    }

    /// <summary>
    /// Moves an item within the tab control's items or bound item source from the source index to the target index.
    /// </summary>
    /// <param name="sourceIndex">The index of the item to move.</param>
    /// <param name="targetIndex">The index where the item should be moved to.</param>
    private void MoveItem(int sourceIndex, int targetIndex)
    {
        if (sourceIndex < 0 || targetIndex < 0 || sourceIndex == targetIndex)
            return;

        var movedItem = MoveWithinList(ItemsSource is IList boundList ? boundList : Items, sourceIndex, targetIndex);
        if (movedItem == null)
            return;

        var newIndex = Items.IndexOf(movedItem);
        if (newIndex < 0)
            newIndex = Math.Max(0, Math.Min(targetIndex, Items.Count - 1));

        SelectedIndex = newIndex;

        if (ItemContainerGenerator.ContainerFromItem(movedItem) is StswTabItem newContainer)
            _draggedItem = newContainer;
    }

    /// <summary>
    /// Moves an item within a given list from the source index to the target index.
    /// </summary>
    /// <param name="list">The list containing the item to move.</param>
    /// <param name="sourceIndex">The index of the item to move.</param>
    /// <param name="targetIndex">The index where the item should be moved to.</param>
    /// <returns>The moved item, or <see langword="null"/> if the move was unsuccessful.</returns>
    private static object? MoveWithinList(IList list, int sourceIndex, int targetIndex)
    {
        if (sourceIndex < 0 || sourceIndex >= list.Count)
            return null;

        var movedItem = list[sourceIndex];
        list.RemoveAt(sourceIndex);

        targetIndex = Math.Max(0, Math.Min(targetIndex, list.Count));
        list.Insert(targetIndex, movedItem);

        return movedItem;
    }
    #endregion
}
