using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// A flexible converter that allows basic "commands" in ConverterParameter,
/// such as "where", "any", "select", etc.
/// Example usage:
///   ConverterParameter="where ItemState == Unchanged"
///   ConverterParameter="any IsEnabled != false"
/// 
/// This version can be easily extended by adding more cases in the switch statement.
/// </summary>
public class StswLinqConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswLinqConverter Instance => instance ??= new StswLinqConverter();
    private static StswLinqConverter? instance;

    /// <summary>
    /// Provides the singleton instance of the converter.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The singleton instance of the converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Converts the provided collection based on the command specified in the ConverterParameter.
    /// Supports commands like "where", "any", etc., which can filter, check, or manipulate the collection.
    /// </summary>
    /// <param name="value">The input value, expected to be an IEnumerable collection.</param>
    /// <param name="targetType">The target type of the binding.</param>
    /// <param name="parameter">The ConverterParameter string containing the command and its arguments.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    /// The result of applying the specified command to the collection. For example, "where" returns the count of items matching the condition, and "any" returns a boolean.
    /// </returns>
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ICollectionView collectionView)
            value = collectionView.Cast<object>();

        if (value is not IEnumerable collection || parameter is not string paramString)
            return null;

        paramString = paramString.Trim();

        var spaceIndex = paramString.IndexOf(' ');
        var command = spaceIndex > 0 ? paramString[..spaceIndex].ToLower() : paramString.ToLower();
        var commandParams = spaceIndex > 0 ? paramString[(spaceIndex + 1)..].Trim() : string.Empty;

        return command switch
        {
            "any" => HandleAny(collection, commandParams),
            "count" => HandleCount(collection, commandParams),
            "sum" => HandleSum(collection, commandParams),
            "where" => HandleWhere(collection, commandParams),
            _ => $"Unknown command: '{command}'",
        };
    }

    /// <summary>
    /// This converter does not support converting back from the target value to the source value.
    /// </summary>
    /// <param name="value">The value produced by the binding target.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    /// Always returns <see cref="Binding.DoNothing"/> as this converter does not support backward conversion.
    /// </returns>
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

    /// <summary>
    /// Evaluates whether any element in the collection satisfies the specified condition.
    /// </summary>
    /// <param name="collection">The collection to evaluate.</param>
    /// <param name="condition">The condition in the format "PropertyName Operator Value".</param>
    /// <returns>
    /// <see langword="true"/> if at least one element satisfies the condition; otherwise, <see langword="false"/>.
    /// </returns>
    private object HandleAny(IEnumerable collection, string condition)
    {
        var (propertyName, op, targetValueString) = ParseCondition(condition);
        if (propertyName == null)
            return false;

        foreach (var item in collection)
        {
            if (item == null)
                continue;

            var propInfo = item.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (propInfo == null)
                continue;

            var propVal = propInfo.GetValue(item);
            var isMatch = EvaluateCondition(propVal, op, targetValueString, propInfo.PropertyType);

            if (isMatch)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Counts the number of elements in the collection that satisfy the specified condition.
    /// If no condition is provided, returns the total count of elements.
    /// </summary>
    /// <param name="collection">The collection to evaluate.</param>
    /// <param name="condition">The condition in the format "PropertyName Operator Value" or empty.</param>
    /// <returns>
    /// The count of elements that match the condition, or the total count if no condition is provided.
    /// </returns>
    private object HandleCount(IEnumerable collection, string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return collection.Cast<object>().Count();

        var (propertyName, op, targetValueString) = ParseCondition(condition);
        if (propertyName == null)
            return $"Invalid condition format: '{condition}'";

        var count = 0;

        foreach (var item in collection)
        {
            if (item == null)
                continue;

            var propInfo = item.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (propInfo == null)
                continue;

            var propVal = propInfo.GetValue(item);
            var isMatch = EvaluateCondition(propVal, op, targetValueString, propInfo.PropertyType);

            if (isMatch)
                count++;
        }

        return count;
    }

    /// <summary>
    /// Sums the values of a specified numeric property for all elements in the collection.
    /// </summary>
    /// <param name="collection">The collection to evaluate.</param>
    /// <param name="condition">The property to sum, e.g., "PropertyName".</param>
    /// <returns>
    /// The sum of the property values across all elements.
    /// </returns>
    private object HandleSum(IEnumerable collection, string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return $"Invalid format: property name required for 'sum'";

        var propertyName = condition.Trim();
        var sum = 0.0;

        foreach (var item in collection)
        {
            if (item == null)
                continue;

            var propInfo = item.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (propInfo == null)
                continue;

            var propVal = propInfo.GetValue(item);
            if (propVal == null)
                continue;

            try
            {
                var numericValue = System.Convert.ToDouble(propVal, CultureInfo.InvariantCulture);
                sum += numericValue;
            }
            catch
            {
                return $"Property '{propertyName}' is not numeric and cannot be summed.";
            }
        }

        return sum;
    }

    /// <summary>
    /// Filters the collection based on the specified condition and returns the count of items that match the condition.
    /// </summary>
    /// <param name="collection">The collection to filter.</param>
    /// <param name="condition">The condition in the format "PropertyName Operator Value".</param>
    /// <returns>
    /// The number of items in the collection that satisfy the condition.
    /// </returns>
    private object HandleWhere(IEnumerable collection, string condition)
    {
        var (propertyName, op, targetValueString) = ParseCondition(condition);
        if (propertyName == null)
            return $"Invalid condition format: '{condition}'";

        var filtered = new List<object>();

        foreach (var item in collection)
        {
            if (item == null)
                continue;

            var propInfo = item.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (propInfo == null)
                continue;

            var propVal = propInfo.GetValue(item);
            var isMatch = EvaluateCondition(propVal, op, targetValueString, propInfo.PropertyType);

            if (isMatch)
                filtered.Add(item);
        }

        return filtered.Count;
    }

    /// <summary>
    /// Parses a condition string into its components: the property name, operator, and target value.
    /// </summary>
    /// <param name="condition">The condition string in the format "PropertyName Operator Value".</param>
    /// <returns>
    /// A tuple containing the property name, operator, and target value.
    /// If the condition is invalid, the tuple contains nulls.
    /// </returns>
    private (string? propertyName, string? op, string? targetValue) ParseCondition(string condition)
    {
        var parts = condition.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
            return (null, null, null);

        var propertyName = parts[0];
        var op = parts[1];
        var targetValue = string.Join(' ', parts.Skip(2));

        return (propertyName, op, targetValue);
    }

    /// <summary>
    /// Evaluates a condition for a specific property value using the provided operator and target value.
    /// </summary>
    /// <param name="propVal">The current value of the property.</param>
    /// <param name="op">The operator to use for comparison (e.g., "==", "!=", "&gt;", "&lt;").</param>
    /// <param name="targetValueString">The target value as a string.</param>
    /// <param name="propType">The type of the property being evaluated.</param>
    /// <returns>
    /// <see langword="true"/> if the condition is satisfied; otherwise, <see langword="false"/>.
    /// </returns>
    private bool EvaluateCondition(object? propVal, string? op, string? targetValueString, Type propType) => op switch
    {
        "==" => CompareValues(propVal, targetValueString, propType) == 0,
        "!=" => CompareValues(propVal, targetValueString, propType) != 0,
        ">" => CompareValues(propVal, targetValueString, propType) > 0,
        ">=" => CompareValues(propVal, targetValueString, propType) >= 0,
        "<" => CompareValues(propVal, targetValueString, propType) < 0,
        "<=" => CompareValues(propVal, targetValueString, propType) <= 0,
        _ => false,
    };

    /// <summary>
    /// Compares the actual property value with the target value, considering the property's type.
    /// Supports enums, numeric types, booleans, and strings.
    /// </summary>
    /// <param name="propVal">The current value of the property.</param>
    /// <param name="targetValueString">The target value as a string.</param>
    /// <param name="propType">The type of the property being compared.</param>
    /// <returns>
    /// An integer that indicates the relative order of the values being compared:
    /// - <c>0</c> if they are equal,
    /// - less than <c>0</c> if the property value is less than the target value,
    /// - greater than <c>0</c> if the property value is greater than the target value.
    /// </returns>
    private int CompareValues(object? propVal, string? targetValueString, Type propType)
    {
        if (propVal == null && targetValueString == null)
            return 0;
        if (propVal == null)
            return -1;
        if (targetValueString == null)
            return 1;

        try
        {
            if (propType.IsEnum)
            {
                var enumVal = Enum.Parse(propType, targetValueString, ignoreCase: true);
                return Comparer.DefaultInvariant.Compare(propVal, enumVal);
            }
            else if (propType == typeof(int))
            {
                var intVal = int.Parse(targetValueString);
                return ((int)propVal).CompareTo(intVal);
            }
            else if (propType.IsNumericType())
            {
                var doubleVal = double.Parse(targetValueString, CultureInfo.InvariantCulture);
                return ((double)propVal).CompareTo(doubleVal);
            }
            else if (propType == typeof(bool))
            {
                var boolVal = bool.Parse(targetValueString);
                return ((bool)propVal).CompareTo(boolVal);
            }
            else
            {
                var strPropVal = propVal.ToString();
                return string.Compare(strPropVal, targetValueString, StringComparison.OrdinalIgnoreCase);
            }
        }
        catch
        {
            var strPropVal = propVal.ToString();
            return string.Compare(strPropVal, targetValueString, StringComparison.OrdinalIgnoreCase);
        }
    }
}
