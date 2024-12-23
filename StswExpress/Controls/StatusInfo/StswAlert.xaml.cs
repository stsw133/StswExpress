using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
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
    /// 
    /// </summary>
    /// <param name="content"></param>
    /// <param name="onClick"></param>
    /// <param name="window"></param>
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
    /// Removes an item from an ItemsControl by using its container.
    /// </summary>
    /// <param name="itemsControl">The ItemsControl containing the item.</param>
    /// <param name="container">The item to remove.</param>
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
    /// Gets or sets a value indicating whether the items are closable and have a close button.
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
    /// Gets or sets a value indicating whether the items are copyable and have a copy button.
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
    /// Gets or sets a value indicating whether the items are expandable and have an expand button.
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
    /// 
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

/// <summary>
/// 
/// </summary>
public class StswAlertItem : ContentControl
{
    static StswAlertItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswAlertItem), new FrameworkPropertyMetadata(typeof(StswAlertItem)));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Button: close
        if (GetTemplateChild("PART_CloseButton") is ButtonBase btnClose)
            btnClose.Click += (s, e) => StswAlert.RemoveItemFromItemsControl(StswFn.FindVisualAncestor<ItemsControl>(this), this);
    }
    #endregion

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
            typeof(StswAlertItem),
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
            typeof(StswAlertItem),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
