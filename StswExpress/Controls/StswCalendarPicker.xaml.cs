﻿using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswCalendarPicker.xaml
/// </summary>
public partial class StswCalendarPicker : TextBox
{
    public StswCalendarPicker()
    {
        InitializeComponent();

        SetValue(ButtonsProperty, new ObservableCollection<ButtonBase>());
    }
    static StswCalendarPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswCalendarPicker), new FrameworkPropertyMetadata(typeof(StswCalendarPicker)));
        TextProperty.OverrideMetadata(typeof(StswCalendarPicker), new FrameworkPropertyMetadata(null, OnTextChanged));
    }

    #region Style
    /// BackgroundDisabled
    public static readonly DependencyProperty BackgroundDisabledProperty
        = DependencyProperty.Register(
            nameof(BackgroundDisabled),
            typeof(Brush),
            typeof(StswCalendarPicker),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundDisabled
    {
        get => (Brush)GetValue(BackgroundDisabledProperty);
        set => SetValue(BackgroundDisabledProperty, value);
    }
    /// BorderBrushDisabled
    public static readonly DependencyProperty BorderBrushDisabledProperty
        = DependencyProperty.Register(
            nameof(BorderBrushDisabled),
            typeof(Brush),
            typeof(StswCalendarPicker),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushDisabled
    {
        get => (Brush)GetValue(BorderBrushDisabledProperty);
        set => SetValue(BorderBrushDisabledProperty, value);
    }
    /// ForegroundDisabled
    public static readonly DependencyProperty ForegroundDisabledProperty
        = DependencyProperty.Register(
            nameof(ForegroundDisabled),
            typeof(Brush),
            typeof(StswCalendarPicker),
            new PropertyMetadata(default(Brush))
        );
    public Brush ForegroundDisabled
    {
        get => (Brush)GetValue(ForegroundDisabledProperty);
        set => SetValue(ForegroundDisabledProperty, value);
    }

    /// BackgroundMouseOver
    public static readonly DependencyProperty BackgroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(BackgroundMouseOver),
            typeof(Brush),
            typeof(StswCalendarPicker),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundMouseOver
    {
        get => (Brush)GetValue(BackgroundMouseOverProperty);
        set => SetValue(BackgroundMouseOverProperty, value);
    }
    /// BorderBrushMouseOver
    public static readonly DependencyProperty BorderBrushMouseOverProperty
        = DependencyProperty.Register(
            nameof(BorderBrushMouseOver),
            typeof(Brush),
            typeof(StswCalendarPicker),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushMouseOver
    {
        get => (Brush)GetValue(BorderBrushMouseOverProperty);
        set => SetValue(BorderBrushMouseOverProperty, value);
    }
    /// ForegroundMouseOver
    public static readonly DependencyProperty ForegroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(ForegroundMouseOver),
            typeof(Brush),
            typeof(StswCalendarPicker),
            new PropertyMetadata(default(Brush))
        );
    public Brush ForegroundMouseOver
    {
        get => (Brush)GetValue(ForegroundMouseOverProperty);
        set => SetValue(ForegroundMouseOverProperty, value);
    }

    /// BackgroundFocused
    public static readonly DependencyProperty BackgroundFocusedProperty
        = DependencyProperty.Register(
            nameof(BackgroundFocused),
            typeof(Brush),
            typeof(StswCalendarPicker),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundFocused
    {
        get => (Brush)GetValue(BackgroundFocusedProperty);
        set => SetValue(BackgroundFocusedProperty, value);
    }
    /// BorderBrushFocused
    public static readonly DependencyProperty BorderBrushFocusedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushFocused),
            typeof(Brush),
            typeof(StswCalendarPicker),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushFocused
    {
        get => (Brush)GetValue(BorderBrushFocusedProperty);
        set => SetValue(BorderBrushFocusedProperty, value);
    }
    /// ForegroundFocused
    public static readonly DependencyProperty ForegroundFocusedProperty
        = DependencyProperty.Register(
            nameof(ForegroundFocused),
            typeof(Brush),
            typeof(StswCalendarPicker),
            new PropertyMetadata(default(Brush))
        );
    public Brush ForegroundFocused
    {
        get => (Brush)GetValue(ForegroundFocusedProperty);
        set => SetValue(ForegroundFocusedProperty, value);
    }

    /// BackgroundReadOnly
    public static readonly DependencyProperty BackgroundReadOnlyProperty
        = DependencyProperty.Register(
            nameof(BackgroundReadOnly),
            typeof(Brush),
            typeof(StswCalendarPicker),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundReadOnly
    {
        get => (Brush)GetValue(BackgroundReadOnlyProperty);
        set => SetValue(BackgroundReadOnlyProperty, value);
    }

    /// ForegroundPlaceholder
    public static readonly DependencyProperty ForegroundPlaceholderProperty
        = DependencyProperty.Register(
            nameof(ForegroundPlaceholder),
            typeof(Brush),
            typeof(StswCalendarPicker),
            new PropertyMetadata(default(Brush))
        );
    public Brush ForegroundPlaceholder
    {
        get => (Brush)GetValue(ForegroundPlaceholderProperty);
        set => SetValue(ForegroundPlaceholderProperty, value);
    }

    /// SubBorderThickness
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswCalendarPicker),
            new PropertyMetadata(default(Thickness))
        );
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }
    #endregion

    #region Properties
    /// ButtonClearVisibility
    public static readonly DependencyProperty ButtonClearVisibilityProperty
        = DependencyProperty.Register(
            nameof(ButtonClearVisibility),
            typeof(Visibility),
            typeof(StswCalendarPicker),
            new PropertyMetadata(default(Visibility))
        );
    public Visibility ButtonClearVisibility
    {
        get => (Visibility)GetValue(ButtonClearVisibilityProperty);
        set => SetValue(ButtonClearVisibilityProperty, value);
    }
    /// Buttons
    public static readonly DependencyProperty ButtonsProperty
        = DependencyProperty.Register(
            nameof(Buttons),
            typeof(ObservableCollection<ButtonBase>),
            typeof(StswCalendarPicker),
            new PropertyMetadata(default(ObservableCollection<ButtonBase>))
        );
    public ObservableCollection<ButtonBase> Buttons
    {
        get => (ObservableCollection<ButtonBase>)GetValue(ButtonsProperty);
        set => SetValue(ButtonsProperty, value);
    }
    /// ButtonsAlignment
    public static readonly DependencyProperty ButtonsAlignmentProperty
        = DependencyProperty.Register(
            nameof(ButtonsAlignment),
            typeof(Dock),
            typeof(StswCalendarPicker),
            new PropertyMetadata(default(Dock))
        );
    public Dock ButtonsAlignment
    {
        get => (Dock)GetValue(ButtonsAlignmentProperty);
        set => SetValue(ButtonsAlignmentProperty, value);
    }

    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswCalendarPicker),
            new PropertyMetadata(default(CornerRadius))
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// Format
    public static readonly DependencyProperty FormatProperty
        = DependencyProperty.Register(
            nameof(Format),
            typeof(string),
            typeof(StswCalendarPicker),
            new FrameworkPropertyMetadata(default(DateTime?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFormatChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public string? Format
    {
        get => (string?)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }
    public static void OnFormatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswCalendarPicker stsw)
        {
            if (stsw.Format == null)
                stsw.Text = stsw.SelectedDate?.ToString(CultureInfo.CurrentCulture);
            else
                stsw.Text = stsw.SelectedDate?.ToString(stsw.Format);
        }
    }

    /// IncrementType
    public enum IncrementTypes
    {
        Year,
        Month,
        Day,
        Hour,
        Minute,
        Second
    }
    public static readonly DependencyProperty IncrementTypeProperty
        = DependencyProperty.Register(
              nameof(IncrementType),
              typeof(IncrementTypes),
              typeof(StswCalendarPicker),
              new PropertyMetadata(default(IncrementTypes))
          );
    public IncrementTypes IncrementType
    {
        get => (IncrementTypes)GetValue(IncrementTypeProperty);
        set => SetValue(IncrementTypeProperty, value);
    }
    /// Maximum
    public static readonly DependencyProperty MaximumProperty
        = DependencyProperty.Register(
            nameof(Maximum),
            typeof(DateTime?),
            typeof(StswCalendarPicker),
            new PropertyMetadata(default(DateTime?))
        );
    public DateTime? Maximum
    {
        get => (DateTime?)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }
    /// Minimum
    public static readonly DependencyProperty MinimumProperty
        = DependencyProperty.Register(
            nameof(Minimum),
            typeof(DateTime?),
            typeof(StswCalendarPicker),
            new PropertyMetadata(default(DateTime?))
        );
    public DateTime? Minimum
    {
        get => (DateTime?)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// Placeholder
    public static readonly DependencyProperty PlaceholderProperty
        = DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(StswCalendarPicker),
            new PropertyMetadata(default(string?))
        );
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// SelectedDate
    public static readonly DependencyProperty SelectedDateProperty
        = DependencyProperty.Register(
            nameof(SelectedDate),
            typeof(DateTime?),
            typeof(StswCalendarPicker),
            new FrameworkPropertyMetadata(default(DateTime?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedDateChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }
    public static void OnSelectedDateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswCalendarPicker stsw)
        {
            if (stsw.SelectedDate != null)
            {
                if (stsw.Minimum != null && stsw.SelectedDate < stsw.Minimum)
                    stsw.SelectedDate = stsw.Minimum;
                if (stsw.Maximum != null && stsw.SelectedDate > stsw.Maximum)
                    stsw.SelectedDate = stsw.Maximum;
            }
            OnFormatChanged(stsw, e);

            if (stsw.GetTemplateChild("PART_Popup") is Popup p && p.IsOpen)
                p.IsOpen = false;
        }
    }
    public static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswCalendarPicker stsw)
        {
            if (stsw.Format != null && DateTime.TryParseExact(stsw.Text, stsw.Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime selectedDate))
                stsw.SelectedDate = selectedDate;
            else if (DateTime.TryParse(stsw.Text, out selectedDate))
                stsw.SelectedDate = selectedDate;
            else
            {
                var bindingExpression = BindingOperations.GetBindingExpression(stsw, SelectedDateProperty);
                var boundType = bindingExpression?.ResolvedSource?.GetType().GetProperty(bindingExpression.ResolvedSourcePropertyName)?.PropertyType;
                if (boundType != null)
                {
                    if (Nullable.GetUnderlyingType(boundType) != null || (!boundType.IsValueType && string.IsNullOrEmpty(stsw.Text)))
                        stsw.SelectedDate = null;
                    else
                        Validation.MarkInvalid(bindingExpression, new ValidationError(new ExceptionValidationRule(), SelectedDateProperty, "Incorrect value!", null));
                }
            }
        }
    }
    #endregion

    #region Events
    /// PART_ButtonClear_Click
    private void PART_ButtonClear_Click(object sender, RoutedEventArgs e)
    {
        var bindingExpression = BindingOperations.GetBindingExpression(this, SelectedDateProperty);
        var boundType = bindingExpression?.ResolvedSource?.GetType().GetProperty(bindingExpression.ResolvedSourcePropertyName)?.PropertyType;
        if (boundType != null && Nullable.GetUnderlyingType(boundType) != null)
            SelectedDate = null;
        else
            SelectedDate = DateTime.Now;

        OnFormatChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// PART_IncrementType_Checked
    private void PART_IncrementType_Click(object sender, RoutedEventArgs e)
    {
        IncrementType = ((ButtonBase)sender).Content.ToString() switch
        {
            "YY" => IncrementTypes.Year,
            "MM" => IncrementTypes.Month,
            "DD" => IncrementTypes.Day,
            "hh" => IncrementTypes.Hour,
            "mm" => IncrementTypes.Minute,
            "ss" => IncrementTypes.Second,
            _ => IncrementTypes.Day
        };
        if (GetTemplateChild("PART_IncrementType") is Popup p && p.IsOpen)
            p.IsOpen = false;
        
        Focus();
    }

    /// PART_ContentHost_LostFocus
    private void PART_ContentHost_LostFocus(object sender, RoutedEventArgs e) => OnFormatChanged(this, new DependencyPropertyChangedEventArgs());

    /// PART_ContentHost_MouseDown
    private void PART_ContentHost_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.MiddleButton == MouseButtonState.Pressed && !IsReadOnly)
        {
            if (GetTemplateChild("PART_IncrementType") is Popup p && !p.IsOpen)
                p.IsOpen = true;
        }
    }

    /// PART_ContentHost_MouseWheel
    private void PART_ContentHost_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (IsKeyboardFocused && !IsReadOnly && SelectedDate.HasValue)
        {
            if (e.Delta > 0)
                switch (IncrementType)
                {
                    case IncrementTypes.Year:
                        SelectedDate = SelectedDate.Value.AddYears(1);
                        break;
                    case IncrementTypes.Month:
                        SelectedDate = SelectedDate.Value.AddMonths(1);
                        break;
                    case IncrementTypes.Day:
                        SelectedDate = SelectedDate.Value.AddDays(1);
                        break;
                    case IncrementTypes.Hour:
                        SelectedDate = SelectedDate.Value.AddHours(1);
                        break;
                    case IncrementTypes.Minute:
                        SelectedDate = SelectedDate.Value.AddMinutes(1);
                        break;
                    case IncrementTypes.Second:
                        SelectedDate = SelectedDate.Value.AddSeconds(1);
                        break;
                }
            else
                switch (IncrementType)
                {
                    case IncrementTypes.Year:
                        SelectedDate = SelectedDate.Value.AddYears(-1);
                        break;
                    case IncrementTypes.Month:
                        SelectedDate = SelectedDate.Value.AddMonths(-1);
                        break;
                    case IncrementTypes.Day:
                        SelectedDate = SelectedDate.Value.AddDays(-1);
                        break;
                    case IncrementTypes.Hour:
                        SelectedDate = SelectedDate.Value.AddHours(-1);
                        break;
                    case IncrementTypes.Minute:
                        SelectedDate = SelectedDate.Value.AddMinutes(-1);
                        break;
                    case IncrementTypes.Second:
                        SelectedDate = SelectedDate.Value.AddSeconds(-1);
                        break;
                }

            e.Handled = true;
        }
    }
    #endregion
}
