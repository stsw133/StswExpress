using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Represents a button control with an expandable drop-down menu. 
/// Supports customizable items, automatic closure when an item is selected, 
/// and additional styling options such as corner rounding.
/// </summary>
[ContentProperty(nameof(Items))]
[Stsw(null, Changes = StswPlannedChanges.None)]
public class StswDropButton : HeaderedItemsControl, IStswCornerControl, IStswDropControl
{
    bool IStswDropControl.SuppressNextOpen { get; set; }

    public StswDropButton()
    {
        Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, IStswDropControl.PreviewMouseDownOutsideCapturedElement);
    }
    static StswDropButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDropButton), new FrameworkPropertyMetadata(typeof(StswDropButton)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        OnAutoCloseChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <inheritdoc/>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        base.OnItemsSourceChanged(oldValue, newValue);

        if (AutoClose)
        {
            if (oldValue != null)
                foreach (var item in oldValue)
                    if (item is ButtonBase btn)
                        btn.Click -= (_, _) => IsDropDownOpen = false;

            if (newValue != null)
                foreach (var item in newValue)
                    if (item is ButtonBase btn)
                        btn.Click += (_, _) => IsDropDownOpen = false;
        }
    }
    #endregion

    #region Logic properties
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
    private static void OnAutoCloseChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswDropButton stsw)
        {
            if (stsw.Items != null)
                foreach (var btn in stsw.Items.OfType<ButtonBase>())
                {
                    if (stsw.AutoClose)
                        btn.Click += (_, _) => stsw.IsDropDownOpen = false;
                    else
                        btn.Click -= (_, _) => stsw.IsDropDownOpen = false;
                }
        }
    }

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
    private static void OnIsDropDownOpenChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) => IStswDropControl.IsDropDownOpenChanged(obj, e);

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
    #endregion

    #region Style properties
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
            typeof(StswDropButton),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
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
            typeof(StswDropButton),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
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
}

/* usage:

<se:StswDropButton Header="Options" AutoClose="True">
    <StswButton Content="Option 1"/>
    <StswButton Content="Option 2"/>
</se:StswDropButton>

*/
