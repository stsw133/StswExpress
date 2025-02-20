using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace StswExpress;
/// <summary>
/// A <see cref="ScrollViewer"/> extension with additional directional buttons for scrolling.
/// Supports automatic scrolling when hovering over navigation buttons.
/// This control provides extra navigation buttons for controlling scroll behavior in both horizontal and vertical directions.
/// </summary>
public class StswDirectionView : ScrollViewer
{
    static StswDirectionView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDirectionView), new FrameworkPropertyMetadata(typeof(StswDirectionView)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswDirectionView), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    private ButtonBase? _btnDown, _btnLeft, _btnRight, _btnUp;
    private bool _isLeftMouseDown;
    private DispatcherTimer? _autoScrollTimer;
    private Action? _currentScrollAction;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Button: down
        if (GetTemplateChild("PART_ButtonDown") is ButtonBase btnDown)
        {
            btnDown.Click += (_, _) => ScrollInfo.MouseWheelDown();
            btnDown.MouseEnter += HandleMouseEnterOnButton;
            btnDown.MouseLeave += HandleMouseLeaveFromButton;
            _btnDown = btnDown;
        }
        /// Button: left
        if (GetTemplateChild("PART_ButtonLeft") is ButtonBase btnLeft)
        {
            btnLeft.Click += (_, _) => ScrollInfo.MouseWheelLeft();
            btnLeft.MouseEnter += HandleMouseEnterOnButton;
            btnLeft.MouseLeave += HandleMouseLeaveFromButton;
            _btnLeft = btnLeft;
        }
        /// Button: right
        if (GetTemplateChild("PART_ButtonRight") is ButtonBase btnRight)
        {
            btnRight.Click += (_, _) => ScrollInfo.MouseWheelRight();
            btnRight.MouseEnter += HandleMouseEnterOnButton;
            btnRight.MouseLeave += HandleMouseLeaveFromButton;
            _btnRight = btnRight;
        }
        /// Button: up
        if (GetTemplateChild("PART_ButtonUp") is ButtonBase btnUp)
        {
            btnUp.Click += (_, _) => ScrollInfo.MouseWheelUp();
            btnUp.MouseEnter += HandleMouseEnterOnButton;
            btnUp.MouseLeave += HandleMouseLeaveFromButton;
            _btnUp = btnUp;
        }

        OnHorizontalOffsetChanged();
        OnVerticalOffsetChanged();
    }

    /// <summary>
    /// Detects when the left mouse button is pressed and sets a flag to start auto-scrolling when hovering over directional buttons.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);
        _isLeftMouseDown = true;
    }

    /// <summary>
    /// Detects when the left mouse button is released, stopping any active auto-scrolling.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);
        _isLeftMouseDown = false;
        StopAutoScrollTimer();
    }

    /// <summary>
    /// Handles scrolling behavior using the mouse wheel.
    /// Supports both vertical and horizontal scrolling, and raises events for parent elements when the scroll reaches the top or bottom.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        /// horizontal scrolling
        if (ComputedHorizontalScrollBarVisibility == Visibility.Visible
        && (ComputedVerticalScrollBarVisibility != Visibility.Visible || Keyboard.Modifiers == ModifierKeys.Shift))
        {
            if (e.Delta > 0)
                ScrollInfo.MouseWheelLeft();
            else
                ScrollInfo.MouseWheelRight();

            e.Handled = true;
        }

        base.OnMouseWheel(e);

        /// scrolling scroll in another scroll
        //if (Parent is UIElement parentElement)
        //{
        //    if ((e.Delta > 0 && VerticalOffset == 0) || (e.Delta < 0 && VerticalOffset == ScrollableHeight))
        //    {
        //        e.Handled = true;
        //
        //        var routedArgs = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        //        {
        //            RoutedEvent = MouseWheelEvent
        //        };
        //        parentElement.RaiseEvent(routedArgs);
        //    }
        //}
    }

    /// <summary>
    /// Handles the ScrollChanged event, enabling or disabling the directional buttons based on the current scroll offsets.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnScrollChanged(ScrollChangedEventArgs e)
    {
        if (e.HorizontalChange != 0)
        {
            if (_btnLeft != null)
                _btnLeft.IsEnabled = e.HorizontalOffset > 0;
            if (_btnRight != null)
                _btnRight.IsEnabled = e.HorizontalOffset + e.ViewportWidth < e.ExtentWidth;
        }
        if (e.VerticalChange != 0)
        {
            if (_btnUp != null)
                _btnUp.IsEnabled = e.VerticalOffset > 0;
            if (_btnDown != null)
                _btnDown.IsEnabled = e.VerticalOffset + e.ViewportHeight < e.ExtentHeight;
        }
    }

    /// <summary>
    /// Handles changes in horizontal offset.
    /// Enables or disables the left and right directional buttons based on horizontal scroll position.
    /// </summary>
    private void OnHorizontalOffsetChanged()
    {
        if (_btnLeft == null || _btnRight == null)
            return;

        if (HorizontalOffset == 0)
        {
            _btnLeft.IsEnabled = false;
        }
        else if (HorizontalOffset >= ScrollableWidth)
        {
            _btnRight.IsEnabled = false;
        }
        else
        {
            _btnLeft.IsEnabled = true;
            _btnRight.IsEnabled = true;
        }
    }

    /// <summary>
    /// Handles changes in vertical offset.
    /// Enables or disables the up and down directional buttons based on vertical scroll position.
    /// </summary>
    private void OnVerticalOffsetChanged()
    {
        if (_btnUp == null || _btnDown == null)
            return;

        if (VerticalOffset == 0)
        {
            _btnUp.IsEnabled = false;
        }
        else if (VerticalOffset >= ScrollableWidth)
        {
            _btnDown.IsEnabled = false;
        }
        else
        {
            _btnUp.IsEnabled = true;
            _btnDown.IsEnabled = true;
        }
    }

    /// <summary>
    /// Starts auto-scrolling when the mouse enters a directional button and the left mouse button is held down.
    /// </summary>
    /// <param name="sender">The button which raised the event.</param>
    /// <param name="e">The mouse event arguments.</param>
    private void HandleMouseEnterOnButton(object sender, MouseEventArgs e)
    {
        if (_isLeftMouseDown)
        {
            if (sender == _btnDown)
                _currentScrollAction = ScrollInfo.MouseWheelDown;
            else if (sender == _btnUp)
                _currentScrollAction = ScrollInfo.MouseWheelUp;
            else if (sender == _btnLeft)
                _currentScrollAction = ScrollInfo.MouseWheelLeft;
            else if (sender == _btnRight)
                _currentScrollAction = ScrollInfo.MouseWheelRight;

            var interval = TimeSpan.FromMilliseconds(200);
            if (sender is RepeatButton btn)
                interval = TimeSpan.FromMilliseconds(btn.Interval);
            
            StartAutoScrollTimer(interval);
        }
    }

    /// <summary>
    /// Stops any active auto-scrolling when the mouse leaves a directional button.
    /// </summary>
    /// <param name="sender">The button which raised the event.</param>
    /// <param name="e">The mouse event arguments.</param>
    private void HandleMouseLeaveFromButton(object sender, MouseEventArgs e)
    {
        StopAutoScrollTimer();
    }

    /// <summary>
    /// Starts a timer to repeatedly invoke the current scroll action using the specified interval.
    /// The action will be invoked at regular intervals while the mouse is over the button and the left mouse button is pressed.
    /// </summary>
    /// <param name="interval">The interval to use for auto-scrolling.</param>
    private void StartAutoScrollTimer(TimeSpan interval)
    {
        if (_autoScrollTimer != null)
            return;

        _autoScrollTimer = new DispatcherTimer { Interval = interval };
        _autoScrollTimer.Tick += AutoScrollTimer_Tick;
        _currentScrollAction?.Invoke();
        _autoScrollTimer.Start();
    }

    /// <summary>
    /// Timer tick event handler that repeatedly calls the scroll action while the left mouse button is pressed.
    /// Stops the timer if the left mouse button is released.
    /// </summary>
    /// <param name="sender">The timer instance.</param>
    /// <param name="e">The event arguments.</param>
    private void AutoScrollTimer_Tick(object? sender, EventArgs e)
    {
        if (_isLeftMouseDown)
            _currentScrollAction?.Invoke();
        else
            StopAutoScrollTimer();
    }

    /// <summary>
    /// Stops the auto-scroll timer and resets the current scroll action.
    /// </summary>
    private void StopAutoScrollTimer()
    {
        if (_autoScrollTimer != null)
        {
            _autoScrollTimer.Stop();
            _autoScrollTimer.Tick -= AutoScrollTimer_Tick;
            _autoScrollTimer = null;
        }
        _currentScrollAction = null;
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the orientation of the control (horizontal or vertical).
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    public static readonly DependencyProperty OrientationProperty
        = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(StswDirectionView),
            new FrameworkPropertyMetadata(default(Orientation), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the thickness of the back (up and left) buttons.
    /// </summary>
    public Thickness BBtnThickness
    {
        get => (Thickness)GetValue(BBtnThicknessProperty);
        set => SetValue(BBtnThicknessProperty, value);
    }
    public static readonly DependencyProperty BBtnThicknessProperty
        = DependencyProperty.Register(
            nameof(BBtnThickness),
            typeof(Thickness),
            typeof(StswDirectionView),
            new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the thickness of the forward (down and right) buttons.
    /// </summary>
    public Thickness FBtnThickness
    {
        get => (Thickness)GetValue(FBtnThicknessProperty);
        set => SetValue(FBtnThicknessProperty, value);
    }
    public static readonly DependencyProperty FBtnThicknessProperty
        = DependencyProperty.Register(
            nameof(FBtnThickness),
            typeof(Thickness),
            typeof(StswDirectionView),
            new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion

    #region Excluded properties
    /// The following properties are hidden from the designer and serialization:

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Brush? BorderBrush { get; private set; }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Thickness? BorderThicknessProperty { get; private set; }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new HorizontalAlignment HorizontalContentAlignment { get; private set; }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new VerticalAlignment VerticalContentAlignment { get; private set; }
    #endregion
}

/* usage:

<se:StswDirectionView Orientation="Horizontal">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="Item 1"/>
        <TextBlock Text="Item 2"/>
        <TextBlock Text="Item 3"/>
    </StackPanel>
</se:StswDirectionView>

*/
