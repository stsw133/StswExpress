using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress.Wpf;

/// <summary>
/// A date picker control that allows users to enter a date manually or select it from a drop-down calendar.
/// Supports custom date formats, min/max validation, and incremental adjustments via mouse wheel.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswDatePicker SelectedDate="{Binding BirthDate}" Format="dd/MM/yyyy" IncrementType="Day"/&gt;
/// </code>
/// </example>
[ContentProperty(nameof(SelectedDate))]
public class StswDatePicker : StswInputBoxBase, IStswBoxControl, IStswCornerControl
{
    static StswDatePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDatePicker), new FrameworkPropertyMetadata(typeof(StswDatePicker)));
    }

    public StswDatePicker()
    {
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());
    }

    #region Dependency properties

    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty =
        DependencyProperty.Register(nameof(CornerClipping), typeof(bool), typeof(StswDatePicker));

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(StswDatePicker));

    /// <inheritdoc/>
    public ReadOnlyObservableCollection<ValidationError> Errors
    {
        get => (ReadOnlyObservableCollection<ValidationError>)GetValue(ErrorsProperty);
        set => SetValue(ErrorsProperty, value);
    }
    public static readonly DependencyProperty ErrorsProperty =
        DependencyProperty.Register(nameof(Errors), typeof(ReadOnlyObservableCollection<ValidationError>), typeof(StswDatePicker));

    /// <summary>
    /// Gets or sets the date format displayed in the control. Example: "dd/MM/yyyy".
    /// </summary>
    public string? Format
    {
        get => (string?)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }
    public static readonly DependencyProperty FormatProperty =
        DependencyProperty.Register(
            nameof(Format),
            typeof(string),
            typeof(StswDatePicker),
            new FrameworkPropertyMetadata(
                default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFormatChanged));

    private static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var input = (StswDatePicker)d;
        input.FormatChanged(input.Format ?? "d");
    }

    /// <inheritdoc/>
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }
    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.Register(nameof(HasError), typeof(bool), typeof(StswDatePicker));

    /// <inheritdoc/>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(StswDatePicker));

    /// <summary>
    /// Gets or sets a value indicating whether the drop-down calendar is currently open.
    /// </summary>
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(StswDatePicker));

    /// <summary>
    /// Gets or sets the increment type used when changing the date with the mouse wheel.
    /// </summary>
    public StswDateTimeIncrementType IncrementType
    {
        get => (StswDateTimeIncrementType)GetValue(IncrementTypeProperty);
        set => SetValue(IncrementTypeProperty, value);
    }
    public static readonly DependencyProperty IncrementTypeProperty =
        DependencyProperty.Register(nameof(IncrementType), typeof(StswDateTimeIncrementType), typeof(StswDatePicker));

    /// <summary>
    /// Gets or sets the maximum allowable date.
    /// </summary>
    public DateTime? Maximum
    {
        get => (DateTime?)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }
    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(DateTime?),
            typeof(StswDatePicker),
            new PropertyMetadata(default(DateTime?), OnMinMaxChanged));

    /// <summary>
    /// Gets or sets the minimum allowable date.
    /// </summary>
    public DateTime? Minimum
    {
        get => (DateTime?)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }
    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
            nameof(Minimum),
            typeof(DateTime?),
            typeof(StswDatePicker),
            new PropertyMetadata(default(DateTime?), OnMinMaxChanged));

    private static void OnMinMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var input = (StswDatePicker)d;
        if (input.SelectedDate != null && !input.SelectedDate.Between(input.Minimum, input.Maximum))
            input.SelectedDate = input.MinMaxValidate(input.SelectedDate);
    }

    /// <summary>
    /// Gets or sets the currently selected date.
    /// </summary>
    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }
    public static readonly DependencyProperty SelectedDateProperty =
        DependencyProperty.Register(
            nameof(SelectedDate),
            typeof(DateTime?),
            typeof(StswDatePicker),
            new FrameworkPropertyMetadata(
                default(DateTime?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                null,
                OnSelectedDateChanging,
                false,
                UpdateSourceTrigger.PropertyChanged));

    private static object? OnSelectedDateChanging(DependencyObject d, object? baseValue)
    {
        var input = (StswDatePicker)d;
        return input.MinMaxValidate((DateTime?)baseValue);
    }

    /// <summary>
    /// Gets or sets the selection unit of the drop-down calendar.
    /// </summary>
    public StswCalendarUnit SelectionUnit
    {
        get => (StswCalendarUnit)GetValue(SelectionUnitProperty);
        set => SetValue(SelectionUnitProperty, value);
    }
    public static readonly DependencyProperty SelectionUnitProperty =
        DependencyProperty.Register(nameof(SelectionUnit), typeof(StswCalendarUnit), typeof(StswDatePicker));

    /// <summary>
    /// Gets or sets the thickness of the separator between the editable area and the drop-down button.
    /// </summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }
    public static readonly DependencyProperty SeparatorThicknessProperty =
        DependencyProperty.Register(
            nameof(SeparatorThickness),
            typeof(double),
            typeof(StswDatePicker),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <inheritdoc/>
    public ObservableCollection<IStswSubControl> SubControls
    {
        get => (ObservableCollection<IStswSubControl>)GetValue(SubControlsProperty);
        set => SetValue(SubControlsProperty, value);
    }
    public static readonly DependencyProperty SubControlsProperty =
        DependencyProperty.Register(nameof(SubControls), typeof(ObservableCollection<IStswSubControl>), typeof(StswDatePicker));

    /// <summary>
    /// Internal editing buffer. <see cref="SelectedDate"/> is the public semantic value of the control.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new string? Text
    {
        get => base.Text;
        internal set => base.Text = value ?? string.Empty;
    }

    #endregion

    #region Template and overrides

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        FormatChanged(Format ?? "d");
    }

    /// <summary>
    /// Enter is the explicit date commit path supplied by <see cref="StswInputBoxBase.CommitOnEnter"/>.
    /// </summary>
    protected override void OnCommit()
    {
        UpdateMainProperty(alwaysUpdate: true);
    }

    /// <summary>
    /// Losing focus commits a valid changed date.
    /// </summary>
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        UpdateMainProperty(alwaysUpdate: false);
        base.OnLostFocus(e);
    }

    /// <inheritdoc/>
    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
        // Handle date stepping before PART_ContentHost/StswScrollView gets the wheel event.
        if (IsKeyboardFocused && !IsReadOnly && IncrementType != StswDateTimeIncrementType.None && TryParseDate(Text, out var result))
        {
            result = e.Delta > 0
                ? IncrementDate(result, IncrementType, increase: true)
                : IncrementDate(result, IncrementType, increase: false);

            SelectedDate = result;
            CaretIndex = Text?.Length ?? 0;
            e.Handled = true;
            return;
        }

        base.OnPreviewMouseWheel(e);
    }

    #endregion

    #region Logic

    /// <summary>
    /// Applies the requested date format to the internal Text-to-SelectedDate binding without replacing the user's binding on SelectedDate.
    /// </summary>
    private void FormatChanged(string? newFormat)
    {
        if (GetBindingExpression(TextProperty)?.ParentBinding is Binding binding)
        {
            var newBinding = (Binding)binding.Clone();
            newBinding.StringFormat = newFormat;
            SetBinding(TextProperty, newBinding);
        }
    }

    /// <summary>
    /// Parses the editing buffer and commits <see cref="SelectedDate"/>.
    /// </summary>
    private void UpdateMainProperty(bool alwaysUpdate)
    {
        var isInvalid = false;
        var isPlain = false;

        var result = SelectedDate;

        if (string.IsNullOrWhiteSpace(Text))
        {
            result = null;
        }
        else if (!string.IsNullOrEmpty(Format)
            && DateTime.TryParseExact(Text, Format, CultureInfo.CurrentCulture, DateTimeStyles.None, out var exact))
        {
            isPlain = true;
            result = exact;
        }
        else if (DateTime.TryParse(Text, CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsed))
        {
            isPlain = true;
            result = parsed;
        }
        else
        {
            isInvalid = true;
        }

        if (result != SelectedDate || alwaysUpdate)
        {
            SelectedDate = result;

            var textBE = GetBindingExpression(TextProperty);
            var valueBE = GetBindingExpression(SelectedDateProperty);

            if (!isInvalid && valueBE?.Status == BindingStatus.Active)
                valueBE.UpdateSource();

            if (textBE != null && textBE.Status is BindingStatus.Active or BindingStatus.UpdateSourceError)
            {
                if (string.IsNullOrWhiteSpace(Text) || isPlain)
                    textBE.UpdateSource();
                else if (isInvalid && alwaysUpdate)
                    textBE.UpdateSource();
            }
        }
    }

    private DateTime? MinMaxValidate(DateTime? value)
    {
        if (value == null)
            return null;

        if (Minimum.HasValue && value < Minimum)
            value = Minimum;

        if (Maximum.HasValue && value > Maximum)
            value = Maximum;

        return value;
    }

    private bool TryParseDate(string? text, out DateTime result)
    {
        if (!string.IsNullOrEmpty(Format)
            && DateTime.TryParseExact(text, Format, CultureInfo.CurrentCulture, DateTimeStyles.None, out result))
            return true;

        return DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out result);
    }

    private static DateTime IncrementDate(DateTime value, StswDateTimeIncrementType incrementType, bool increase)
    {
        return (incrementType, increase) switch
        {
            (StswDateTimeIncrementType.Year, true) => DateTime.MaxValue.AddYears(-1) >= value ? value.AddYears(1) : DateTime.MaxValue,
            (StswDateTimeIncrementType.Month, true) => DateTime.MaxValue.AddMonths(-1) >= value ? value.AddMonths(1) : DateTime.MaxValue,
            (StswDateTimeIncrementType.Day, true) => DateTime.MaxValue.AddDays(-1) >= value ? value.AddDays(1) : DateTime.MaxValue,
            (StswDateTimeIncrementType.Hour, true) => DateTime.MaxValue.AddHours(-1) >= value ? value.AddHours(1) : DateTime.MaxValue,
            (StswDateTimeIncrementType.Minute, true) => DateTime.MaxValue.AddMinutes(-1) >= value ? value.AddMinutes(1) : DateTime.MaxValue,
            (StswDateTimeIncrementType.Second, true) => DateTime.MaxValue.AddSeconds(-1) >= value ? value.AddSeconds(1) : DateTime.MaxValue,

            (StswDateTimeIncrementType.Year, false) => DateTime.MinValue.AddYears(1) <= value ? value.AddYears(-1) : DateTime.MinValue,
            (StswDateTimeIncrementType.Month, false) => DateTime.MinValue.AddMonths(1) <= value ? value.AddMonths(-1) : DateTime.MinValue,
            (StswDateTimeIncrementType.Day, false) => DateTime.MinValue.AddDays(1) <= value ? value.AddDays(-1) : DateTime.MinValue,
            (StswDateTimeIncrementType.Hour, false) => DateTime.MinValue.AddHours(1) <= value ? value.AddHours(-1) : DateTime.MinValue,
            (StswDateTimeIncrementType.Minute, false) => DateTime.MinValue.AddMinutes(1) <= value ? value.AddMinutes(-1) : DateTime.MinValue,
            (StswDateTimeIncrementType.Second, false) => DateTime.MinValue.AddSeconds(1) <= value ? value.AddSeconds(-1) : DateTime.MinValue,
            _ => value
        };
    }

    #endregion
}
