﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswNumericBox.xaml
/// </summary>
public partial class StswNumericBox : StswNumericBoxBase
{
    public StswNumericBox()
    {
        InitializeComponent();
    }
    static StswNumericBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNumericBox), new FrameworkPropertyMetadata(typeof(StswNumericBox)));
    }

    /// BtnDown_Click
    private void BtnDown_Click(object sender, RoutedEventArgs e)
    {
        Value -= Increment;
        if (Value == null)
        {
            if (((double?)0).Between(Min, Max))
                Value = 0;
            else
                Value = Math.Min(Math.Abs(Min ?? 0d), Math.Abs(Max ?? 0d));
        }
        else if (Min != null && Value < Min)
            Value = (double)Min;
    }

    /// BtnUp_Click
    private void BtnUp_Click(object sender, RoutedEventArgs e)
    {
        Value += Increment;
        if (Value == null)
        {
            if (((double?)0).Between(Min, Max))
                Value = 0;
            else
                Value = Math.Min(Math.Abs(Min ?? 0d), Math.Abs(Max ?? 0d));
        }
        else if (Max != null && Value > Max)
            Value = (double)Max;
    }

    /// TextBox_LostFocus
    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        /*
        var chars = ((TextBox)sender).Text?.ToString()?.ToCharArray()?.Where(x => !char.IsLetter(x))?.ToList();
        var val = string.Concat(chars);

        if (val != null)
        {
            double.TryParse(val, out var res);
            Value = res;
        }
        */
        if (Min != null && Value < Min)
            Value = (double)Min;
        if (Max != null && Value > Max)
            Value = (double)Max;
    }

    /// TextBox_KeyDown
    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            if (e.Key == Key.Enter)
                Value = Convert.ToDouble(((TextBox)sender).Text);
        }
        catch { }
    }
}

