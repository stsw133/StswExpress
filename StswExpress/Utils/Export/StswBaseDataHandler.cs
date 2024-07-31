using ClosedXML.Excel;
using System;
using System.Collections;
using System.Linq;

namespace StswExpress;

internal static class StswBaseDataHandler
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
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
        {
            if (value == formatParts[0])
                return true;
            else
                return false;
        }
        else if (formatParts.Length == 2)
        {
            if (value == formatParts[0])
                return true;
            if (value == formatParts[1])
                return false;
        }
        else if (formatParts.Length == 3)
        {
            if (value == formatParts[0])
                return true;
            if (value == formatParts[1])
                return false;
            if (value == formatParts[2])
                return null;
        }

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
        if (DateTime.TryParse(value, out var result))
            return result;
        return null;
    }

    /// <summary>
    /// Converts a string to a numeric value based on the specified type and format.
    /// </summary>
    /// <typeparam name="T">The numeric type to convert to.</typeparam>
    /// <param name="value">The string value to convert.</param>
    /// <param name="format">The format string used for the export.</param>
    /// <returns>The converted numeric value.</returns>
    internal static T? ConvertStringToNumeric<T>(string value, string? format) where T : struct, IConvertible
    {
        if (string.IsNullOrEmpty(value))
            return null;

        if (typeof(T) == typeof(int))
            return int.TryParse(value, out var result) ? (T)(object)result : (T?)(object?)null;
        if (typeof(T) == typeof(double))
            return double.TryParse(value, out var result) ? (T)(object)result : (T?)(object?)null;
        if (typeof(T) == typeof(decimal))
            return decimal.TryParse(value, out var result) ? (T)(object)result : (T?)(object?)null;

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

        if (boolValue)
            return formatParts.ElementAtOrDefault(0) ?? string.Empty;
        else
            return formatParts.ElementAtOrDefault(1) ?? string.Empty;
    }
}
