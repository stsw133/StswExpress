﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Converts <see cref="bool"/> → targetType.<br/>
/// Use '<c>!</c>' at the beginning of converter parameter to reverse output value.<br/>
/// <br/>
/// When targetType is <see cref="Visibility"/> then output is <c>Visible</c> when <see langword="true"/>, otherwise <c>Collapsed</c>.<br/>
/// When targetType is anything else then returns <see cref="bool"/> with value depending on converter result.<br/>
/// </summary>
public class StswBoolConverter : MarkupExtension, IValueConverter
{
    private static StswBoolConverter? instance;
    public static StswBoolConverter Instance => instance ??= new StswBoolConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (!bool.TryParse(value?.ToString(), out var val))
            return value;
        var pmr = parameter?.ToString() ?? string.Empty;

        /// parameters
        var isReversed = pmr.Contains('!');
        //if (isReversed)
        //    pmr = pmr.Remove(pmr.IndexOf('!'), 1);

        /// result
        if (targetType == typeof(Visibility))
            return (val ^ isReversed) ? Visibility.Visible : Visibility.Collapsed;
        else
            return val ^ isReversed;
    }

    /// ConvertBack
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
