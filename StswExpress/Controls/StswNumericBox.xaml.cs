using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
