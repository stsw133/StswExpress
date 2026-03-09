using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace StswExpress.Wpf;

/// <summary>
/// Represents a button that executes its command only after being held long enough to fill the circular progress.
/// </summary>
/// <example>
/// <code>
/// &lt;se:StswHoldButton Command="{Binding DeleteCommand}" HoldDuration="0:0:1.2"&gt;
///     &lt;se:StswIcon Data="{x:Static se:StswIcons.TrashCan}"/&gt;
/// &lt;/se:StswHoldButton&gt;
/// </code>
/// </example>
public class StswHoldButton : Button
{
    static StswHoldButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswHoldButton), new FrameworkPropertyMetadata(typeof(StswHoldButton)));
    }
    public StswHoldButton()
    {
        _timer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(20)
        };
        _timer.Tick += Timer_Tick;

        PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
        MouseLeave += OnMouseLeave;
        LostMouseCapture += OnLostMouseCapture;
        Unloaded += OnUnloaded;
    }

    #region Dependency properties
    /// <summary>
    /// Gets or sets time required to fully fill the ring and trigger command.
    /// </summary>
    public TimeSpan HoldDuration
    {
        get => (TimeSpan)GetValue(HoldDurationProperty);
        set => SetValue(HoldDurationProperty, value);
    }
    public static readonly DependencyProperty HoldDurationProperty
        = DependencyProperty.Register(
            nameof(HoldDuration),
            typeof(TimeSpan),
            typeof(StswHoldButton),
            new FrameworkPropertyMetadata(TimeSpan.FromSeconds(1))
        );

    /// <summary>
    /// Gets current hold progress in range 0..1.
    /// </summary>
    public double HoldProgress
    {
        get => (double)GetValue(HoldProgressProperty);
        private set => SetValue(HoldProgressPropertyKey, value);
    }
    private static readonly DependencyPropertyKey HoldProgressPropertyKey
        = DependencyProperty.RegisterReadOnly(
            nameof(HoldProgress),
            typeof(double),
            typeof(StswHoldButton),
            new FrameworkPropertyMetadata(0d)
        );
    public static readonly DependencyProperty HoldProgressProperty = HoldProgressPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets or sets time in which progress returns to zero after release.
    /// </summary>
    public TimeSpan ReleaseDuration
    {
        get => (TimeSpan)GetValue(ReleaseDurationProperty);
        set => SetValue(ReleaseDurationProperty, value);
    }
    public static readonly DependencyProperty ReleaseDurationProperty
        = DependencyProperty.Register(
            nameof(ReleaseDuration),
            typeof(TimeSpan),
            typeof(StswHoldButton),
            new FrameworkPropertyMetadata(TimeSpan.FromMilliseconds(150))
        );
    #endregion

    #region Overrides
    /// <inheritdoc/>
    protected override void OnClick()
    {
        if (_allowClick)
            base.OnClick();
    }
    #endregion

    #region Logic
    private readonly DispatcherTimer _timer;
    private HoldAnimationMode _mode = HoldAnimationMode.None;
    private bool _hasTriggered;
    private bool _allowClick;

    /// <summary>
    /// Handles the preview left mouse button down event to initiate the hold animation if the control is enabled and the left mouse button is pressed.
    /// </summary>
    /// <param name="sender">The source of the event, typically the control receiving the mouse input.</param>
    /// <param name="e">The event data containing information about the mouse button press, including which button was pressed and its state.</param>
    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!IsEnabled || e.ChangedButton != MouseButton.Left)
            return;

        CaptureMouse();
        _hasTriggered = HoldProgress >= 1d;
        _mode = HoldAnimationMode.Filling;
        _timer.Start();
    }

    /// <summary>
    /// Handles the PreviewMouseLeftButtonUp event to release mouse capture and initiate reduction when the left mouse button is released.
    /// </summary>
    /// <param name="sender">The source of the event, typically the control that received the mouse input.</param>
    /// <param name="e">The event data containing information about the mouse button state and event details.</param>
    private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
            return;

        ReleaseMouseCapture();
        StartReducing();
    }

    /// <summary>
    /// Handles the mouse leave event for the associated control, triggering any necessary actions when the mouse pointer exits the control's bounds.
    /// </summary>
    /// <param name="sender">The source of the event, typically the control from which the mouse pointer has left.</param>
    /// <param name="e">The event data containing information about the mouse event.</param>
    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (IsMouseCaptured)
            StartReducing();
    }

    /// <summary>
    /// Handles the event when mouse capture is lost, initiating the reduction process.
    /// </summary>
    /// <param name="sender">The source of the event, typically the control that lost mouse capture.</param>
    /// <param name="e">The event data associated with the mouse event.</param>
    private void OnLostMouseCapture(object sender, MouseEventArgs e)
    {
        StartReducing();
    }

    /// <summary>
    /// Handles the Unloaded event by stopping the associated timer.
    /// </summary>
    /// <remarks>This method should be attached to the Unloaded event of a UI element to ensure that any ongoing timer activity is halted when the element is removed from the visual tree.</remarks>
    /// <param name="sender">The source of the event, typically the control being unloaded.</param>
    /// <param name="e">The event data for the Unloaded event.</param>
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
    }

    /// <summary>
    /// Sets the hold animation mode to reducing if progress is greater than zero; otherwise, disables the animation and stops the timer.
    /// </summary>
    /// <remarks>Call this method to initiate the reducing phase of the hold animation based on the current progress. If there is no progress, the animation is disabled and the timer is stopped.</remarks>
    private void StartReducing()
    {
        _mode = HoldProgress > 0d ? HoldAnimationMode.Reducing : HoldAnimationMode.None;

        if (_mode == HoldAnimationMode.None)
            _timer.Stop();
    }

    /// <summary>
    /// Handles timer tick events to update the hold animation progress and trigger actions based on the current animation mode.
    /// </summary>
    /// <remarks>This method is intended to be called on each tick of the timer to update the hold progress. Depending on whether the animation is in the filling or reducing mode, it will adjust the progress accordingly and trigger the click event when the hold is complete.</remarks>
    /// <param name="sender">The source of the event, typically the timer instance that raised the tick event.</param>
    /// <param name="e">The event data associated with the timer tick.</param>
    private void Timer_Tick(object? sender, EventArgs e)
    {
        var elapsedSeconds = _timer.Interval.TotalSeconds;

        if (_mode == HoldAnimationMode.Filling)
        {
            var durationSeconds = Math.Max(HoldDuration.TotalSeconds, 0.01d);
            HoldProgress = Math.Clamp(HoldProgress + (elapsedSeconds / durationSeconds), 0d, 1d);

            if (HoldProgress >= 1d && !_hasTriggered)
            {
                _hasTriggered = true;
                _allowClick = true;
                base.OnClick();
                _allowClick = false;
            }

            return;
        }

        if (_mode == HoldAnimationMode.Reducing)
        {
            var durationSeconds = Math.Max(ReleaseDuration.TotalSeconds, 0.01d);
            HoldProgress = Math.Clamp(HoldProgress - (elapsedSeconds / durationSeconds), 0d, 1d);

            if (HoldProgress <= 0d)
            {
                _mode = HoldAnimationMode.None;
                _hasTriggered = false;
                _timer.Stop();
            }
        }
    }

    /// <summary>
    /// Defines the modes of the hold animation, indicating whether the animation is currently filling, reducing, or inactive (none).
    /// </summary>
    private enum HoldAnimationMode
    {
        None,
        Filling,
        Reducing
    }
    #endregion
}
