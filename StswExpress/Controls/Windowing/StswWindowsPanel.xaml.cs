using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace StswExpress;

public class StswWindowsPanel : ItemsControl, IStswCornerControl
{
    private object? _dragDropItem;
    private FrameworkElement? _dragDropItemContainer;
    private object? _dragDropTabItem;
    private FrameworkElement? _dragDropTabItemContainer;
    private Point? _onBarPosition;
    private Point? _onTabPosition;
    private int _zIndexCounter = 1;
    private ListBox? _tabList;
    private ItemsPresenter? _windowsHost;

    private int? _tabDragStartIndex;
    private int? _tabDragCurrentIndex;

    static StswWindowsPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswWindowsPanel), new FrameworkPropertyMetadata(typeof(StswWindowsPanel)));
    }

    public StswWindowsPanel()
    {
        ItemContainerStyle = new Style(typeof(StswWindowPanelItem));

        ItemContainerStyle.Setters.Add(new EventSetter()
        {
            Event = StswWindowPanelItem.BarMouseMoveEvent,
            Handler = new MouseEventHandler(OnItemMouseMouse)
        });
        ItemContainerStyle.Setters.Add(new EventSetter()
        {
            Event = StswWindowPanelItem.BarMouseDoubleClickEvent,
            Handler = new MouseButtonEventHandler(OnBarMouseDoubleClick)
        });
        ItemContainerStyle.Setters.Add(new EventSetter()
        {
            Event = StswWindowPanelItem.WindowMouseClickEvent,
            Handler = new MouseButtonEventHandler(OnItemMouseDown)
        });
        ItemContainerStyle.Setters.Add(new EventSetter()
        {
            Event = StswWindowPanelItem.CloseBtnClickedEvent,
            Handler = new RoutedEventHandler(OnCloseBtnClick)
        });
    }

    public new IList ItemsSource
    {
        get => (IList)base.ItemsSource; // Get the original ItemsSource
        set
        {
            if (value != null && !(value is IList))
                throw new InvalidOperationException("ItemsSource must be an IList.");

            base.ItemsSource = value; // Set ItemsSource using base logic
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        DragOver += StswWindowsPanel_DragOver;
        SizeChanged += StswWindowsPanel_SizeChanged;

        _windowsHost = GetTemplateChild("PART_WindowsHost") as ItemsPresenter;
        _tabList = GetTemplateChild("PART_TabList") as ListBox;

        _tabList.ItemContainerStyle = new Style(typeof(ListBoxItem));
        _tabList.ItemContainerStyle.Setters.Add(new EventSetter()
        {
            Event = MouseMoveEvent,
            Handler = new MouseEventHandler(OnTabMouseMove)
        });
        _tabList.ItemContainerStyle.Setters.Add(new EventSetter()
        {
            Event = UIElement.DragOverEvent,
            Handler = new DragEventHandler(OnTabDragOver)
        });

        //RearangeTabs();
        _tabList.Loaded += (_, _) => RearangeTabs();
        _tabList.DragOver += _tabList_DragOver;
    }

    private void _tabList_DragOver(object sender, DragEventArgs e)
    {
        if (_dragDropTabItemContainer == null)
            return;
        var dragX = e.GetPosition(_tabList).X;
        Canvas.SetLeft(_dragDropTabItemContainer, dragX);
        var hoverIndex = (int)dragX / 100;
        if (_tabDragCurrentIndex != hoverIndex)
        {
            _tabDragCurrentIndex = Math.Min(hoverIndex, Items.Count - 1);
            MoveTabs();
        }
    }

    private void MoveTabs()
    {
        if (_tabDragCurrentIndex == _tabDragStartIndex)
            AnimateTabsToStartPosition();
        else if (_tabDragCurrentIndex > _tabDragStartIndex)
            AnimateTabsWhenMovedToRight();
        else if (_tabDragCurrentIndex < _tabDragStartIndex)
            AnimateTabsWhenMovedToLeft();
    }

    private void AnimateTabsWhenMovedToLeft()
    {
        foreach (var tabItem in _tabList.Items)
        {
            if (tabItem == _dragDropTabItem)
                return;

            var index = _tabList.Items.IndexOf(tabItem);

            if (index >= _tabDragStartIndex)
                return;
            //index < _tabDragCurrentIndex

            var tab = _tabList.ItemContainerGenerator.ContainerFromItem(tabItem) as FrameworkElement;

            int leftValue = 0;
            if (index < _tabDragCurrentIndex)
                leftValue = index * 100;
            else
                leftValue = (index + 1) * 100;

            var startValue = Canvas.GetLeft(tab);
            if (startValue == leftValue)
                return;

            if (double.IsNaN(startValue))
                startValue = 0;

            var sb = new Storyboard();
            var anim = new DoubleAnimation(
                fromValue: startValue,
               toValue: leftValue,
               duration: TimeSpan.FromMilliseconds(300),
               fillBehavior: FillBehavior.HoldEnd)
            {
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            sb.Children.Add(anim);
            Storyboard.SetTarget(anim, tab);
            Storyboard.SetTargetProperty(anim, new PropertyPath(Canvas.LeftProperty));
            sb.Begin();
        }
    }

    private void AnimateTabsWhenMovedToRight()
    {
        foreach (var tabItem in _tabList.Items)
        {
            if (tabItem == _dragDropTabItem)
                return;

            var index = _tabList.Items.IndexOf(tabItem);

            if (index <= _tabDragStartIndex)
                return;

            var tab = _tabList.ItemContainerGenerator.ContainerFromItem(tabItem) as FrameworkElement;

            int leftValue = 0;
            if (index > _tabDragCurrentIndex)
                leftValue = index * 100;
            else
                leftValue = (index - 1) * 100;

            var startValue = Canvas.GetLeft(tab);
            if (startValue == leftValue)
                return;

            if (double.IsNaN(startValue))
                startValue = 0;

            var sb = new Storyboard();
            var anim = new DoubleAnimation(
                fromValue: startValue,
               toValue: leftValue,
               duration: TimeSpan.FromMilliseconds(300),
               fillBehavior: FillBehavior.HoldEnd)
            {
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            sb.Children.Add(anim);
            Storyboard.SetTarget(anim, tab);
            Storyboard.SetTargetProperty(anim, new PropertyPath(Canvas.LeftProperty));
            sb.Begin();
        }
    }

    private void AnimateTabsToStartPosition()
    {
        foreach (var tabItem in _tabList.Items)
        {
            if (tabItem == _dragDropTabItem)
                return;

            var tab = _tabList.ItemContainerGenerator.ContainerFromItem(tabItem) as FrameworkElement;
            var index = _tabList.Items.IndexOf(tabItem);
            var leftValue = index * 100;
            var startValue = Canvas.GetLeft(tab);
            if (startValue == leftValue)
                return;

            var sb = new Storyboard();

            if (double.IsNaN(startValue))
                startValue = 0;

            var anim = new DoubleAnimation(
                fromValue: startValue,
                toValue: leftValue,
                duration: TimeSpan.FromMilliseconds(300),
                fillBehavior: FillBehavior.HoldEnd)
            {
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            sb.Children.Add(anim);
            Storyboard.SetTarget(anim, tab);
            Storyboard.SetTargetProperty(anim, new PropertyPath(Canvas.LeftProperty));
            sb.Begin();
        }
    }

    private void StswWindowsPanel_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        foreach (var item in Items)
        {
            if (item is StswWindowPanelItem window && window.WindowState == WindowState.Maximized)
            {
                window.Width = ActualWidth;
                window.Height = ActualHeight;
            }
        }
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        foreach (var item in Items)
        {
            var container = ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
            if (container is StswWindowPanelItem window && window.WindowState == WindowState.Maximized)
            {
                window.Width = ActualWidth >= window.MinWidth ? ActualWidth : window.MinWidth;
                window.Height = ActualHeight >= window.MinHeight ? ActualHeight : window.MinHeight; ;
            }
        }
    }

    private void StswWindowsPanel_DragOver(object sender, DragEventArgs e)
    {
        if (_dragDropItemContainer == null)
            return;
        var dropPosition = e.GetPosition(_windowsHost);
        Canvas.SetLeft(_dragDropItemContainer, dropPosition.X - _onBarPosition.Value.X);
        Canvas.SetTop(_dragDropItemContainer, dropPosition.Y - _onBarPosition.Value.Y);
    }

    private void OnItemMouseMouse(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed
           && sender is FrameworkElement panelControl
           && e is BarMouseEventArgs args)
        {
            _onBarPosition = args.OnBarPosition;
            _dragDropItemContainer = panelControl;
            _dragDropItem = panelControl.DataContext;
            try
            {
                DragDropEffects dragDropResult = DragDrop.DoDragDrop(panelControl,
                    new DataObject(DataFormats.Serializable, _dragDropItem),
                    DragDropEffects.Move);
            }
            catch { throw; }
            finally
            {
                _onBarPosition = null;
                _dragDropItem = null;
                _dragDropItemContainer = null;
            }
        }
    }

    private void OnBarMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is StswWindowPanelItem panelControl)
        {
            panelControl.WindowState = panelControl.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }
    }

    private void OnItemMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed
            && sender is FrameworkElement panelControl)
        {
            Panel.SetZIndex(panelControl, _zIndexCounter++);

            if (_zIndexCounter == int.MaxValue)
                NormalizeZIndex();
        }
    }

    private void OnCloseBtnClick(object sender, RoutedEventArgs e)
    {
        var container = sender as FrameworkElement;
        var item = container?.DataContext;
        CloseCommand?.Execute(item);
    }

    #region TABS
    private void OnTabMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed
           && sender is FrameworkElement panelControl)
        {
            _onTabPosition = e.GetPosition(panelControl);
            _dragDropTabItemContainer = panelControl;
            _dragDropTabItem = panelControl.DataContext;
            _tabDragStartIndex = _tabList.Items.IndexOf(_dragDropTabItem);
            _tabDragCurrentIndex = _tabDragStartIndex;
            try
            {
                DragDropEffects dragDropResult = DragDrop.DoDragDrop(panelControl,
                    new DataObject(DataFormats.Serializable, _dragDropTabItem),
                    DragDropEffects.Move);

                if (_tabDragStartIndex != _tabDragCurrentIndex)
                    MoveItem(ItemsSource, (int)_tabDragStartIndex, (int)_tabDragCurrentIndex);

                RearangeTabs((int)_tabDragCurrentIndex);
            }
            catch { throw; }
            finally
            {
                _onTabPosition = null;
                _dragDropTabItem = null;
                _dragDropTabItemContainer = null;
                _tabDragStartIndex = null;
            }
        }
    }

    public static void MoveItem(IList list, int oldIndex, int newIndex)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list));
        if (oldIndex < 0 || oldIndex >= list.Count || newIndex < 0 || newIndex >= list.Count)
            throw new ArgumentOutOfRangeException("Indices must be within the bounds of the list.");

        object item = list[oldIndex];

        if (oldIndex < newIndex)
        {
            for (int i = oldIndex; i < newIndex; i++)
            {
                list[i] = list[i + 1];
            }
        }
        else
        {
            for (int i = oldIndex; i > newIndex; i--)
            {
                list[i] = list[i - 1];
            }
        }

        list[newIndex] = item;
    }

    private void OnTabDragOver(object sender, DragEventArgs e)
    {
        return;
        if (sender is FrameworkElement tabToMove)
        {
            var itemToMove = tabToMove.DataContext;
            var draggingTabItem = e.Data.GetData(DataFormats.Serializable);

            if (itemToMove == draggingTabItem)
                return;

            var index = _tabList.Items.IndexOf(_dragDropTabItem);
            var movingTabIndex = _tabList.Items.IndexOf(itemToMove);

            if (_dragDropTabItem == null || _dragDropTabItem != draggingTabItem)
                return;

            if (_movingTabs.Contains(tabToMove))
                return;

            _movingTabs.Add(tabToMove);
            MoveTabToIndex(tabToMove,
                index,
                () =>
                {
                    _movingTabs.Remove(tabToMove);
                    SwapItems(ItemsSource, index, movingTabIndex); // Swaps items at index 0 and 2
                    _tabList.Items.Refresh();
                    RearangeTabs();
                });
        }
    }

    private void MoveTabToIndex(FrameworkElement tabToMove, int index, Action continueWith)
    {
        var sb = new Storyboard()
        {
            Duration = TimeSpan.FromMilliseconds(300)
        };

        var canvasLeft = Canvas.GetLeft(tabToMove);

        if (double.IsNaN(canvasLeft))
            canvasLeft = 0;

        var tabAnim = new DoubleAnimation(
            fromValue: canvasLeft,
            toValue: index * 100,
            TimeSpan.FromMilliseconds(300),
            FillBehavior.HoldEnd)
        {
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut },
        };
        sb.Children.Add(tabAnim);
        Storyboard.SetTarget(tabAnim, tabToMove);
        Storyboard.SetTargetProperty(tabAnim, new PropertyPath(Canvas.LeftProperty));

        sb.Completed += (_, _) => continueWith.Invoke();

        sb.Begin();
    }

    private List<FrameworkElement> _movingTabs = [];

    public void SwapItems(IList list, int index1, int index2)
    {
        if (list == null || index1 < 0 || index2 < 0 || index1 >= list.Count || index2 >= list.Count)
            return;

        // Swap items
        (list[index1], list[index2]) = (list[index2], list[index1]);
    }

    #endregion

    private void NormalizeZIndex()
    {
        var items = ItemsSource.Cast<object>()
            .Select(x => ItemContainerGenerator.ContainerFromItem(x) as FrameworkElement)
            .Where(i => i != null)
            .OrderBy(i => Panel.GetZIndex(i))
            .ToList();

        for (int i = 0; i < items.Count; i++)
        {
            Panel.SetZIndex(items[i], i);
        }

        _zIndexCounter = items.Count;
    }

    private void RearangeTabs()
    {
        foreach (var item in Items)
        {
            if (_dragDropTabItem == item)
                continue;
            var container = _tabList.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
            var index = _tabList.ItemsSource.IndexOf(item);
            Canvas.SetLeft(container, index * 100);
        }
    }

    private void RearangeTabs(int index)
    {
        var tabItem = _tabList.ItemsSource.Cast<object>().ToList()[index];
        var container = _tabList.ItemContainerGenerator.ContainerFromItem(tabItem) as FrameworkElement;
        Canvas.SetLeft(container, index * 100);

    }


    protected override DependencyObject GetContainerForItemOverride() => new StswWindowPanelItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswWindowPanelItem;

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
            typeof(StswWindowsPanel),
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
            typeof(StswWindowsPanel),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion

    #region Logic properties
    public ICommand CloseCommand
    {
        get { return (ICommand)GetValue(CloseCommandProperty); }
        set { SetValue(CloseCommandProperty, value); }
    }
    public static readonly DependencyProperty CloseCommandProperty =
        DependencyProperty.Register(nameof(CloseCommand),
            typeof(ICommand),
            typeof(StswWindowsPanel),
            new PropertyMetadata(null));
    #endregion
}

