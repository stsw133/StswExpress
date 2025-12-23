using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace StswExpress.Wpf;
/// <summary>
/// A tab control with extended functionality, including dynamic tab creation, 
/// visibility toggling, and identifier-based tab management.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswTabControl Identifier="MainTabs" AreTabsVisible="True" NewItemButtonVisibility="Visible"&gt;
///     &lt;se:StswTabItem Header="Home"/&gt;
///     &lt;se:StswTabItem Header="Settings"/&gt;
/// &lt;/se:StswTabControl&gt;
///
/// &lt;!-- somewhere else in code --&gt;
/// var newTab = StswTabControl.Add("MainTabs");
/// newTab.Header = "Dynamic tab";
/// </code>
/// </example>
public class StswTabControl : TabControl
{
    static StswTabControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTabControl), new FrameworkPropertyMetadata(typeof(StswTabControl)));
    }
    public StswTabControl()
    {
        _previewMouseLeftButtonDownHandler = OnTabPreviewMouseLeftButtonDown;
        _mouseMoveHandler = OnTabMouseMove;
        _mouseLeftButtonUpHandler = OnTabMouseLeftButtonUp;
        _dropHandler = OnTabDrop;
        _dragOverHandler = OnTabDragOver;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #region Dependency properties
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
    /// Gets or sets an identifier for the tab control instance, allowing for easy retrieval and manipulation of specific tab controls in code.
    /// </summary>
    public object? Identifier
    {
        get => GetValue(IdentifierProperty);
        set => SetValue(IdentifierProperty, value);
    }
    public static readonly DependencyProperty IdentifierProperty
        = DependencyProperty.Register(
            nameof(Identifier),
            typeof(object),
            typeof(StswTabControl)
        );

    /// <summary>
    /// Gets or sets the template used to create a new tab item when the add button is invoked.
    /// </summary>
    public DataTemplate? NewItemTemplate
    {
        get => (DataTemplate?)GetValue(NewItemTemplateProperty);
        set => SetValue(NewItemTemplateProperty, value);
    }
    public static readonly DependencyProperty NewItemTemplateProperty
        = DependencyProperty.Register(
            nameof(NewItemTemplate),
            typeof(DataTemplate),
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

    #region Template
    private static readonly HashSet<WeakReference<StswTabControl>> _loadedInstances = [];
    private readonly MouseButtonEventHandler _previewMouseLeftButtonDownHandler;
    private readonly MouseEventHandler _mouseMoveHandler;
    private readonly MouseButtonEventHandler _mouseLeftButtonUpHandler;
    private readonly DragEventHandler _dropHandler;
    private readonly DragEventHandler _dragOverHandler;

    private ICommand? _newItemCommand;
    private ButtonBase? _newItemButton;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _newItemButton = GetTemplateChild("OPT_NewItemButton") as ButtonBase;
        if (_newItemButton is not null)
            _newItemButton.Command = _newItemCommand ??= new StswCommand(() => AddTab());

        UpdateReorderHandlers(CanReorder);
        UpdateTabItemsAllowDrop();
    }

    /// <summary>
    /// Tracks loaded instances to enable identifier-based lookups.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        foreach (var weakRef in _loadedInstances.ToList())
            if (weakRef.TryGetTarget(out StswTabControl? tabControl) && ReferenceEquals(tabControl, this))
                return;

        _loadedInstances.Add(new WeakReference<StswTabControl>(this));
    }

    /// <summary>
    /// Removes unloaded instances to prevent memory leaks.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        foreach (var weakRef in _loadedInstances.ToList())
            if (!weakRef.TryGetTarget(out StswTabControl? tabControl) || ReferenceEquals(tabControl, this))
            {
                _loadedInstances.Remove(weakRef);
                break;
            }
    }
    #endregion

    #region Overrides
    /// <inheritdoc/>
    protected override DependencyObject GetContainerForItemOverride() => new StswTabItem();
    /// <inheritdoc/>
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswTabItem;
    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is TabItem tabItem)
            tabItem.AllowDrop = CanReorder;
    }

    /// <inheritdoc/>
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);

        if (CanReorder)
            UpdateTabItemsAllowDrop();
    }
    #endregion

    #region Logic
    /// <summary>
    /// Adds a new tab item to the control identified by the provided <paramref name="tabControlIdentifier"/>.
    /// </summary>
    /// <param name="tabControlIdentifier">The <see cref="Identifier"/> value of the target <see cref="StswTabControl"/>.</param>
    /// <returns>The created <see cref="StswTabItem"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no matching control instance is found or when multiple matches are detected.</exception>
    public static StswTabItem Add(object? tabControlIdentifier)
    {
        var tabControl = GetInstance(tabControlIdentifier);
        return tabControl.AddTab();
    }

    /// <summary>
    /// Adds a new tab item to the control using <see cref="NewItemTemplate"/> or a default <see cref="StswTabItem"/> instance.
    /// Returns the created <see cref="StswTabItem"/> so it can be further customized by the caller.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the created item cannot be converted to <see cref="StswTabItem"/>.</exception>
    private StswTabItem AddTab()
    {
        var newItem = CreateNewItemInstance ?? throw new InvalidOperationException("Failed to create a new tab item. The NewItemTemplate or default constructor returned null.");

        if (ItemsSource is IList list)
            list.Add(newItem);
        else Items.Add(newItem);

        SetCurrentValue(SelectedItemProperty, newItem);

        return EnsureTabContainer(newItem);
    }

    /// <summary>
    /// Instantiates a new <see cref="StswTabItem"/> based on the <see cref="NewItemTemplate"/>, or a default instance when no template is provided.
    /// </summary>
    /// <returns>The created tab item.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="NewItemTemplate"/> does not create a <see cref="StswTabItem"/>.</exception>
    private object? CreateNewItemInstance
    {
        get
        {
            if (NewItemTemplate?.LoadContent() is { } templateItem)
            {
                if (templateItem is not StswTabItem templatedTab)
                    throw new InvalidOperationException($"{nameof(NewItemTemplate)} must create a {nameof(StswTabItem)} instance.");

                return templatedTab;
            }

            return new StswTabItem();
        }
    }

    /// <summary>
    /// Ensures the created item is presented as a <see cref="StswTabItem"/>, generating the container if necessary.
    /// </summary>
    /// <param name="item">The item that was added to the control.</param>
    /// <returns>The corresponding <see cref="StswTabItem"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a tab item container cannot be resolved.</exception>
    private StswTabItem EnsureTabContainer(object item)
    {
        if (item is StswTabItem tabItem)
            return tabItem;

        var container = ItemContainerGenerator.ContainerFromItem(item) as StswTabItem;
        if (container is not null)
            return container;

        UpdateLayout();
        container = ItemContainerGenerator.ContainerFromItem(item) as StswTabItem;

        return container ?? throw new InvalidOperationException($"Item created for {nameof(StswTabControl)} must be of type {nameof(StswTabItem)}.");
    }

    /// <summary>
    /// Finds the <see cref="StswTabControl"/> instance matching the provided identifier.
    /// </summary>
    /// <param name="tabControlIdentifier">The identifier used to locate the control.</param>
    /// <returns>The matching control instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no matching instance is found or when multiple matches are detected.</exception>
    internal static StswTabControl GetInstance(object? tabControlIdentifier)
    {
        if (_loadedInstances.Count == 0)
            throw new InvalidOperationException($"No loaded {nameof(StswTabControl)} instances.");

        var targets = new List<StswTabControl>();
        foreach (var instance in _loadedInstances.ToList())
        {
            if (instance.TryGetTarget(out var tabInstance))
            {
                object? identifier = null;

                if (tabInstance.CheckAccess())
                    identifier = tabInstance.Identifier;
                else identifier = tabInstance.Dispatcher.Invoke(() => tabInstance.Identifier);

                if (Equals(tabControlIdentifier, identifier))
                    targets.Add(tabInstance);
            }
            else _loadedInstances.Remove(instance);
        }

        if (targets.Count == 0)
            throw new InvalidOperationException($"No loaded {nameof(StswTabControl)} have an {nameof(Identifier)} property matching {nameof(tabControlIdentifier)} ('{tabControlIdentifier}') argument.");
        if (targets.Count > 1)
            throw new InvalidOperationException($"Multiple viable {nameof(StswTabControl)}s. Specify a unique Identifier on each {nameof(StswTabControl)}, especially where multiple Windows are a concern.");

        return targets[0];
    }
    #endregion

    #region Drag & drop logic
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
