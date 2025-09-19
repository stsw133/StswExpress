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
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswSplitButton&gt;
///     &lt;se:StswSplitButton.Header&gt;
///         &lt;se:TextBox Text="Enter text..."/&gt;
///     &lt;/se:StswSplitButton.Header&gt;
///     &lt;se:Button Command="{Binding ClearTextCommand}" Content="Clear"/&gt;
///     &lt;se:Button Command="{Binding SubmitTextCommand}" Content="Submit"/&gt;
/// &lt;/se:StswSplitButton&gt;
/// </code>
/// </example>
[ContentProperty(nameof(Items))]
[StswInfo(null)]
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

    /// <inheritdoc/>
    [StswInfo("0.7.0")]
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        base.OnItemsSourceChanged(oldValue, newValue);

        if (AutoClose)
        {
            if (oldValue != null)
                foreach (var item in oldValue)
                    if (item is ButtonBase btn)
                        btn.Click -= OnDropItemClick;

            if (newValue != null)
                foreach (var item in newValue)
                    if (item is ButtonBase btn)
                    {
                        btn.Click -= OnDropItemClick;
                        btn.Click += OnDropItemClick;
                    }
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the drop-down should automatically close after an item inside it is clicked.
    /// </summary>
    [StswInfo("0.7.0")]
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
        if (obj is not StswSplitButton stsw)
            return;

        if (stsw.Items != null)
        {
            foreach (var btn in stsw.Items.OfType<ButtonBase>())
            {
                btn.Click -= stsw.OnDropItemClick;
                if (stsw.AutoClose)
                    btn.Click += stsw.OnDropItemClick;
            }
        }
    }
    private void OnDropItemClick(object sender, RoutedEventArgs e) => IsDropDownOpen = false;

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
    [StswInfo("0.15.0")]
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