public class StswWindowPanelItem : ListBoxItem
{
    #region PRIVATE
    StswSubButton? _maximizeBtn;
    StswSubButton? _minimizeBtn;
    StswSubButton? _closeBtn;
    Panel? _barPanel;

    private Point _startPoint;
    double _startWidth = 0.0;
    double _startCanvasLeft = 0.0;
    double _startHeight = 0.0;
    double _startCanvasTop = 0.0;

    Rectangle? _rightResizeBorder;
    bool _isRightDragging = false;
    Rectangle? _leftResizeBorder;
    bool _isLeftDragging = false;
    Rectangle? _topResizeBorder;
    bool _isTopDragging = false;
    Rectangle? _bottomResizeBorder;
    bool _isBottomDragging = false;

    Rectangle? _topRightResizeBorder;
    bool _isTopRightDragging = false;
    Rectangle? _topLeftResizeBorder;
    bool _isTopLeftDragging = false;
    Rectangle? _bottomRightResizeBorder;
    bool _isBottomRightDragging = false;
    Rectangle? _bottomLeftResizeBorder;
    bool _isBottomLeftDragging = false;

    double _normalWidth = 0.0;
    double _normalHeight = 0.0;
    double _normalLeft = 0.0;
    double _normalTop = 0.0;
    #endregion

