using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        NewItemCommand = new StswCommand(CreateItem);
        UpdateReorderHandlers(CanReorder);
        UpdateTabItemsAllowDrop();
    }

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
        {
            UpdateTabItemsAllowDrop();
        }
    }

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is TabItem tabItem)
        {
            tabItem.AllowDrop = CanReorder;
        }
    }
    #endregion

    #region Logic properties
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
    public static readonly DependencyProperty NewItemButtonVisibilityProperty
        = DependencyProperty.Register(
            nameof(NewItemButtonVisibility),
            typeof(Visibility),
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
    #endregion

    #region Drag & drop logic
    private readonly MouseButtonEventHandler _previewMouseLeftButtonDownHandler;
    private readonly MouseEventHandler _mouseMoveHandler;
    private readonly DragEventHandler _dropHandler;
    private readonly DragEventHandler _dragOverHandler;

    private Point _dragStartPoint;
    private TabItem? _draggedItem;
    private bool _reorderHandlersAttached;

    private static void OnCanReorderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StswTabControl control)
        {
            control.UpdateReorderHandlers((bool)e.NewValue);
            control.UpdateTabItemsAllowDrop();
        }
    }

    private void UpdateReorderHandlers(bool enable)
    {
        if (enable && !_reorderHandlersAttached)
        {
            AddHandler(TabItem.PreviewMouseLeftButtonDownEvent, _previewMouseLeftButtonDownHandler, true);
            AddHandler(TabItem.MouseMoveEvent, _mouseMoveHandler, true);
            AddHandler(TabItem.DropEvent, _dropHandler, true);
            AddHandler(TabItem.DragOverEvent, _dragOverHandler, true);
            _reorderHandlersAttached = true;
        }
        else if (!enable && _reorderHandlersAttached)
        {
            RemoveHandler(TabItem.PreviewMouseLeftButtonDownEvent, _previewMouseLeftButtonDownHandler);
            RemoveHandler(TabItem.MouseMoveEvent, _mouseMoveHandler);
            RemoveHandler(TabItem.DropEvent, _dropHandler);
            RemoveHandler(TabItem.DragOverEvent, _dragOverHandler);
            _reorderHandlersAttached = false;
        }
    }

    private void UpdateTabItemsAllowDrop()
    {
        foreach (var item in Items)
        {
            if (ItemContainerGenerator.ContainerFromItem(item) is TabItem tabItem)
            {
                tabItem.AllowDrop = CanReorder;
            }
        }
    }

    private void OnTabPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!CanReorder)
        {
            return;
        }

        _dragStartPoint = e.GetPosition(null);
        _draggedItem = FindAncestorTabItem(e.OriginalSource as DependencyObject);
    }

    private void OnTabMouseMove(object sender, MouseEventArgs e)
    {
        if (!CanReorder || e.LeftButton != MouseButtonState.Pressed)
        {
            _draggedItem = null;
            return;
        }

        if (_draggedItem == null)
        {
            return;
        }

        var currentPosition = e.GetPosition(null);
        if (Math.Abs(currentPosition.X - _dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(currentPosition.Y - _dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
        {
            DragDrop.DoDragDrop(_draggedItem, _draggedItem, DragDropEffects.Move);
            _draggedItem = null;
        }
    }

    private void OnTabDragOver(object sender, DragEventArgs e)
    {
        if (CanReorder && e.Data.GetDataPresent(typeof(TabItem)))
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }
    }

    private void OnTabDrop(object sender, DragEventArgs e)
    {
        if (!CanReorder || !e.Data.GetDataPresent(typeof(TabItem)))
        {
            return;
        }

        var targetContainer = FindAncestorTabItem(e.OriginalSource as DependencyObject);
        var targetIndex = targetContainer != null
            ? ItemContainerGenerator.IndexFromContainer(targetContainer)
            : Items.Count;

        if (targetIndex < 0)
        {
            return;
        }

        if (e.Data.GetData(typeof(TabItem)) is not TabItem sourceTabItem || sourceTabItem == targetContainer)
        {
            return;
        }

        var sourceIndex = ItemContainerGenerator.IndexFromContainer(sourceTabItem);

        if (sourceIndex < 0 || targetIndex < 0 || sourceIndex == targetIndex)
        {
            return;
        }

        if (ItemsSource is IList list)
        {
            var item = list[sourceIndex];
            list.RemoveAt(sourceIndex);
            if (sourceIndex < targetIndex)
            {
                targetIndex--;
            }
            list.Insert(targetIndex, item);
        }
        else
        {
            var item = Items[sourceIndex];
            Items.RemoveAt(sourceIndex);
            if (sourceIndex < targetIndex)
            {
                targetIndex--;
            }
            Items.Insert(targetIndex, item);
        }

        SelectedIndex = targetIndex;
        e.Handled = true;
    }

    private static TabItem? FindAncestorTabItem(DependencyObject? source)
    {
        while (source != null && source is not TabItem)
        {
            source = VisualTreeHelper.GetParent(source);
        }

        return source as TabItem;
    }
    #endregion
}
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
}
