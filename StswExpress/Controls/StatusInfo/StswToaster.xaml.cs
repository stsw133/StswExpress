using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// A notification control for displaying temporary toasts.
/// Supports dynamic content, click actions, and automatic removal of notifications.
/// </summary>
/// <remarks>
/// This control allows notifications to be dynamically added, clicked for interaction, and removed after a specified duration.
/// </remarks>
public class StswToaster : ItemsControl
{
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
    public static void Show(StswDialogImage type, object content, Action? onClick, StswWindow? window = null)
    {
        window ??= StswApp.StswWindow;

        if (window.Template.FindName(nameof(StswToaster), window) is StswToaster toaster)
        {
            var toastItem = new StswToastItem
            {
                Type = type,
                Content = content
            };
            toastItem.MouseUp += (s, e) =>
            {
                onClick?.Invoke();
                RemoveItemFromItemsControl(toaster, toastItem);
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
    #endregion

    #region Logic properties
    /*
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

    /// <summary>
    /// Gets or sets a value indicating whether toasts are copyable, providing a copy button for each toast.
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
            typeof(StswToaster)
        );

    /// <summary>
    /// Gets or sets a value indicating whether toasts are expandable, providing an expand button for detailed view.
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
            typeof(StswToaster)
        );
    */
    #endregion

    #region Style properties
    /*
    /// <summary>
    /// Gets or sets the duration for which toasts are displayed before being automatically removed.
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
            typeof(StswToast),
            new PropertyMetadata(TimeSpan.FromSeconds(5))
        );
    */

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

/* usage:

<se:StswToaster/>

*/
