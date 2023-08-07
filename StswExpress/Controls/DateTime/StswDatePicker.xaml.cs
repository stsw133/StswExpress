using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// A control that allows users to select and display date.
/// </summary>
[ContentProperty(nameof(SelectedDate))]
public class StswDatePicker : TextBox
{
    public StswDatePicker()
    {
        SetValue(ComponentsProperty, new ObservableCollection<IStswComponent>());
    }
    static StswDatePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDatePicker), new FrameworkPropertyMetadata(typeof(StswDatePicker)));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the selected date in the control changes.
    /// </summary>
    public event EventHandler? SelectedDateChanged;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        OnFormatChanged(this, new DependencyPropertyChangedEventArgs());

        base.OnApplyTemplate();
    }

    /// <summary>
    /// Handles the KeyDown event for the internal content host of the date picker.
    /// If the Enter key is pressed, the LostFocus event is triggered for the content host.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Enter)
            UpdateMainProperty();
    }

    /// <summary>
    /// Handles the LostFocus event for the internal content host of the date picker.
    /// Parses the text input and updates the SelectedDate property based on the provided format.
    /// The new SelectedDate is displayed in the Text property, and the binding is updated if active.
    /// </summary>
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        UpdateMainProperty();
        base.OnLostFocus(e);
    }

    /// <summary>
    /// Handles the MouseWheel event for the internal content host of the date picker.
    /// Adjusts the selected date based on the mouse wheel's scrolling direction and the IncrementType property.
    /// </summary>
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        if (IsKeyboardFocused && !IsReadOnly && IncrementType != StswDateIncrementType.None && SelectedDate.HasValue)
        {
            if (DateTime.TryParse(Text, out var result))
                SelectedDate = result;

            if (e.Delta > 0)
            {
                SelectedDate = IncrementType switch
                {
                    StswDateIncrementType.Year => DateTime.MaxValue.AddYears(-1) >= SelectedDate ? SelectedDate.Value.AddYears(1) : DateTime.MaxValue,
                    StswDateIncrementType.Month => DateTime.MaxValue.AddMonths(-1) >= SelectedDate ? SelectedDate.Value.AddMonths(1) : DateTime.MaxValue,
                    StswDateIncrementType.Day => DateTime.MaxValue.AddDays(-1) >= SelectedDate ? SelectedDate.Value.AddDays(1) : DateTime.MaxValue,
                    StswDateIncrementType.Hour => DateTime.MaxValue.AddHours(-1) >= SelectedDate ? SelectedDate.Value.AddHours(1) : DateTime.MaxValue,
                    StswDateIncrementType.Minute => DateTime.MaxValue.AddMinutes(-1) >= SelectedDate ? SelectedDate.Value.AddMinutes(1) : DateTime.MaxValue,
                    StswDateIncrementType.Second => DateTime.MaxValue.AddSeconds(-1) >= SelectedDate ? SelectedDate.Value.AddSeconds(1) : DateTime.MaxValue,
                    _ => SelectedDate
                };
            }
            else if (e.Delta < 0)
            {
                SelectedDate = IncrementType switch
                {
                    StswDateIncrementType.Year => DateTime.MinValue.AddYears(1) <= SelectedDate ? SelectedDate.Value.AddYears(-1) : DateTime.MinValue,
                    StswDateIncrementType.Month => DateTime.MinValue.AddMonths(1) <= SelectedDate ? SelectedDate.Value.AddMonths(-1) : DateTime.MinValue,
                    StswDateIncrementType.Day => DateTime.MinValue.AddDays(1) <= SelectedDate ? SelectedDate.Value.AddDays(-1) : DateTime.MinValue,
                    StswDateIncrementType.Hour => DateTime.MinValue.AddHours(1) <= SelectedDate ? SelectedDate.Value.AddHours(-1) : DateTime.MinValue,
                    StswDateIncrementType.Minute => DateTime.MinValue.AddMinutes(1) <= SelectedDate ? SelectedDate.Value.AddMinutes(-1) : DateTime.MinValue,
                    StswDateIncrementType.Second => DateTime.MinValue.AddSeconds(1) <= SelectedDate ? SelectedDate.Value.AddSeconds(-1) : DateTime.MinValue,
                    _ => SelectedDate
                };
            }

            e.Handled = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private DateTime? MinMaxValidate(DateTime? newValue)
    {
        if (newValue.HasValue)
        {
            if (Minimum.HasValue && newValue < Minimum)
                newValue = Minimum;
            if (Maximum.HasValue && newValue > Maximum)
                newValue = Maximum;
        }
        return newValue;
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateMainProperty()
    {
        if (string.IsNullOrEmpty(Text))
            SelectedDate = null;
        else if (Format != null && DateTime.TryParseExact(Text, Format, CultureInfo.CurrentCulture, DateTimeStyles.None, out var result))
            SelectedDate = result;
        else if (DateTime.TryParse(Text, out result))
            SelectedDate = result;

        Text = SelectedDate?.ToString(Format);
        var bindingExpression = GetBindingExpression(TextProperty);
        if (bindingExpression != null && bindingExpression.Status.In(BindingStatus.Active/*, BindingStatus.UpdateSourceError*/))
            bindingExpression.UpdateSource();
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the collection of components to be displayed in the control.
    /// </summary>
    public ObservableCollection<IStswComponent> Components
    {
        get => (ObservableCollection<IStswComponent>)GetValue(ComponentsProperty);
        set => SetValue(ComponentsProperty, value);
    }
    public static readonly DependencyProperty ComponentsProperty
        = DependencyProperty.Register(
            nameof(Components),
            typeof(ObservableCollection<IStswComponent>),
            typeof(StswDatePicker)
        );

    /// <summary>
    /// Gets or sets the alignment of the components within the control.
    /// </summary>
    public Dock ComponentsAlignment
    {
        get => (Dock)GetValue(ComponentsAlignmentProperty);
        set => SetValue(ComponentsAlignmentProperty, value);
    }
    public static readonly DependencyProperty ComponentsAlignmentProperty
        = DependencyProperty.Register(
            nameof(ComponentsAlignment),
            typeof(Dock),
            typeof(StswDatePicker)
        );

    /// <summary>
    /// Gets or sets the custom date and time format string used to display the selected date in the control.
    /// When set, the date is formatted according to the provided format string.
    /// </summary>
    public string? Format
    {
        get => (string?)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }
    public static readonly DependencyProperty FormatProperty
        = DependencyProperty.Register(
            nameof(Format),
            typeof(string),
            typeof(StswDatePicker),
            new PropertyMetadata(default(string?), OnFormatChanged)
        );
    public static void OnFormatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswDatePicker stsw)
        {
            if (stsw.GetBindingExpression(TextProperty)?.ParentBinding is Binding binding and not null)
            {
                var newBinding = new Binding()
                {
                    ConverterCulture = binding.ConverterCulture,
                    Mode = binding.Mode,
                    Path = binding.Path,
                    RelativeSource = binding.RelativeSource,
                    StringFormat = stsw.Format,
                    UpdateSourceTrigger = binding.UpdateSourceTrigger
                };
                stsw.SetBinding(TextProperty, newBinding);
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the drop-down portion of the button is open.
    /// </summary>
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    public static readonly DependencyProperty IsDropDownOpenProperty
        = DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(StswDatePicker)
        );

    /// <summary>
    /// Gets or sets the type of increment to be applied when scrolling the mouse wheel over the date picker.
    /// This property defines how the date changes when the mouse wheel is scrolled up or down.
    /// </summary>
    public StswDateIncrementType IncrementType
    {
        get => (StswDateIncrementType)GetValue(IncrementTypeProperty);
        set => SetValue(IncrementTypeProperty, value);
    }
    public static readonly DependencyProperty IncrementTypeProperty
        = DependencyProperty.Register(
            nameof(IncrementType),
            typeof(StswDateIncrementType),
            typeof(StswDatePicker)
        );

    /// <summary>
    /// Gets or sets the maximum allowable date in the control.
    /// </summary>
    public DateTime? Maximum
    {
        get => (DateTime?)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }
    public static readonly DependencyProperty MaximumProperty
        = DependencyProperty.Register(
            nameof(Maximum),
            typeof(DateTime?),
            typeof(StswDatePicker),
            new PropertyMetadata(default(DateTime?), OnMinMaxChanged)
        );
    public static void OnMinMaxChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswDatePicker stsw)
        {
            stsw.SelectedDate = stsw.MinMaxValidate(stsw.SelectedDate);
        }
    }

    /// <summary>
    /// Gets or sets the minimum allowable date in the control.
    /// </summary>
    public DateTime? Minimum
    {
        get => (DateTime?)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }
    public static readonly DependencyProperty MinimumProperty
        = DependencyProperty.Register(
            nameof(Minimum),
            typeof(DateTime?),
            typeof(StswDatePicker),
            new PropertyMetadata(default(DateTime?), OnMinMaxChanged)
        );

    /// <summary>
    /// Gets or sets the placeholder text to display in the box when no date is selected.
    /// </summary>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }
    public static readonly DependencyProperty PlaceholderProperty
        = DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(StswDatePicker)
        );

    /// <summary>
    /// Gets or sets the currently selected date in the control.
    /// </summary>
    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, MinMaxValidate(value));
    }
    public static readonly DependencyProperty SelectedDateProperty
        = DependencyProperty.Register(
            nameof(SelectedDate),
            typeof(DateTime?),
            typeof(StswDatePicker),
            new FrameworkPropertyMetadata(default(DateTime?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedDateChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnSelectedDateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswDatePicker stsw)
        {
            stsw.SelectedDateChanged?.Invoke(stsw, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets or sets the text value of the control.
    /// </summary>
    [Browsable(false)]
    //[Bindable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new string? Text
    {
        get => base.Text;
        internal set => base.Text = value;
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the degree to which the corners of the control are rounded.
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
            typeof(StswDatePicker)
        );

    /// <summary>
    /// Gets or sets the thickness of the border used as separator between box and drop-down button.
    /// </summary>
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswDatePicker)
        );
    #endregion
}
