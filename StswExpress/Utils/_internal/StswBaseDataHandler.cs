using ClosedXML.Excel;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace StswExpress;

/// <summary>
/// Provides a set of static methods for handling data conversion and processing within the <see cref="StswExpress"/> framework.
/// </summary>
internal static class StswBaseDataHandler
{
    /// <summary>
    /// Escapes a CSV value by enclosing it in double quotes if it contains a comma, double quote, or newline.
    /// </summary>
    /// <param name="value">The CSV value to escape.</param>
    /// <returns>The escaped CSV value.</returns>
    internal static string EscapeCsvValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            value = $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }

    /// <summary>
    /// Gets the element type of an IEnumerable.
    /// </summary>
    /// <param name="enumerable">The IEnumerable to get the element type from.</param>
    /// <returns>The element type if found; otherwise, null.</returns>
    internal static Type? GetElementType(IEnumerable enumerable)
    {
        var enumerableType = enumerable.GetType();
        return enumerableType.IsGenericType ? enumerableType.GetGenericArguments().FirstOrDefault() : null;
    }

    /// <summary>
    /// Converts an Excel cell value to the specified type.
    /// </summary>
    /// <param name="cell">The Excel cell to convert.</param>
    /// <param name="type">The type to convert the cell value to.</param>
    /// <param name="format">The format string used for the export.</param>
    /// <returns>The converted value.</returns>
    internal static object? ConvertCellValue(IXLCell cell, Type type, string? format = null)
    {
        if (type == typeof(string))
            return cell.GetString();
        if (type == typeof(int) || type == typeof(int?))
            return ConvertStringToNumeric<int>(cell.GetString(), format);
        if (type == typeof(double) || type == typeof(double?))
            return ConvertStringToNumeric<double>(cell.GetString(), format);
        if (type == typeof(decimal) || type == typeof(decimal?))
            return ConvertStringToNumeric<decimal>(cell.GetString(), format);
        if (type == typeof(bool) || type == typeof(bool?))
            return ConvertStringToBool(cell.GetString(), format);
        if (type == typeof(DateTime) || type == typeof(DateTime?))
            return ConvertStringToDateTime(cell.GetString(), format);

        if (type.IsEnum)
            return Enum.Parse(type, cell.GetString(), true);

        if (type.IsClass)
        {
            var classInstance = Activator.CreateInstance(type);
            var classProperties = type.GetProperties();
            var cellValue = cell.GetString();

            foreach (var prop in classProperties)
            {
                if (string.Equals(prop.Name, cellValue, StringComparison.OrdinalIgnoreCase))
                {
                    var propValue = ConvertCellValue(cell, prop.PropertyType, format);
                    prop.SetValue(classInstance, propValue);
                }
            }

            return classInstance;
        }

        return Convert.ChangeType(cell.Value, type);
    }

    /// <summary>
    /// Converts a string to a boolean value based on the specified format.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <param name="format">The format string used for the export.</param>
    /// <returns>The converted boolean value.</returns>
    internal static bool? ConvertStringToBool(string value, string? format)
    {
        if (format == null)
            return bool.TryParse(value, out var result) ? result : null;

        var formatParts = format.Split('~');
        if (formatParts.Length == 1)
            return value == formatParts[0];
        if (formatParts.Length == 2)
            return value == formatParts[0] ? true : value == formatParts[1] ? false : null;
        if (formatParts.Length == 3)
            return value == formatParts[0] ? true : value == formatParts[1] ? false : value == formatParts[2] ? null : null;

        return null;
    }

    /// <summary>
    /// Converts a string to a DateTime value based on the specified format.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <param name="format">The format string used for the export.</param>
    /// <returns>The converted DateTime value.</returns>
    internal static DateTime? ConvertStringToDateTime(string value, string? format)
    {
        if (string.IsNullOrEmpty(format))
            return DateTime.TryParse(value, out var result) ? result : null;

        if (DateTime.TryParseExact(value, format, null, DateTimeStyles.None, out var formattedResult))
            return formattedResult;

        return null;
    }

    /// <summary>
    /// Converts a string to a numeric value based on the specified type and format.
    /// </summary>
    /// <typeparam name="T">The numeric type to convert to.</typeparam>
    /// <param name="value">The string value to convert.</param>
    /// <param name="format">The format string used for the export (not used in this implementation).</param>
    /// <returns>The converted numeric value.</returns>
    internal static T? ConvertStringToNumeric<T>(string value, string? format) where T : struct, IConvertible
    {
        if (string.IsNullOrEmpty(value))
            return null;

        if (typeof(T) == typeof(int) && int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var intValue))
            return (T)(object)intValue;
        if (typeof(T) == typeof(double) && double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleValue))
            return (T)(object)doubleValue;
        if (typeof(T) == typeof(decimal) && decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var decimalValue))
            return (T)(object)decimalValue;

        return null;
    }

    /// <summary>
    /// Formats a boolean value based on the specified format string.
    /// </summary>
    /// <param name="value">The boolean value to format.</param>
    /// <param name="format">The format string to apply.</param>
    /// <returns>The formatted string.</returns>
    internal static string FormatBool(object? value, string? format)
    {
        if (value == null)
            return format?.Split('~').ElementAtOrDefault(2) ?? string.Empty;

        bool boolValue = Convert.ToBoolean(value);
        var formatParts = format?.Split('~') ?? [];

        return formatParts.ElementAtOrDefault(boolValue ? 0 : 1) ?? string.Empty;
    }
}
