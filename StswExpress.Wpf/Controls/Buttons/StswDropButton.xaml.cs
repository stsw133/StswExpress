using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress.Wpf;

/// <summary>
/// Represents a button control with an expandable drop-down menu. 
/// Supports customizable items, automatic closure when an item is selected, 
/// and additional styling options such as corner rounding.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswDropButton Header="Options" AutoClose="True"&gt;
///     &lt;StswButton Content="Option 1"/&gt;
///     &lt;StswButton Content="Option 2"/&gt;
/// &lt;/se:StswDropButton&gt;
/// </code>
/// </example>
[ContentProperty(nameof(Items))]
public class StswDropButton : HeaderedItemsControl, IStswCornerControl, IStswDropControl
{
    static StswDropButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDropButton), new FrameworkPropertyMetadata(typeof(StswDropButton)));
    }
    public StswDropButton()
    {
        Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, IStswDropControl.PreviewMouseDownOutsideCapturedElement);
    }

    #region Dependency properties
    /// <summary>
    /// Gets or sets a value indicating whether the drop-down should automatically close after an item inside it is clicked.
    /// </summary>
    public bool AutoClose
    {
        get => (bool)GetValue(AutoCloseProperty);
        set => SetValue(AutoCloseProperty, value);
    }
    public static readonly DependencyProperty AutoCloseProperty
        = DependencyProperty.Register(
            nameof(AutoClose),
            typeof(bool),
            typeof(StswDropButton),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnAutoCloseChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnAutoCloseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswDropButton)d;
        stsw.UpdateAutoCloseHandlers(stsw.Items);
    }
    private void OnDropItemClick(object sender, RoutedEventArgs e) => IsDropDownOpen = false;

    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswDropButton)
        );

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswDropButton)
        );

    /// <inheritdoc/>
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    public static readonly DependencyProperty IsDropDownOpenProperty
        = DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(StswDropButton),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsDropDownOpenChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => IStswDropControl.IsDropDownOpenChanged(d, e);

    /// <summary>
    /// Gets or sets a value indicating whether the control is in read-only mode.
    /// When set to <see langword="true"/>, the drop-down can be opened, but all items inside are disabled.
    /// </summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(StswDropButton)
        );

    /// <inheritdoc/>
    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }
    public static readonly DependencyProperty MaxDropDownHeightProperty
        = DependencyProperty.Register(
            nameof(MaxDropDownHeight),
            typeof(double),
            typeof(StswDropButton),
            new PropertyMetadata(SystemParameters.PrimaryScreenHeight / 3)
        );

    /// <inheritdoc/>
    public double MaxDropDownWidth
    {
        get => (double)GetValue(MaxDropDownWidthProperty);
        set => SetValue(MaxDropDownWidthProperty, value);
    }
    public static readonly DependencyProperty MaxDropDownWidthProperty
        = DependencyProperty.Register(
            nameof(MaxDropDownWidth),
            typeof(double),
            typeof(StswDropButton),
            new PropertyMetadata(double.NaN)
        );
    #endregion

    #region Template
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateAutoCloseHandlers(Items);
    }
    #endregion

    #region Overrides
    /// <inheritdoc/>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        base.OnItemsSourceChanged(oldValue, newValue);
        UpdateAutoCloseHandlers(oldValue, detachOnly: true);
        UpdateAutoCloseHandlers(newValue);
    }
    #endregion

    #region Logic
    bool IStswDropControl.SuppressNextOpen { get; set; }

    /// <summary>
    /// Updates the click event handlers for items to manage automatic drop-down closure.
    /// </summary>
    /// <param name="items">The collection of items to update.</param>
    /// <param name="detachOnly">If set to <see langword="true"/>, only detaches existing handlers without adding new ones.</param>
    private void UpdateAutoCloseHandlers(IEnumerable? items, bool detachOnly = false)
    {
        if (items is null)
            return;

        foreach (var btn in items.OfType<ButtonBase>())
        {
            btn.Click -= OnDropItemClick;

            if (!detachOnly && AutoClose)
                btn.Click += OnDropItemClick;
        }
    }
    #endregion
}
