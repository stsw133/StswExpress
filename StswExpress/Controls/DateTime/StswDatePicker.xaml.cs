using System;
using System.Collections.ObjectModel;
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
public class StswDatePicker : TextBox, IStswBoxControl, IStswCornerControl
{
    public StswDatePicker()
    {
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());
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
        base.OnApplyTemplate();

        OnFormatChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <summary>
    /// Handles the KeyDown event for the internal content host of the date picker.
    /// If the Enter key is pressed, the LostFocus event is triggered for the content host.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!AcceptsReturn && e.Key == Key.Enter)
            UpdateMainProperty(true);
    }

    /// <summary>
    /// Handles the LostFocus event for the internal content host of the date picker.
    /// Parses the text input and updates the SelectedDate property based on the provided format.
    /// The new SelectedDate is displayed in the Text property, and the binding is updated if active.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        UpdateMainProperty(false);
        base.OnLostFocus(e);
    }

    /// <summary>
    /// Handles the MouseDown event for the internal content host of the time picker.
    /// If the Middle mouse button is pressed, the IncrementType value is changed.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
            IncrementType = IncrementType.GetNextValue();
    }

    /// <summary>
    /// Handles the MouseWheel event for the internal content host of the date picker.
    /// Adjusts the selected date based on the mouse wheel's scrolling direction and the IncrementType property.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        if (IsKeyboardFocused && !IsReadOnly && IncrementType != StswDateTimeIncrementType.None && DateTime.TryParse(Text, out var result))
        {
            if (e.Delta > 0)
            {
                result = IncrementType switch
                {
                    StswDateTimeIncrementType.Year => DateTime.MaxValue.AddYears(-1) >= result ? result.AddYears(1) : DateTime.MaxValue,
                    StswDateTimeIncrementType.Month => DateTime.MaxValue.AddMonths(-1) >= result ? result.AddMonths(1) : DateTime.MaxValue,
                    StswDateTimeIncrementType.Day => DateTime.MaxValue.AddDays(-1) >= result ? result.AddDays(1) : DateTime.MaxValue,
                    StswDateTimeIncrementType.Hour => DateTime.MaxValue.AddHours(-1) >= result ? result.AddHours(1) : DateTime.MaxValue,
                    StswDateTimeIncrementType.Minute => DateTime.MaxValue.AddMinutes(-1) >= result ? result.AddMinutes(1) : DateTime.MaxValue,
                    StswDateTimeIncrementType.Second => DateTime.MaxValue.AddSeconds(-1) >= result ? result.AddSeconds(1) : DateTime.MaxValue,
                    _ => result
                };
            }
            else //if (e.Delta < 0)
            {
                result = IncrementType switch
                {
                    StswDateTimeIncrementType.Year => DateTime.MinValue.AddYears(1) <= result ? result.AddYears(-1) : DateTime.MinValue,
                    StswDateTimeIncrementType.Month => DateTime.MinValue.AddMonths(1) <= result ? result.AddMonths(-1) : DateTime.MinValue,
                    StswDateTimeIncrementType.Day => DateTime.MinValue.AddDays(1) <= result ? result.AddDays(-1) : DateTime.MinValue,
                    StswDateTimeIncrementType.Hour => DateTime.MinValue.AddHours(1) <= result ? result.AddHours(-1) : DateTime.MinValue,
                    StswDateTimeIncrementType.Minute => DateTime.MinValue.AddMinutes(1) <= result ? result.AddMinutes(-1) : DateTime.MinValue,
                    StswDateTimeIncrementType.Second => DateTime.MinValue.AddSeconds(1) <= result ? result.AddSeconds(-1) : DateTime.MinValue,
                    _ => result
                };
            }
            SelectedDate = result;

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
    /// Updates the main property associated with the selected date in the control based on user input.
    /// </summary>
    /// <param name="alwaysUpdate">A value indicating whether to force a binding update regardless of changes.</param>
    private void UpdateMainProperty(bool alwaysUpdate)
    {
        var result = SelectedDate;

        if (string.IsNullOrEmpty(Text))
            result = null;
        else if (Format != null && DateTime.TryParseExact(Text, Format, CultureInfo.CurrentCulture, DateTimeStyles.None, out var res))
            result = res;
        else if (DateTime.TryParse(Text, out res))
            result = res;

        if (result != SelectedDate || alwaysUpdate)
        {
            Text = result?.ToString(Format);

            var bindingExpression = GetBindingExpression(TextProperty);
            if (bindingExpression != null && bindingExpression.Status.In(BindingStatus.Active/*, BindingStatus.UpdateSourceError*/))
                bindingExpression.UpdateSource();
        }
    }
    #endregion

    #region Main properties
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
            if (stsw.GetBindingExpression(TextProperty)?.ParentBinding is Binding binding)
            {
                var newBinding = binding.Clone();
                newBinding.StringFormat = stsw.Format;
                stsw.SetBinding(TextProperty, newBinding);
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether or not the drop-down portion of the control is currently open.
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
    public StswDateTimeIncrementType IncrementType
    {
        get => (StswDateTimeIncrementType)GetValue(IncrementTypeProperty);
        set => SetValue(IncrementTypeProperty, value);
    }
    public static readonly DependencyProperty IncrementTypeProperty
        = DependencyProperty.Register(
            nameof(IncrementType),
            typeof(StswDateTimeIncrementType),
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
    /// Gets or sets the collection of sub controls to be displayed in the control.
    /// </summary>
    public ObservableCollection<IStswSubControl> SubControls
    {
        get => (ObservableCollection<IStswSubControl>)GetValue(SubControlsProperty);
        set => SetValue(SubControlsProperty, value);
    }
    public static readonly DependencyProperty SubControlsProperty
        = DependencyProperty.Register(
            nameof(SubControls),
            typeof(ObservableCollection<IStswSubControl>),
            typeof(StswDatePicker)
        );

    ///// <summary>
    ///// Gets or sets the text value of the control.
    ///// </summary>
    //[Browsable(false)]
    ////[Bindable(false)]
    //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    //[EditorBrowsable(EditorBrowsableState.Never)]
    //public new string? Text
    //{
    //    get => base.Text;
    //    internal set => base.Text = value;
    //}
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
            typeof(StswDatePicker)
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
            typeof(StswDatePicker)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between box and drop-down button.
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
            typeof(StswDatePicker)
        );
    #endregion
}
