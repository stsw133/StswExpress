using System;
using System.Collections;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace StswExpress;

/// <summary>
/// A notification control for displaying temporary toasts.
/// Supports dynamic content, click actions, and automatic removal of notifications.
/// </summary>
/// <remarks>
/// This control allows notifications to be dynamically added, clicked for interaction, and removed after a specified duration.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswToaster/&gt;
/// </code>
/// </example>
public class StswToaster : ItemsControl
{
    private readonly Timer? _timer;
    private bool _fastRemoving;
    private bool _timerStarted;

    public StswToaster()
    {
        _timer = new Timer(OnTimerTick, null, Timeout.Infinite, Timeout.Infinite);
    }
    static StswToaster()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswToaster), new FrameworkPropertyMetadata(typeof(StswToaster)));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswToastItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswToastItem;

    #region Events & methods
    /// <summary>
    /// Displays a new toast notification with the specified content and an optional click action.
    /// </summary>
    /// <param name="content">The content of the toast.</param>
    /// <param name="onClick">The action to perform when the toast is clicked.</param>
    /// <param name="window">The target window where the toast should be displayed (optional).</param>
    public static void Show(StswDialogImage type, object content, Action? onClick = null, StswWindow? window = null)
    {
        window ??= StswApp.StswWindow;

        if (window.Template.FindName(nameof(StswToaster), window) is StswToaster toaster)
        {
            var toastItem = new StswToastItem
            {
                Type = type,
                Content = content,
                ClickAction = onClick
            };

            if (toaster.ItemsSource is IList itemsSource)
            {
                if (toaster.GenerateAtBottom)
                    itemsSource.Insert(0, toastItem);
                else
                    itemsSource.Add(toastItem);
            }
            else
            {
                if (toaster.GenerateAtBottom)
                    toaster.Items.Insert(0, toastItem);
                else
                    toaster.Items.Add(toastItem);
            }

            if (!toaster.IsMouseOver && !toaster._timerStarted)
                toaster.StartTimer();
            else if (!toaster.IsMouseOver)
                toaster._timer?.Change(toaster.DisplayDuration, toaster.DisplayDuration);
        }
    }

    /// <summary>
    /// Removes an item from the toaster control.
    /// This method removes the specified toast item from the control's item collection or its bound source.
    /// </summary>
    /// <param name="itemsControl">The <see cref="ItemsControl"/> containing the toast item.</param>
    /// <param name="container">The toast item to remove.</param>
    internal static void RemoveItemFromItemsControl(ItemsControl? itemsControl, DependencyObject? container)
    {
        if (itemsControl == null || container == null)
            return;

        var item = itemsControl.ItemContainerGenerator.ItemFromContainer(container);
        if (itemsControl.ItemsSource is IList itemsSource)
            itemsSource.Remove(item);
        else if (itemsControl.Items.Contains(item))
            itemsControl.Items.Remove(item);
    }

    /// <inheritdoc/>
    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        if (_timerStarted)
            StopTimer();
    }

    /// <inheritdoc/>
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (!_timerStarted)
            StartTimer();
    }

    /// <summary>
    /// Handles the timer tick event for automatic removal of toasts.
    /// </summary>
    /// <param name="state">The state object passed to the timer (not used).</param>
    private void OnTimerTick(object? state)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (Items.Count == 0 || DisplayDuration == default)
            {
                StopTimer();
                return;
            }

            var item = (GenerateAtBottom ? Items[^1] : Items[0]) as StswToastItem;

            if (!_fastRemoving)
            {
                _timer?.Change(TimeSpan.FromSeconds(.5), TimeSpan.FromSeconds(.5));
                _fastRemoving = true;
            }

            HideToastItem(item);
        });
    }

    /// <summary>
    /// Hides the specified toast item with a fade-out animation.
    /// </summary>
    /// <param name="item">The toast item to hide.</param>
    private void HideToastItem(StswToastItem? item)
    {
        if (item == null)
            return;

        var sb = new Storyboard();

        var opacityAnim = new DoubleAnimation(
            item.Opacity,
            0,
            new Duration(TimeSpan.FromSeconds(0.15)))
        {
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(opacityAnim);
        Storyboard.SetTarget(opacityAnim, item);
        Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(OpacityProperty));

        var heightAnim = new DoubleAnimation(
            item.ActualHeight,
            0,
            new Duration(TimeSpan.FromSeconds(0.25)))
        {
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(heightAnim);
        Storyboard.SetTarget(heightAnim, item);
        Storyboard.SetTargetProperty(heightAnim, new PropertyPath(HeightProperty));

        sb.Completed += (_, _) => RemoveItemFromItemsControl(this, item);

        sb.Begin();
    }

    /// <summary>
    /// Starts the timer for automatic removal of toasts.
    /// </summary>
    private void StartTimer()
    {
        _timerStarted = true;
        _timer?.Change(DisplayDuration, DisplayDuration);
    }

    /// <summary>
    /// Stops the timer for automatic removal of toasts.
    /// </summary>
    private void StopTimer()
    {
        _timerStarted = false;
        _fastRemoving = false;
        _timer?.Change(Timeout.Infinite, Timeout.Infinite);
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the duration for which toasts are displayed before being automatically removed.
    /// </summary>
    public TimeSpan DisplayDuration
    {
        get => (TimeSpan)GetValue(DisplayDurationProperty);
        set => SetValue(DisplayDurationProperty, value);
    }
    public static readonly DependencyProperty DisplayDurationProperty
        = DependencyProperty.Register(
            nameof(DisplayDuration),
            typeof(TimeSpan),
            typeof(StswToaster)
        );

    /// <summary>
    /// Gets or sets a value indicating whether toasts are closable, providing a close button for each alert.
    /// </summary>
    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }
    public static readonly DependencyProperty IsClosableProperty
        = DependencyProperty.Register(
            nameof(IsClosable),
            typeof(bool),
            typeof(StswToaster)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether toasts are generated at the bottom of the toast list.
    /// If <see langword="true"/>, new toasts are added to the bottom; otherwise, they are added to the top.
    /// </summary>
    public bool GenerateAtBottom
    {
        get => (bool)GetValue(GenerateAtBottomProperty);
        set => SetValue(GenerateAtBottomProperty, value);
    }
    public static readonly DependencyProperty GenerateAtBottomProperty
        = DependencyProperty.Register(
            nameof(GenerateAtBottom),
            typeof(bool),
            typeof(StswToaster)
        );

    /// <summary>
    /// Gets or sets the text trimming behavior for the toast items.
    /// Defines how the text is trimmed when it overflows the available space.
    /// </summary>
    public TextTrimming TextTrimming
    {
        get => (TextTrimming)GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }
    public static readonly DependencyProperty TextTrimmingProperty
        = DependencyProperty.Register(
            nameof(TextTrimming),
            typeof(TextTrimming),
            typeof(StswToaster)
        );

    /// <summary>
    /// Gets or sets the text wrapping behavior for the toast items.
    /// Defines how the text is wrapped when it overflows the available space.
    /// </summary>
    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }
    public static readonly DependencyProperty TextWrappingProperty
        = DependencyProperty.Register(
            nameof(TextWrapping),
            typeof(TextWrapping),
            typeof(StswToaster)
        );
    #endregion
}
