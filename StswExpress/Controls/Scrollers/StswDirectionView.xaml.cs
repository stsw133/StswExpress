using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace StswExpress;
/// <summary>
/// A <see cref="ScrollViewer"/> extension with additional directional buttons for scrolling.
/// Supports automatic scrolling when hovering over navigation buttons.
/// This control provides extra navigation buttons for controlling scroll behavior in both horizontal and vertical directions.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswDirectionView Orientation="Horizontal"&gt;
///     &lt;StackPanel Orientation="Horizontal"&gt;
///         &lt;TextBlock Text="Item 1"/&gt;
///         &lt;TextBlock Text="Item 2"/&gt;
///         &lt;TextBlock Text="Item 3"/&gt;
///     &lt;/StackPanel&gt;
/// &lt;/se:StswDirectionView&gt;
/// </code>
/// </example>
public class StswDirectionView : ScrollViewer
{
    private ButtonBase? _btnDown, _btnLeft, _btnRight, _btnUp;
    private DispatcherTimer? _autoScrollTimer;
    private Action? _currentScrollAction;
    private bool _isLeftMouseDown;
    private readonly Dictionary<ButtonBase, double> _expandedLengths = [];
    private bool _templateApplied;

    static StswDirectionView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDirectionView), new FrameworkPropertyMetadata(typeof(StswDirectionView)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        DetachButtonEvents();
        base.OnApplyTemplate();

        _expandedLengths.Clear();
        _templateApplied = false;

        /// Button: down
        if (GetTemplateChild("PART_ButtonDown") is ButtonBase btnDown)
        {
            btnDown.Click += (_, _) => ScrollInfo.MouseWheelDown();
            btnDown.MouseEnter += HandleMouseEnterOnButton;
            btnDown.MouseLeave += HandleMouseLeaveFromButton;
            btnDown.IsVisibleChanged += Button_IsVisibleChanged;
            _btnDown = btnDown;
        }
        /// Button: left
        if (GetTemplateChild("PART_ButtonLeft") is ButtonBase btnLeft)
        {
            btnLeft.Click += (_, _) => ScrollInfo.MouseWheelLeft();
            btnLeft.MouseEnter += HandleMouseEnterOnButton;
            btnLeft.MouseLeave += HandleMouseLeaveFromButton;
            btnLeft.IsVisibleChanged += Button_IsVisibleChanged;
            _btnLeft = btnLeft;
        }
        /// Button: right
        if (GetTemplateChild("PART_ButtonRight") is ButtonBase btnRight)
        {
            btnRight.Click += (_, _) => ScrollInfo.MouseWheelRight();
            btnRight.MouseEnter += HandleMouseEnterOnButton;
            btnRight.MouseLeave += HandleMouseLeaveFromButton;
            btnRight.IsVisibleChanged += Button_IsVisibleChanged;
            _btnRight = btnRight;
        }
        /// Button: up
        if (GetTemplateChild("PART_ButtonUp") is ButtonBase btnUp)
        {
            btnUp.Click += (_, _) => ScrollInfo.MouseWheelUp();
            btnUp.MouseEnter += HandleMouseEnterOnButton;
            btnUp.MouseLeave += HandleMouseLeaveFromButton;
            btnUp.IsVisibleChanged += Button_IsVisibleChanged;
            _btnUp = btnUp;
        }

        _templateApplied = true;
        ApplyDynamicMode();

        OnHorizontalOffsetChanged();
        OnVerticalOffsetChanged();

        Dispatcher.BeginInvoke(new Action(() =>
        {
            CaptureButtonSizes();
            ApplyDynamicMode();
        }), DispatcherPriority.Loaded);
    }

    /// <inheritdoc/>
    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);
        _isLeftMouseDown = true;
    }

    /// <inheritdoc/>
    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);
        _isLeftMouseDown = false;
        StopAutoScrollTimer();
    }

    /// <inheritdoc/>
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);

        if (DynamicMode == StswDynamicVisibilityMode.Full)
            HideFullButtons();
    }

    /// <inheritdoc/>
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (DynamicMode == StswDynamicVisibilityMode.Full)
            UpdateFullModeButtons(e.GetPosition(this));
    }

    /// <inheritdoc/>
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        /// horizontal scrolling
        if ((ComputedHorizontalScrollBarVisibility == Visibility.Visible)
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

    /// <inheritdoc/>
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
        if (sender is ButtonBase button)
            OnButtonMouseEnter(button);

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
        if (sender is ButtonBase button)
            OnButtonMouseLeave(button);

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
    ///
    /// </summary>
    public StswDynamicVisibilityMode DynamicMode
    {
        get => (StswDynamicVisibilityMode)GetValue(DynamicModeProperty);
        set => SetValue(DynamicModeProperty, value);
    }
    public static readonly DependencyProperty DynamicModeProperty
        = DependencyProperty.Register(
            nameof(DynamicMode),
            typeof(StswDynamicVisibilityMode),
            typeof(StswDirectionView),
            new FrameworkPropertyMetadata(default(StswDynamicVisibilityMode), OnDynamicModeChanged)
        );
    private static void OnDynamicModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswDirectionView stsw)
            return;

        stsw.ApplyDynamicMode();
    }

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

    #region Animations
    private const double PartialCollapsedRatio = 0.55;
    private const double PartialCollapsedMinimum = 16d;
    private const double RevealProximity = 32d;
    private static readonly TimeSpan LengthAnimationDuration = TimeSpan.FromMilliseconds(250);
    private static readonly TimeSpan OpacityAnimationDuration = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// Applies the current dynamic visibility mode to all directional buttons.
    /// </summary>
    private void ApplyDynamicMode()
    {
        if (!_templateApplied)
            return;

        foreach (var button in EnumerateButtons())
        {
            button.BeginAnimation(OpacityProperty, null);

            if (!button.IsVisible)
                continue;

            switch (DynamicMode)
            {
                case StswDynamicVisibilityMode.Off:
                    SetLength(button, null);
                    button.IsHitTestVisible = true;
                    button.Opacity = 1;
                    break;
                case StswDynamicVisibilityMode.Partial:
                    var expanded = EnsureExpandedLength(button);
                    var collapsed = GetPartialLength(expanded);
                    SetLength(button, collapsed);
                    button.IsHitTestVisible = true;
                    button.Opacity = 1;
                    break;
                case StswDynamicVisibilityMode.Full:
                case StswDynamicVisibilityMode.Collapsed:
                    SetLength(button, 0);
                    button.IsHitTestVisible = false;
                    button.Opacity = 0;
                    break;
            }
        }

        if (DynamicMode == StswDynamicVisibilityMode.Full && IsMouseOver)
            UpdateFullModeButtons(Mouse.GetPosition(this));
    }

    /// <summary>
    /// Captures the original sizes of all directional buttons for use in animations.
    /// </summary>
    private void CaptureButtonSizes()
    {
        foreach (var button in EnumerateButtons())
        {
            if (!button.IsVisible)
                continue;

            var length = MeasureButtonLength(button);
            if (length > 0)
                _expandedLengths[button] = length;
        }
    }

    /// <summary>
    /// Handles mouse enter events on directional buttons.
    /// </summary>
    /// <param name="button">The button which raised the event.</param>
    private void OnButtonMouseEnter(ButtonBase button)
    {
        if (!button.IsVisible)
            return;

        switch (DynamicMode)
        {
            case StswDynamicVisibilityMode.Partial:
                var expanded = EnsureExpandedLength(button);
                if (expanded <= 0 || double.IsNaN(expanded))
                    expanded = PartialCollapsedMinimum;
                AnimateLength(button, expanded);
                break;
            case StswDynamicVisibilityMode.Full:
                ShowFullButton(button);
                break;
        }
    }

    /// <summary>
    /// Handles mouse leave events from directional buttons.
    /// </summary>
    /// <param name="button">The button which raised the event.</param>
    private void OnButtonMouseLeave(ButtonBase button)
    {
        if (!button.IsVisible)
            return;

        switch (DynamicMode)
        {
            case StswDynamicVisibilityMode.Partial:
                var expanded = EnsureExpandedLength(button);
                var collapsed = GetPartialLength(expanded);
                AnimateLength(button, collapsed);
                break;
            case StswDynamicVisibilityMode.Full:
                HideFullButton(button);
                break;
        }
    }

    /// <summary>
    /// Updates the visibility and size of directional buttons based on mouse position when in full dynamic mode.
    /// </summary>
    /// <param name="position">The current mouse position relative to the control.</param>
    private void UpdateFullModeButtons(Point position)
    {
        foreach (var button in EnumerateButtons())
        {
            if (!button.IsVisible)
                continue;

            if (ShouldRevealButton(button, position) || button.IsMouseOver)
                ShowFullButton(button);
            else
                HideFullButton(button);
        }
    }

    /// <summary>
    /// Hides all directional buttons when in full dynamic mode.
    /// </summary>
    private void HideFullButtons()
    {
        foreach (var button in EnumerateButtons())
        {
            if (!button.IsVisible)
                continue;

            HideFullButton(button);
        }
    }

    /// <summary>
    /// Shows a directional button in full size with animation.
    /// </summary>
    /// <param name="button">The button to show.</param>
    private void ShowFullButton(ButtonBase button)
    {
        var expanded = EnsureExpandedLength(button);
        if (expanded <= 0 || double.IsNaN(expanded))
            expanded = RevealProximity;

        AnimateLength(button, expanded);
        AnimateOpacity(button, 1);
        button.IsHitTestVisible = true;
    }

    /// <summary>
    /// Hides a directional button with animation, unless the mouse is currently over it.
    /// </summary>
    /// <param name="button">The button to hide.</param>
    private void HideFullButton(ButtonBase button)
    {
        if (button.IsMouseOver)
            return;

        AnimateLength(button, 0);
        AnimateOpacity(button, 0);
        button.IsHitTestVisible = false;
    }

    /// <summary>
    /// Determines if a directional button should be revealed based on the current mouse position.
    /// </summary>
    /// <param name="button">The button to check.</param>
    /// <param name="position">The current mouse position relative to the control.</param>
    /// <returns><see langword="true"/> if the button should be revealed; otherwise, <see langword="false"/>.</returns>
    private bool ShouldRevealButton(ButtonBase button, Point position)
    {
        var threshold = GetRevealThreshold(button);

        if (ReferenceEquals(button, _btnLeft))
            return position.X <= threshold;
        if (ReferenceEquals(button, _btnRight))
            return ActualWidth - position.X <= threshold;
        if (ReferenceEquals(button, _btnUp))
            return position.Y <= threshold;
        if (ReferenceEquals(button, _btnDown))
            return ActualHeight - position.Y <= threshold;

        return false;
    }

    /// <summary>
    /// Calculates the reveal threshold for a directional button based on its expanded size and a minimum proximity value.
    /// </summary>
    /// <param name="button">The button to calculate the threshold for.</param>
    /// <returns>The reveal threshold distance.</returns>
    private double GetRevealThreshold(ButtonBase button)
    {
        var expanded = EnsureExpandedLength(button);
        if (expanded <= 0 || double.IsNaN(expanded))
            expanded = RevealProximity;

        return Math.Max(RevealProximity, expanded);
    }

    /// <summary>
    /// Ensures that the expanded length of a button is recorded.
    /// </summary>
    /// <param name="button">The button to measure.</param>
    /// <returns>The expanded length of the button.</returns>
    private double EnsureExpandedLength(ButtonBase button)
    {
        if (!_expandedLengths.TryGetValue(button, out var value) || value <= 0)
        {
            value = MeasureButtonLength(button);
            if (value > 0)
                _expandedLengths[button] = value;
        }

        return value;
    }

    /// <summary>
    /// Measures the actual length (width or height) of a button.
    /// </summary>
    /// <param name="button">The button to measure.</param>
    /// <returns>The measured length of the button.</returns>
    private double MeasureButtonLength(ButtonBase button)
    {
        var property = GetLengthProperty(button);
        var length = property == WidthProperty ? button.ActualWidth : button.ActualHeight;

        if (length <= 0 || double.IsNaN(length))
        {
            button.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var desired = button.DesiredSize;
            length = property == WidthProperty ? desired.Width : desired.Height;
        }

        return length;
    }

    /// <summary>
    /// Calculates the collapsed length for partial dynamic mode based on the expanded length.
    /// </summary>
    /// <param name="expanded">The expanded length of the button.</param>
    /// <returns>The calculated collapsed length.</returns>
    private static double GetPartialLength(double expanded)
    {
        if (double.IsNaN(expanded) || expanded <= 0)
            return PartialCollapsedMinimum;

        var collapsed = Math.Max(PartialCollapsedMinimum, expanded * PartialCollapsedRatio);
        return Math.Min(expanded, collapsed);
    }

    /// <summary>
    /// Gets the appropriate length property (Width or Height) for a given button based on its orientation.
    /// </summary>
    /// <param name="button">The button to evaluate.</param>
    /// <returns>The corresponding length dependency property.</returns>
    private DependencyProperty GetLengthProperty(ButtonBase button)
        => ReferenceEquals(button, _btnLeft) || ReferenceEquals(button, _btnRight)
            ? WidthProperty
            : HeightProperty;

    /// <summary>
    /// Sets the length (width or height) of a button directly, cancelling any ongoing animations.
    /// </summary>
    /// <param name="button">The button to modify.</param>
    /// <param name="value">The new length value, or <see langword="null"/> to clear the value.</param>
    private void SetLength(ButtonBase button, double? value)
    {
        var property = GetLengthProperty(button);
        button.BeginAnimation(property, null);

        if (value.HasValue)
        {
            if (property == WidthProperty)
                button.Width = value.Value;
            else
                button.Height = value.Value;
        }
        else
        {
            button.ClearValue(property);
        }
    }

    /// <summary>
    /// Animates the length (width or height) of a button to a specified value.
    /// </summary>
    /// <param name="button">The button to animate.</param>
    /// <param name="value">The target length value.</param>
    private void AnimateLength(ButtonBase button, double value)
    {
        var property = GetLengthProperty(button);

        if (!StswSettings.Default.EnableAnimations)
        {
            button.BeginAnimation(property, null);
            if (property == WidthProperty)
                button.Width = value;
            else
                button.Height = value;
            return;
        }

        var animation = new DoubleAnimation(value, LengthAnimationDuration)
        {
            EasingFunction = new CubicEase(),
            FillBehavior = FillBehavior.HoldEnd
        };
        button.BeginAnimation(property, animation);
    }

    /// <summary>
    /// Animates the opacity of a UI element to a specified value.
    /// </summary>
    /// <param name="element">The UI element to animate.</param>
    /// <param name="value">The target opacity value.</param>
    private static void AnimateOpacity(UIElement element, double value)
    {
        if (!StswSettings.Default.EnableAnimations)
        {
            element.BeginAnimation(OpacityProperty, null);
            element.Opacity = value;
            return;
        }

        var animation = new DoubleAnimation(value, OpacityAnimationDuration)
        {
            EasingFunction = new CubicEase(),
            FillBehavior = FillBehavior.HoldEnd
        };
        element.BeginAnimation(OpacityProperty, animation);
    }

    /// <summary>
    /// Enumerates all directional buttons that are currently assigned.
    /// </summary>
    /// <returns>An enumerable of the directional buttons.</returns>
    private IEnumerable<ButtonBase> EnumerateButtons()
    {
        if (_btnLeft != null)
            yield return _btnLeft;
        if (_btnRight != null)
            yield return _btnRight;
        if (_btnUp != null)
            yield return _btnUp;
        if (_btnDown != null)
            yield return _btnDown;
    }

    /// <summary>
    /// Handles visibility changes of directional buttons.
    /// </summary>
    /// <param name="sender">The button which raised the event.</param>
    /// <param name="e">The dependency property changed event arguments.</param>
    private void Button_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not ButtonBase button || !button.IsVisible)
            return;

        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (!button.IsVisible)
                return;

            _expandedLengths[button] = MeasureButtonLength(button);
            ApplyDynamicMode();
        }), DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Detaches event handlers from all directional buttons to prevent memory leaks.
    /// </summary>
    private void DetachButtonEvents()
    {
        UnregisterButton(ref _btnDown);
        UnregisterButton(ref _btnLeft);
        UnregisterButton(ref _btnRight);
        UnregisterButton(ref _btnUp);
    }

    /// <summary>
    /// Unregisters event handlers from a specific button and sets its reference to null.
    /// </summary>
    /// <param name="button">The button reference to unregister and nullify.</param>
    private void UnregisterButton(ref ButtonBase? button)
    {
        if (button == null)
            return;

        button.MouseEnter -= HandleMouseEnterOnButton;
        button.MouseLeave -= HandleMouseLeaveFromButton;
        button.IsVisibleChanged -= Button_IsVisibleChanged;
        button = null;
    }
    #endregion
}
