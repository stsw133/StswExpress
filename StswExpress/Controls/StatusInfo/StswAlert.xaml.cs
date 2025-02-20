using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// A notification control for displaying temporary alerts.
/// Supports dynamic content, click actions, and automatic removal of notifications.
/// </summary>
/// <remarks>
/// This control allows notifications to be dynamically added, clicked for interaction, and removed after a specified duration.
/// </remarks>
public class StswAlert : ItemsControl
{
    static StswAlert()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswAlert), new FrameworkPropertyMetadata(typeof(StswAlert)));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswAlertItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswAlertItem;

    #region Events & methods
    /// <summary>
    /// Displays a new alert notification with the specified content and an optional click action.
    /// </summary>
    /// <param name="content">The content of the alert.</param>
    /// <param name="onClick">The action to perform when the alert is clicked.</param>
    /// <param name="window">The target window where the alert should be displayed (optional).</param>
    public static void Show(object content, Action onClick, StswWindow? window = null)
    {
        window ??= StswApp.StswWindow;

        if (window.Template.FindName(nameof(StswAlert), window) is StswAlert alert)
        {
            var index = alert.ItemsSource is IList itemsSource ? itemsSource.Add(content) : alert.Items.Add(content);

            void OnStatusChanged(object? sender, EventArgs e)
            {
                if (alert.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    if (alert.ItemContainerGenerator.ContainerFromIndex(index) is StswAlertItem item)
                    {
                        item.MouseUp += (s, e) =>
                        {
                            onClick.Invoke();
                            RemoveItemFromItemsControl(alert, item);
                        };
                        alert.ItemContainerGenerator.StatusChanged -= OnStatusChanged;
                    }
                }
            }
            alert.ItemContainerGenerator.StatusChanged += OnStatusChanged;
        }
    }

    /// <summary>
    /// Removes an item from the alert control.
    /// This method removes the specified alert item from the control's item collection or its bound source.
    /// </summary>
    /// <param name="itemsControl">The <see cref="ItemsControl"/> containing the alert item.</param>
    /// <param name="container">The alert item to remove.</param>
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
    #endregion

    #region Logic properties
    /*
    /// <summary>
    /// Gets or sets a value indicating whether alerts are closable, providing a close button for each alert.
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
            typeof(StswAlert)
        );

    /// <summary>
    /// Gets or sets a value indicating whether alerts are copyable, providing a copy button for each alert.
    /// </summary>
    public bool IsCopyable
    {
        get => (bool)GetValue(IsCopyableProperty);
        set => SetValue(IsCopyableProperty, value);
    }
    public static readonly DependencyProperty IsCopyableProperty
        = DependencyProperty.Register(
            nameof(IsCopyable),
            typeof(bool),
            typeof(StswAlert)
        );

    /// <summary>
    /// Gets or sets a value indicating whether alerts are expandable, providing an expand button for detailed view.
    /// </summary>
    public bool IsExpandable
    {
        get => (bool)GetValue(IsExpandableProperty);
        set => SetValue(IsExpandableProperty, value);
    }
    public static readonly DependencyProperty IsExpandableProperty
        = DependencyProperty.Register(
            nameof(IsExpandable),
            typeof(bool),
            typeof(StswAlert)
        );
    */
    #endregion

    #region Style properties
    /*
    /// <summary>
    /// Gets or sets the duration for which alerts are displayed before being automatically removed.
    /// Defaults to 5 seconds.
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
            typeof(StswAlert),
            new PropertyMetadata(TimeSpan.FromSeconds(5))
        );
    */
    #endregion
}

/* usage:

<se:StswAlert/>

*/