    #region PUBLIC EVENTS
    public event EventHandler<MouseEventArgs>? BarMouseMove;
    #endregion

    static StswWindowPanelItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswWindowPanelItem), new FrameworkPropertyMetadata(typeof(StswWindowPanelItem)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswWindowPanelItem), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _maximizeBtn = GetTemplateChild("PART_MaximizeBtn") as StswSubButton;
        _minimizeBtn = GetTemplateChild("PART_MinimizeBtn") as StswSubButton;
        _closeBtn = GetTemplateChild("PART_CloseBtn") as StswSubButton;
        _barPanel = GetTemplateChild("PART_BarPanel") as Panel;

        _rightResizeBorder = GetTemplateChild("PART_RightResizeBorder") as Rectangle;
        _rightResizeBorder.MouseDown += _rightResizeBorder_MouseDown;
        _rightResizeBorder.MouseMove += _rightResizeBorder_MouseMove;
        _rightResizeBorder.MouseUp += _rightResizeBorder_MouseUp;

        _leftResizeBorder = GetTemplateChild("PART_LeftResizeBorder") as Rectangle;
        _leftResizeBorder.MouseDown += _leftResizeBorder_MouseDown;
        _leftResizeBorder.MouseMove += _leftResizeBorder_MouseMove;
        _leftResizeBorder.MouseUp += _leftResizeBorder_MouseUp;

        _topResizeBorder = GetTemplateChild("PART_TopResizeBorder") as Rectangle;
        _topResizeBorder.MouseDown += _topResizeBorder_MouseDown;
        _topResizeBorder.MouseMove += _topResizeBorder_MouseMove;
        _topResizeBorder.MouseUp += _topResizeBorder_MouseUp;

        _bottomResizeBorder = GetTemplateChild("PART_BottomResizeBorder") as Rectangle;
        _bottomResizeBorder.MouseDown += _bottomResizeBorder_MouseDown;
        _bottomResizeBorder.MouseMove += _bottomResizeBorder_MouseMove;
        _bottomResizeBorder.MouseUp += _bottomResizeBorder_MouseUp;

        _topRightResizeBorder = GetTemplateChild("PART_TopRightResizeBorder") as Rectangle;
        _topRightResizeBorder.MouseDown += _topRightResizeBorder_MouseDown;
        _topRightResizeBorder.MouseMove += _topRightResizeBorder_MouseMove;
        _topRightResizeBorder.MouseUp += _topRightResizeBorder_MouseUp;

        _topLeftResizeBorder = GetTemplateChild("PART_TopLeftResizeBorder") as Rectangle;
        _topLeftResizeBorder.MouseDown += _topLeftResizeBorder_MouseDown;
        _topLeftResizeBorder.MouseMove += _topLeftResizeBorder_MouseMove;
        _topLeftResizeBorder.MouseUp += _topLeftResizeBorder_MouseUp;

        _bottomLeftResizeBorder = GetTemplateChild("PART_BottomLeftResizeBorder") as Rectangle;
        _bottomLeftResizeBorder.MouseDown += _bottomLeftResizeBorder_MouseDown;
        _bottomLeftResizeBorder.MouseMove += _bottomLeftResizeBorder_MouseMove;
        _bottomLeftResizeBorder.MouseUp += _bottomLeftResizeBorder_MouseUp;

        _bottomRightResizeBorder = GetTemplateChild("PART_BottomRightResizeBorder") as Rectangle;
        _bottomRightResizeBorder.MouseDown += _bottomRightResizeBorder_MouseDown;
        _bottomRightResizeBorder.MouseMove += _bottomRightResizeBorder_MouseMove;
        _bottomRightResizeBorder.MouseUp += _bottomRightResizeBorder_MouseUp;

        // BAR BUTTONS
        var maximizeBtn = GetTemplateChild("PART_MaximizeBtn") as StswSubButton;
        maximizeBtn.Click += (_, _) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        var closeBtn = GetTemplateChild("PART_CloseBtn") as StswSubButton;
        closeBtn.Click += CloseBtn_Click;

        if (_barPanel != null)
        {
            _barPanel.MouseMove += _barPanel_MouseMove;
            _barPanel.MouseDown += _barPanel_MouseDown;
        }

        PreviewMouseDown += StswWindowPanelItem_MouseDown;
    }

    #region Dependency Properties
    public WindowState WindowState
    {
        get { return (WindowState)GetValue(WindowStateProperty); }
        set { SetValue(WindowStateProperty, value); }
    }
    public static readonly DependencyProperty WindowStateProperty =
        DependencyProperty.Register(
            nameof(WindowState),
            typeof(WindowState),
            typeof(StswWindowPanelItem),
            new FrameworkPropertyMetadata(
                WindowState.Normal,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnWindowStateChanged));

    private static void OnWindowStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StswWindowPanelItem window)
        {
            if ((WindowState)e.OldValue == WindowState.Normal)
            {
                window._normalWidth = window.Width;
                window._normalHeight = window.Height;
                window._normalLeft = Canvas.GetLeft(window);
                window._normalTop = Canvas.GetTop(window);
            }

            var canvas = VisualTreeHelper.GetParent(window) as FrameworkElement;
            if (window.WindowState == WindowState.Maximized)
            {
                window.Width = canvas.ActualWidth;
                window.Height = canvas.ActualHeight;
                Canvas.SetLeft(window, 0);
                Canvas.SetTop(window, 0);
            }
            else if (window.WindowState == WindowState.Normal)
            {
                window.Width = window._normalWidth;
                window.Height = window._normalHeight;
                Canvas.SetLeft(window, window._normalLeft);
                Canvas.SetTop(window, window._normalTop);
            }
        }
    }

    #endregion

    #region Routed Events
    public static readonly RoutedEvent BarMouseMoveEvent =
            EventManager.RegisterRoutedEvent(
                nameof(BarMouseMoveEvent),
                RoutingStrategy.Bubble,
                typeof(MouseEventHandler),
                typeof(StswWindowPanelItem));
    public event RoutedEventHandler BarMouseMoveEventHandler
    {
        add => AddHandler(BarMouseMoveEvent, value);
        remove => RemoveHandler(BarMouseMoveEvent, value);
    }

    public static readonly RoutedEvent BarMouseDoubleClickEvent =
            EventManager.RegisterRoutedEvent(
                nameof(BarMouseDoubleClickEvent),
                RoutingStrategy.Bubble,
                typeof(MouseButtonEventHandler),
                typeof(StswWindowPanelItem));
    public event RoutedEventHandler BarMouseDoubleClickEventHandler
    {
        add => AddHandler(BarMouseDoubleClickEvent, value);
        remove => RemoveHandler(BarMouseDoubleClickEvent, value);
    }

    public static readonly RoutedEvent WindowMouseClickEvent =
           EventManager.RegisterRoutedEvent(
               nameof(WindowMouseClickEvent),
               RoutingStrategy.Bubble,
               typeof(MouseButtonEventHandler),
               typeof(StswWindowPanelItem));
    public event RoutedEventHandler WindowMouseClickEventHandler
    {
        add => AddHandler(WindowMouseClickEvent, value);
        remove => RemoveHandler(WindowMouseClickEvent, value);
    }

    public static readonly RoutedEvent CloseBtnClickedEvent =
           EventManager.RegisterRoutedEvent(
               nameof(CloseBtnClickedEvent),
               RoutingStrategy.Bubble,
               typeof(RoutedEventHandler),
               typeof(StswWindowPanelItem));
    public event RoutedEventHandler CloseBtnClickedEventHandler
    {
        add => AddHandler(CloseBtnClickedEvent, value);
        remove => RemoveHandler(CloseBtnClickedEvent, value);
    }
    #endregion

    #region Events
    private void _barPanel_MouseMove(object sender, MouseEventArgs e)
    {
        var onBarPosition = e.GetPosition(this);
        var args = new BarMouseEventArgs(e.MouseDevice, e.Timestamp, onBarPosition)
        {
            RoutedEvent = BarMouseMoveEvent,
            Source = this
        };
        RaiseEvent(args);
    }

    private void _barPanel_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left
            && e.ClickCount == 2)
        {
            var args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
            {
                RoutedEvent = BarMouseDoubleClickEvent,
                Source = this
            };
            RaiseEvent(args);
        }
    }

    private void StswWindowPanelItem_MouseDown(object sender, MouseButtonEventArgs e)
    {
        var args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
        {
            RoutedEvent = WindowMouseClickEvent,
            Source = this
        };
        RaiseEvent(args);
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e)
    {
        var args = new RoutedEventArgs(CloseBtnClickedEvent, this);
        RaiseEvent(args);
    }
    #endregion

    #region ResizeEvents
    // RIGHT
    private void _rightResizeBorder_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isRightDragging && _rightResizeBorder != null)
        {
            _isRightDragging = false;
            _rightResizeBorder.ReleaseMouseCapture();
        }
    }

    private void _rightResizeBorder_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isRightDragging && _rightResizeBorder != null)
        {
            Point currentPoint = e.GetPosition(this);
            double offsetX = currentPoint.X - _startPoint.X;
            var newWidth = _startWidth + offsetX;

            if (newWidth < MinWidth)
            {
                newWidth = MinWidth;
            }

            Width = newWidth;
        }
    }

    private void _rightResizeBorder_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            _isRightDragging = true;
            _startWidth = Width;
            _startPoint = e.GetPosition(this);
            _rightResizeBorder.CaptureMouse();
        }
    }
    // LEFT

    private void _leftResizeBorder_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isLeftDragging && _leftResizeBorder != null)
        {
            _isLeftDragging = false;
            _leftResizeBorder.ReleaseMouseCapture();
        }
    }

    private void _leftResizeBorder_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isLeftDragging && _leftResizeBorder != null)
        {
            Point currentPoint = e.GetPosition(this.Parent as FrameworkElement);
            double offsetX = _startPoint.X - currentPoint.X;
            var newWidth = _startWidth + offsetX;

            if (newWidth < MinWidth)
            {
                newWidth = MinWidth;
                offsetX = newWidth - _startWidth;
            }

            Canvas.SetLeft(this, _startCanvasLeft - offsetX);
            Width = newWidth;
        }
    }

    private void _leftResizeBorder_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            _isLeftDragging = true;
            _startWidth = Width;
            _startCanvasLeft = Canvas.GetLeft(this);
            _startPoint = e.GetPosition(this.Parent as FrameworkElement);
            _leftResizeBorder.CaptureMouse();
        }
    }
    // TOP

    private void _topResizeBorder_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isTopDragging && _topResizeBorder != null)
        {
            _isTopDragging = false;
            _topResizeBorder.ReleaseMouseCapture();
        }
    }

    private void _topResizeBorder_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isTopDragging && _topResizeBorder != null)
        {
            Point currentPoint = e.GetPosition(this.Parent as FrameworkElement);
            double offsetY = _startPoint.Y - currentPoint.Y;
            var newHeight = _startHeight + offsetY;

            if (newHeight < MinHeight)
            {
                newHeight = MinHeight;
                offsetY = newHeight - _startHeight;
            }

            Canvas.SetTop(this, _startCanvasTop - offsetY);
            Height = newHeight;
        }
    }

    private void _topResizeBorder_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _isTopDragging = true;
        _startHeight = Height;
        _startCanvasTop = Canvas.GetTop(this);
        _startPoint = e.GetPosition(this.Parent as FrameworkElement);
        _topResizeBorder.CaptureMouse();
    }
    // BOTTOM

    private void _bottomResizeBorder_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isBottomDragging && _bottomResizeBorder != null)
        {
            _isBottomDragging = false;
            _bottomResizeBorder.ReleaseMouseCapture();
        }
    }

    private void _bottomResizeBorder_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isBottomDragging && _bottomResizeBorder != null)
        {
            Point currentPoint = e.GetPosition(this.Parent as FrameworkElement);
            double offsetY = currentPoint.Y - _startPoint.Y;
            var newHeight = _startHeight + offsetY;

            if (newHeight < MinHeight)
                newHeight = MinHeight;

            Height = newHeight;
        }
    }

    private void _bottomResizeBorder_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _isBottomDragging = true;
        _startHeight = Height;
        _startPoint = e.GetPosition(this.Parent as FrameworkElement);
        _bottomResizeBorder.CaptureMouse();
    }

    // TOP-RIGHT
    private void _topRightResizeBorder_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isTopRightDragging && _topRightResizeBorder != null)
        {
            _isTopRightDragging = false;
            _topRightResizeBorder.ReleaseMouseCapture();
        }
    }

    private void _topRightResizeBorder_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isTopRightDragging && _topRightResizeBorder != null)
        {
            Point currentPoint = e.GetPosition(this.Parent as FrameworkElement);
            double offsetY = _startPoint.Y - currentPoint.Y;
            double offsetX = currentPoint.X - _startPoint.X;
            var newHeight = _startHeight + offsetY;
            var newWidth = _startWidth + offsetX;

            if (newHeight < MinHeight)
            {
                newHeight = MinHeight;
                offsetY = newHeight - _startHeight;
            }
            if (newWidth < MinWidth)
            {
                newWidth = MinWidth;
            }

            Canvas.SetTop(this, _startCanvasTop - offsetY);
            Height = newHeight;
            Width = newWidth;
        }
    }

    private void _topRightResizeBorder_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _isTopRightDragging = true;
        _startHeight = Height;
        _startWidth = Width;
        _startCanvasTop = Canvas.GetTop(this);
        _startCanvasLeft = Canvas.GetLeft(this);
        _startPoint = e.GetPosition(this.Parent as FrameworkElement);
        _topRightResizeBorder.CaptureMouse();
    }

    // TOP-LEFT
    private void _topLeftResizeBorder_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isTopLeftDragging && _topLeftResizeBorder != null)
        {
            _isTopLeftDragging = false;
            _topLeftResizeBorder.ReleaseMouseCapture();
        }
    }

    private void _topLeftResizeBorder_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isTopLeftDragging && _topLeftResizeBorder != null)
        {
            Point currentPoint = e.GetPosition(this.Parent as FrameworkElement);
            double offsetY = _startPoint.Y - currentPoint.Y;
            double offsetX = _startPoint.X - currentPoint.X;
            var newHeight = _startHeight + offsetY;
            var newWidth = _startWidth + offsetX;

            if (newHeight < MinHeight)
            {
                newHeight = MinHeight;
                offsetY = newHeight - _startHeight;
            }
            if (newWidth < MinWidth)
            {
                newWidth = MinWidth;
                offsetX = newWidth - _startWidth;
            }

            Canvas.SetTop(this, _startCanvasTop - offsetY);
            Height = newHeight;
            Canvas.SetLeft(this, _startCanvasLeft - offsetX);
            Width = newWidth;
        }
    }

    private void _topLeftResizeBorder_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _isTopLeftDragging = true;
        _startHeight = Height;
        _startWidth = Width;
        _startCanvasTop = Canvas.GetTop(this);
        _startCanvasLeft = Canvas.GetLeft(this);
        _startPoint = e.GetPosition(this.Parent as FrameworkElement);
        _topLeftResizeBorder.CaptureMouse();
    }

    // BOTTOM-LEFT
    private void _bottomLeftResizeBorder_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isBottomLeftDragging && _bottomLeftResizeBorder != null)
        {
            _isBottomLeftDragging = false;
            _bottomLeftResizeBorder.ReleaseMouseCapture();
        }
    }

    private void _bottomLeftResizeBorder_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isBottomLeftDragging && _bottomLeftResizeBorder != null)
        {
            Point currentPoint = e.GetPosition(this.Parent as FrameworkElement);
            double offsetY = currentPoint.Y - _startPoint.Y;
            double offsetX = _startPoint.X - currentPoint.X;
            var newHeight = _startHeight + offsetY;
            var newWidth = _startWidth + offsetX;

            if (newHeight < MinHeight)
                newHeight = MinHeight;

            if (newWidth < MinWidth)
            {
                newWidth = MinWidth;
                offsetX = newWidth - _startWidth;
            }

            Height = newHeight;
            Canvas.SetLeft(this, _startCanvasLeft - offsetX);
            Width = newWidth;
        }
    }

    private void _bottomLeftResizeBorder_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _isBottomLeftDragging = true;
        _startHeight = Height;
        _startWidth = Width;
        _startCanvasTop = Canvas.GetTop(this);
        _startCanvasLeft = Canvas.GetLeft(this);
        _startPoint = e.GetPosition(this.Parent as FrameworkElement);
        _bottomLeftResizeBorder.CaptureMouse();
    }

    // BOTTOM-RIGHT
    private void _bottomRightResizeBorder_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isBottomRightDragging && _bottomRightResizeBorder != null)
        {
            _isBottomRightDragging = false;
            _bottomRightResizeBorder.ReleaseMouseCapture();
        }
    }

    private void _bottomRightResizeBorder_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isBottomRightDragging && _bottomRightResizeBorder != null)
        {
            Point currentPoint = e.GetPosition(this.Parent as FrameworkElement);
            double offsetY = currentPoint.Y - _startPoint.Y;
            double offsetX = currentPoint.X - _startPoint.X;
            var newHeight = _startHeight + offsetY;
            var newWidth = _startWidth + offsetX;

            if (newHeight < MinHeight)
                newHeight = MinHeight;

            if (newWidth < MinWidth)
                newWidth = MinWidth;

            Height = newHeight;
            Width = newWidth;
        }
    }

    private void _bottomRightResizeBorder_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _isBottomRightDragging = true;
        _startHeight = Height;
        _startWidth = Width;
        _startCanvasTop = Canvas.GetTop(this);
        _startCanvasLeft = Canvas.GetLeft(this);
        _startPoint = e.GetPosition(this.Parent as FrameworkElement);
        _bottomRightResizeBorder.CaptureMouse();
    }
    #endregion
}

public class BarMouseEventArgs : MouseEventArgs
{
    public Point OnBarPosition { get; }

    public BarMouseEventArgs(MouseDevice mouseDevice, int timestamp, Point onBarPosition)
        : base(mouseDevice, timestamp)
    {
        OnBarPosition = onBarPosition;
    }
}

public static class EnumerableExtensions
{
    public static int IndexOf(this IEnumerable collection, object item)
    {
        int index = 0;
        foreach (var element in collection)
        {
            if (Equals(element, item)) // Use Equals to compare objects
                return index;
            index++;
        }
        return -1; // Not found
    }
}