public class StswNumericBoxBase : UserControl
{
    #region StyleColors
    /// StyleColorDisabledBackground
    public static readonly DependencyProperty StyleColorDisabledBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorDisabledBackground),
            typeof(Brush),
            typeof(StswNumericBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorDisabledBackground
    {
        get => (Brush)GetValue(StyleColorDisabledBackgroundProperty);
        set => SetValue(StyleColorDisabledBackgroundProperty, value);
    }

    /// StyleColorDisabledBorder
    public static readonly DependencyProperty StyleColorDisabledBorderProperty
        = DependencyProperty.Register(
            nameof(StyleColorDisabledBorder),
            typeof(Brush),
            typeof(StswNumericBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorDisabledBorder
    {
        get => (Brush)GetValue(StyleColorDisabledBorderProperty);
        set => SetValue(StyleColorDisabledBorderProperty, value);
    }

    /// StyleColorMouseOverBackground
    public static readonly DependencyProperty StyleColorMouseOverBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorMouseOverBackground),
            typeof(Brush),
            typeof(StswNumericBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorMouseOverBackground
    {
        get => (Brush)GetValue(StyleColorMouseOverBackgroundProperty);
        set => SetValue(StyleColorMouseOverBackgroundProperty, value);
    }

    /// StyleColorMouseOverBorder
    public static readonly DependencyProperty StyleColorMouseOverBorderProperty
        = DependencyProperty.Register(
            nameof(StyleColorMouseOverBorder),
            typeof(Brush),
            typeof(StswNumericBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorMouseOverBorder
    {
        get => (Brush)GetValue(StyleColorMouseOverBorderProperty);
        set => SetValue(StyleColorMouseOverBorderProperty, value);
    }

    /// StyleColorPressedBackground
    public static readonly DependencyProperty StyleColorPressedBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorPressedBackground),
            typeof(Brush),
            typeof(StswNumericBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorPressedBackground
    {
        get => (Brush)GetValue(StyleColorPressedBackgroundProperty);
        set => SetValue(StyleColorPressedBackgroundProperty, value);
    }

    /// StyleColorPressedBorder
    public static readonly DependencyProperty StyleColorPressedBorderProperty
        = DependencyProperty.Register(
            nameof(StyleColorPressedBorder),
            typeof(Brush),
            typeof(StswNumericBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorPressedBorder
    {
        get => (Brush)GetValue(StyleColorPressedBorderProperty);
        set => SetValue(StyleColorPressedBorderProperty, value);
    }

    /// StyleColorReadOnlyBackground
    public static readonly DependencyProperty StyleColorReadOnlyBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorReadOnlyBackground),
            typeof(Brush),
            typeof(StswNumericBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorReadOnlyBackground
    {
        get => (Brush)GetValue(StyleColorReadOnlyBackgroundProperty);
        set => SetValue(StyleColorReadOnlyBackgroundProperty, value);
    }

    /// StyleColorReadOnlyBorder
    public static readonly DependencyProperty StyleColorReadOnlyBorderProperty
        = DependencyProperty.Register(
            nameof(StyleColorReadOnlyBorder),
            typeof(Brush),
            typeof(StswNumericBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorReadOnlyBorder
    {
        get => (Brush)GetValue(StyleColorReadOnlyBorderProperty);
        set => SetValue(StyleColorReadOnlyBorderProperty, value);
    }

    /// StyleColorPlaceholder
    public static readonly DependencyProperty StyleColorPlaceholderProperty
        = DependencyProperty.Register(
            nameof(StyleColorPlaceholder),
            typeof(Brush),
            typeof(StswNumericBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorPlaceholder
    {
        get => (Brush)GetValue(StyleColorPlaceholderProperty);
        set => SetValue(StyleColorPlaceholderProperty, value);
    }

    /// StyleThicknessSubBorder
    public static readonly DependencyProperty StyleThicknessSubBorderProperty
        = DependencyProperty.Register(
            nameof(StyleThicknessSubBorder),
            typeof(Thickness),
            typeof(StswNumericBoxBase),
            new PropertyMetadata(default(Thickness))
        );
    public Thickness StyleThicknessSubBorder
    {
        get => (Thickness)GetValue(StyleThicknessSubBorderProperty);
        set => SetValue(StyleThicknessSubBorderProperty, value);
    }
    #endregion

    /// ButtonsAlignment
    public static readonly DependencyProperty ButtonsAlignmentProperty
        = DependencyProperty.Register(
              nameof(ButtonsAlignment),
              typeof(Dock),
              typeof(StswNumericBoxBase),
              new PropertyMetadata(Dock.Right)
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
            typeof(StswNumericBoxBase),
            new PropertyMetadata(default(CornerRadius))
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// Increment
    public static readonly DependencyProperty IncrementProperty
        = DependencyProperty.Register(
              nameof(Increment),
              typeof(double),
              typeof(StswNumericBoxBase),
              new PropertyMetadata(1d)
          );
    public double Increment
    {
        get => (double)GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
    }

    /// IsReadOnly
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(StswNumericBoxBase),
            new PropertyMetadata(default(bool))
        );
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// Max
    public static readonly DependencyProperty MaxProperty
        = DependencyProperty.Register(
            nameof(Max),
            typeof(double?),
            typeof(StswNumericBoxBase),
            new PropertyMetadata(default(double?))
        );
    public double? Max
    {
        get => (double?)GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    /// Min
    public static readonly DependencyProperty MinProperty
        = DependencyProperty.Register(
            nameof(Min),
            typeof(double?),
            typeof(StswNumericBoxBase),
            new PropertyMetadata(default(double?))
        );
    public double? Min
    {
        get => (double?)GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }

    /// Placeholder
    public static readonly DependencyProperty PlaceholderProperty
        = DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(StswNumericBoxBase),
            new PropertyMetadata(default(string?))
        );
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// Value
    public static readonly DependencyProperty ValueProperty
        = DependencyProperty.Register(
            nameof(Value),
            typeof(double?),
            typeof(StswNumericBoxBase),
            new PropertyMetadata(default(double?))
        );
    public double? Value
    {
        get => (double?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}
