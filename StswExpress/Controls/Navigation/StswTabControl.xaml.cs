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
[StswInfo("0.1.0", "0.21.0")]
public class StswTabControl : TabControl
{
    public StswTabControl()
    {
        _previewMouseLeftButtonDownHandler = OnTabPreviewMouseLeftButtonDown;
        _mouseMoveHandler = OnTabMouseMove;
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
    [StswInfo("0.21.0")]
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
    private readonly DragEventHandler _dropHandler;
    private readonly DragEventHandler _dragOverHandler;

    private Point _dragStartPoint;
    private TabItem? _draggedItem;
    private bool _reorderHandlersAttached;

    /// <summary>
    /// Called when the CanReorder property changes to attach or detach drag-and-drop event handlers.
    /// </summary>
    /// <param name="obj">The dependency object where the property changed.</param>
    /// <param name="e">The event data.</param>
    [StswInfo("0.21.0")]
    private static void OnCanReorderChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswTabControl control)
        {
            control.UpdateReorderHandlers((bool)e.NewValue);
            control.UpdateTabItemsAllowDrop();
        }
    }

    /// <summary>
    /// Attaches or detaches drag-and-drop event handlers based on the CanReorder property.
    /// </summary>
    /// <param name="enable">If set to <see langword="true"/>, attaches the event handlers; otherwise, detaches them.</param>
    [StswInfo("0.21.0")]
    private void UpdateReorderHandlers(bool enable)
    {
        if (enable && !_reorderHandlersAttached)
        {
            AddHandler(PreviewMouseLeftButtonDownEvent, _previewMouseLeftButtonDownHandler, true);
            AddHandler(MouseMoveEvent, _mouseMoveHandler, true);
            AddHandler(DropEvent, _dropHandler, true);
            AddHandler(DragOverEvent, _dragOverHandler, true);
            _reorderHandlersAttached = true;
        }
        else if (!enable && _reorderHandlersAttached)
        {
            RemoveHandler(PreviewMouseLeftButtonDownEvent, _previewMouseLeftButtonDownHandler);
            RemoveHandler(MouseMoveEvent, _mouseMoveHandler);
            RemoveHandler(DropEvent, _dropHandler);
            RemoveHandler(DragOverEvent, _dragOverHandler);
            _reorderHandlersAttached = false;
        }
    }

    /// <summary>
    /// Updates the AllowDrop property of each tab item based on the CanReorder property.
    /// </summary>
    [StswInfo("0.21.0")]
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
    [StswInfo("0.21.0")]
    private void OnTabPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!CanReorder)
            return;

        if (e.OriginalSource is not DependencyObject originalSource)
            return;

        _dragStartPoint = e.GetPosition(null);
        _draggedItem = StswFnUI.FindVisualAncestor<StswTabItem>(originalSource);
    }

    /// <summary>
    /// Handles the MouseMove event to perform drag-and-drop operations for tab reordering.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    [StswInfo("0.21.0")]
    private void OnTabMouseMove(object sender, MouseEventArgs e)
    {
        if (!CanReorder || e.LeftButton != MouseButtonState.Pressed)
        {
            _draggedItem = null;
            return;
        }

        if (_draggedItem == null)
            return;

        var currentPosition = e.GetPosition(null);
        if (Math.Abs(currentPosition.X - _dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance
         || Math.Abs(currentPosition.Y - _dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
        {
            DragDrop.DoDragDrop(_draggedItem, _draggedItem, DragDropEffects.Move);
            _draggedItem = null;
        }
    }

    /// <summary>
    /// Handles the DragOver event to provide visual feedback during drag-and-drop operations for tab reordering.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    [StswInfo("0.21.0")]
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
    [StswInfo("0.21.0")]
    private void OnTabDrop(object sender, DragEventArgs e)
    {
        if (!CanReorder || !e.Data.GetDataPresent(typeof(StswTabItem)))
            return;

        if (e.OriginalSource is not DependencyObject originalSource)
            return;

        var targetContainer = StswFnUI.FindVisualAncestor<StswTabItem>(originalSource);
        var targetIndex = targetContainer != null
            ? ItemContainerGenerator.IndexFromContainer(targetContainer)
            : Items.Count;

        if (targetIndex < 0)
            return;

        if (e.Data.GetData(typeof(StswTabItem)) is not StswTabItem sourceTabItem || sourceTabItem == targetContainer)
            return;

        var sourceIndex = ItemContainerGenerator.IndexFromContainer(sourceTabItem);
        if (sourceIndex < 0 || targetIndex < 0 || sourceIndex == targetIndex)
            return;

        if (ItemsSource is IList list)
        {
            var item = list[sourceIndex];
            list.RemoveAt(sourceIndex);
            if (sourceIndex < targetIndex)
                targetIndex--;
            list.Insert(targetIndex, item);
        }
        else
        {
            var item = Items[sourceIndex];
            Items.RemoveAt(sourceIndex);
            if (sourceIndex < targetIndex)
                targetIndex--;
            Items.Insert(targetIndex, item);
        }

        SelectedIndex = targetIndex;
        e.Handled = true;
    }
    #endregion
}
