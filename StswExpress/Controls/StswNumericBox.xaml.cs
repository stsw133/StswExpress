﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswNumericBox.xaml
/// </summary>
public partial class StswNumericBox : UserControl
{
    public StswNumericBox()
    {
        InitializeComponent();
    }

    /// BoxAlignment
    public static readonly DependencyProperty BoxAlignmentProperty
        = DependencyProperty.Register(
              nameof(BoxAlignment),
              typeof(HorizontalAlignment),
              typeof(StswNumericBox),
              new PropertyMetadata(default(HorizontalAlignment))
          );
    public HorizontalAlignment BoxAlignment
    {
        get => (HorizontalAlignment)GetValue(BoxAlignmentProperty);
        set => SetValue(BoxAlignmentProperty, value);
    }

    /// ButtonsAlignment
    public static readonly DependencyProperty ButtonsAlignmentProperty
        = DependencyProperty.Register(
              nameof(ButtonsAlignment),
              typeof(Dock),
              typeof(StswNumericBox),
              new PropertyMetadata(Dock.Right)
          );
    public Dock ButtonsAlignment
    {
        get => (Dock)GetValue(ButtonsAlignmentProperty);
        set => SetValue(ButtonsAlignmentProperty, value);
    }
    /*
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius?),
            typeof(StswNumericBox),
            new PropertyMetadata(default(CornerRadius?))
        );
    public CornerRadius? CornerRadius
    {
        get => (CornerRadius?)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    */
    /// Increment
    public static readonly DependencyProperty IncrementProperty
        = DependencyProperty.Register(
              nameof(Increment),
              typeof(double),
              typeof(StswNumericBox),
              new PropertyMetadata(1d)
          );
    public double Increment
    {
        get => (double)GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
    }

    /// Max
    public static readonly DependencyProperty MaxProperty
        = DependencyProperty.Register(
              nameof(Max),
              typeof(double?),
              typeof(StswNumericBox),
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
              typeof(StswNumericBox),
              new PropertyMetadata(default(double?))
          );
    public double? Min
    {
        get => (double?)GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }

    /// Value
    public static readonly DependencyProperty ValueProperty
        = DependencyProperty.Register(
              nameof(Value),
              typeof(double?),
              typeof(StswNumericBox),
              new PropertyMetadata(default(double?))
          );
    public double? Value
    {
        get => (double?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
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

    /// TextBox_LostFocus
    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
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