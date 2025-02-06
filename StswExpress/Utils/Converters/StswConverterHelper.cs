using System;
using System.Windows;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// Provides helper methods for value converters.
/// </summary>
internal static class StswConverterHelper
{
    /// <summary>
    /// Converts a given value to the expected target type.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The expected output type.</param>
    /// <returns>
    /// - If <paramref name="targetType"/> is <see cref="Visibility"/>, returns <see cref="Visibility.Visible"/> or <see cref="Visibility.Collapsed"/> based on a boolean evaluation.
    /// - Otherwise, returns the value converted to <paramref name="targetType"/>.
    /// </returns>
    public static object? ConvertToTargetType(object? value, Type targetType)
    {
        if (targetType == typeof(Visibility))
        {
            if (value is bool booleanValue)
                return booleanValue ? Visibility.Visible : Visibility.Collapsed;
            return value is not null ? Visibility.Visible : Visibility.Collapsed;
        }

        try
        {
            //return System.Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            return value.ConvertTo(targetType);
        }
        catch
        {
            return value ?? Binding.DoNothing;
        }
    }
}
