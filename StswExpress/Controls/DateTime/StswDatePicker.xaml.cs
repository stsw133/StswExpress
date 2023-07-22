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
        SetValue(ComponentsProperty, new ObservableCollection<UIElement>());
    }
    static StswDatePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDatePicker), new FrameworkPropertyMetadata(typeof(StswDatePicker)));
    }

    #region Events
    /// <summary>
    /// Occurs when the selected date in the control changes.
    /// </summary>
    public event EventHandler? SelectedDateChanged;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        /// Content
        if (GetTemplateChild("PART_ContentHost") is ScrollViewer content)
        {
            content.KeyDown += PART_ContentHost_KeyDown;
            content.LostFocus += PART_ContentHost_LostFocus;
            content.MouseWheel += PART_ContentHost_MouseWheel;
        }
        OnFormatChanged(this, new DependencyPropertyChangedEventArgs());

        base.OnApplyTemplate();
    }

    /// <summary>
    /// Handles the KeyDown event for the internal content host of the date picker.
    /// If the Enter key is pressed, the LostFocus event is triggered for the content host.
    /// </summary>
    protected void PART_ContentHost_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            PART_ContentHost_LostFocus(sender, new RoutedEventArgs());
    }

    /// <summary>
    /// Handles the LostFocus event for the internal content host of the date picker.
    /// Parses the text input and updates the SelectedDate property based on the provided format.
    /// The new SelectedDate is displayed in the Text property, and the binding is updated if active.
    /// </summary>
    private void PART_ContentHost_LostFocus(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Text))
            SelectedDate = null;
        else if (Format != null && DateTime.TryParseExact(Text, Format, CultureInfo.CurrentCulture, DateTimeStyles.None, out var result1))
            SelectedDate = result1;
        else if (DateTime.TryParse(Text, out var result2))
            SelectedDate = result2;

        Text = SelectedDate?.ToString(Format);
        var bindingExpression = GetBindingExpression(TextProperty);
        if (bindingExpression != null && bindingExpression.Status.In(BindingStatus.Active/*, BindingStatus.UpdateSourceError*/))
            bindingExpression.UpdateSource();
    }

    /// <summary>
    /// Handles the MouseWheel event for the internal content host of the date picker.
    /// Adjusts the selected date based on the mouse wheel's scrolling direction and the IncrementType property.
    /// </summary>
    private void PART_ContentHost_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (IsKeyboardFocused && !IsReadOnly && SelectedDate.HasValue && IncrementType != StswDateIncrementType.None)
        {
            if (DateTime.TryParse(Text, out var result))
                SelectedDate = result;

            var step = e.Delta > 0 ? 1 : -1;

            try
            {
                SelectedDate = IncrementType switch
                {
                    StswDateIncrementType.Year => SelectedDate.Value.AddYears(step),
                    StswDateIncrementType.Month => SelectedDate.Value.AddMonths(step),
                    StswDateIncrementType.Day => SelectedDate.Value.AddDays(step),
                    StswDateIncrementType.Hour => SelectedDate.Value.AddHours(step),
                    StswDateIncrementType.Minute => SelectedDate.Value.AddMinutes(step),
                    StswDateIncrementType.Second => SelectedDate.Value.AddSeconds(step),
                    _ => SelectedDate
                };
            }
            catch { }

            e.Handled = true;
        }
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the collection of components to be displayed in the control.
    /// </summary>
    public ObservableCollection<UIElement> Components
    {
        get => (ObservableCollection<UIElement>)GetValue(ComponentsProperty);
        set => SetValue(ComponentsProperty, value);
    }
    public static readonly DependencyProperty ComponentsProperty
        = DependencyProperty.Register(
            nameof(Components),
            typeof(ObservableCollection<UIElement>),
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
            new PropertyMetadata(default(DateTime?), OnSelectedDateChanged)
        );

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
            new PropertyMetadata(default(DateTime?), OnSelectedDateChanged)
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
        set => SetValue(SelectedDateProperty, value);
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
            if (stsw.SelectedDate != null)
            {
                if (stsw.Minimum != null && stsw.SelectedDate < stsw.Minimum)
                    stsw.SelectedDate = stsw.Minimum;
                if (stsw.Maximum != null && stsw.SelectedDate > stsw.Maximum)
                    stsw.SelectedDate = stsw.Maximum;
            }

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
