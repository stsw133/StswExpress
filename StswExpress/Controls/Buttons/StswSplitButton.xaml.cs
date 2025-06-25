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
/// Represents a button control with an expandable drop-down area.
/// The main button content is set using the <see cref="Header"/> property,
/// while additional elements can be placed inside the drop-down menu.
/// Supports optional auto-closing, customizable corner rounding, and styling enhancements.
/// </summary>
[ContentProperty(nameof(Items))]
public class StswSplitButton : HeaderedItemsControl, IStswCornerControl, IStswDropControl
{
    bool IStswDropControl.SuppressNextOpen { get; set; }

    public StswSplitButton()
    {
        Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, IStswDropControl.PreviewMouseDownOutsideCapturedElement);
    }
    static StswSplitButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSplitButton), new FrameworkPropertyMetadata(typeof(StswSplitButton)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        OnAutoCloseChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <summary>
    /// Occurs when the <see cref="ItemsSource"/> property value changes.
    /// </summary>
    /// <param name="oldValue">The previous value of the <see cref="ItemsSource"/> property.</param>
    /// <param name="newValue">The new value of the <see cref="ItemsSource"/> property.</param>
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
            typeof(StswSplitButton),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnAutoCloseChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnAutoCloseChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswSplitButton stsw)
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
            typeof(StswSplitButton),
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
            typeof(StswSplitButton)
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
            typeof(StswSplitButton),
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
            typeof(StswSplitButton),
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
            typeof(StswSplitButton),
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
            typeof(StswSplitButton),
            new PropertyMetadata(double.NaN)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between the main button and the drop-down arrow.
    /// </summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }
    public static readonly DependencyProperty SeparatorThicknessProperty
        = DependencyProperty.Register(
            nameof(SeparatorThickness),
            typeof(double),
            typeof(StswSplitButton),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswSplitButton>
    <se:StswSplitButton.Header>
        <se:TextBox Text="Enter text..."/>
    </se:StswSplitButton.Header>
    <se:Button Command="{Binding ClearTextCommand}" Content="Clear"/>
    <se:Button Command="{Binding SubmitTextCommand}" Content="Submit"/>
</se:StswSplitButton>

*/
