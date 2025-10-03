using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// A flexible converter that applies basic LINQ-like queries to collections in XAML.
/// Supports the following commands in `ConverterParameter`:
/// - `"any Property Operator Value"` → Returns `true` if at least one item matches the condition.
/// - `"count Property Operator Value"` → Returns the count of matching elements.
/// - `"sum Property"` → Returns the sum of a numeric property.
/// - `"where Property Operator Value"` → Returns a filtered collection.
/// </summary>
/// <example>
/// Example XAML usage:
/// ```xml
/// <TextBlock Text="{Binding Items, Converter={StaticResource StswLinqConverter}, ConverterParameter='any IsEnabled == true'}"/>
/// <TextBlock Text="{Binding Items, Converter={StaticResource StswLinqConverter}, ConverterParameter='count Visibility == Collapsed'}"/>
/// ```
/// </example>
public class StswLinqConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswLinqConverter Instance => instance ??= new StswLinqConverter();
    private static StswLinqConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <inheritdoc/>
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ICollectionView collectionView)
            value = collectionView.Cast<object>();

        if (value is not IEnumerable collection || parameter is not string paramString)
            return null;

        paramString = paramString.Trim();

        var spaceIndex = paramString.IndexOf(' ');
        var command = (spaceIndex > 0 ? paramString[..spaceIndex] : paramString).ToLower();
        var commandParams = spaceIndex > 0 ? paramString[(spaceIndex + 1)..].Trim() : string.Empty;

        return command switch
        {
            "any" => HandleAny(collection, commandParams),
            "average" or "avg" => HandleAverage(collection, commandParams),
            "count" => HandleCount(collection, commandParams),
            "sum" => HandleSum(collection, commandParams),
            "where" => HandleWhere(collection, commandParams),
            _ => $"Unknown command: '{command}'",
        };
    }

    /// <inheritdoc/>
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

    /// <summary>
    /// Checks if any element in the collection satisfies the given condition.
    /// </summary>
    /// <param name="collection">The collection to evaluate.</param>
    /// <param name="condition">
    /// A condition string in the format: `Property Operator Value`.
    /// Example: `"IsEnabled == true"`, `"Age > 18"`, `"Status != Inactive"`.
    /// </param>
    /// <returns><see langword="true"/> if at least one item matches the condition; otherwise, <see langword="false"/>.</returns>
    private static bool HandleAny(IEnumerable collection, string condition)
    {
        return collection.Cast<object>()
                         .Any(item => EvaluateCondition(item, condition));
    }

    /// <summary>
    /// Calculates the average of a specified numeric property for all elements in the collection.
    /// </summary>
    /// <param name="collection">The collection to evaluate.</param>
    /// <param name="propertyName">The name of the numeric property to calculate the average.</param>
    /// <returns>
    /// The average of the property values across all elements, or an error message if the property is not numeric.
    /// </returns>
    private static object HandleAverage(IEnumerable collection, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return $"Invalid format: property name required for 'average'";

        return collection.Cast<object>()
                         .Select(item => item.GetPropertyValue(propertyName))
                         .Where(value => double.TryParse(value?.ToString(), out var _))
                         .Average(value => System.Convert.ToDouble(value, CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Counts the number of elements in the collection that satisfy the given condition.
    /// </summary>
    /// <param name="collection">The collection to evaluate.</param>
    /// <param name="condition">
    /// A condition string in the format: `Property Operator Value`.
    /// Example: `"IsVisible == false"`, `"Quantity >= 10"`.
    /// </param>
    /// <returns>The number of elements that match the condition.</returns>
    private static int HandleCount(IEnumerable collection, string condition)
    {
        return collection.Cast<object>()
                         .Count(item => EvaluateCondition(item, condition));
    }

    /// <summary>
    /// Sums the values of a specified numeric property for all elements in the collection.
    /// </summary>
    /// <param name="collection">The collection to evaluate.</param>
    /// <param name="propertyName">The name of the numeric property to sum.</param>
    /// <returns>
    /// The sum of the property values across all elements, or an error message if the property is not numeric.
    /// </returns>
    private static object HandleSum(IEnumerable collection, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return $"Invalid format: property name required for 'sum'";

        return collection.Cast<object>()
                         .Select(item => item.GetPropertyValue(propertyName))
                         .Where(value => double.TryParse(value?.ToString(), out var _))
                         .Sum(value => System.Convert.ToDouble(value, CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Filters the collection based on the given condition.
    /// </summary>
    /// <param name="collection">The collection to filter.</param>
    /// <param name="condition">
    /// A condition string in the format: `Property Operator Value`.
    /// Example: `"Category == 'Electronics'"`, `"Price < 500"`.
    /// </param>
    /// <returns>A filtered collection containing only the elements that match the condition.</returns>
    private static IEnumerable HandleWhere(IEnumerable collection, string condition)
    {
        return collection.Cast<object>()
                         .Where(item => EvaluateCondition(item, condition));
    }

    /// <summary>
    /// Evaluates whether an item satisfies the given condition.
    /// </summary>
    /// <param name="item">The item to evaluate.</param>
    /// <param name="condition">
    /// A condition string in the format: `Property Operator Value`.
    /// Example: `"IsActive == true"`, `"Level >= 5"`, `"Name != 'John'"`.
    /// </param>
    /// <returns><see langword="true"/> if the item matches the condition; otherwise, <see langword="false"/>.</returns>
    private static bool EvaluateCondition(object? item, string condition)
    {
        if (item == null || string.IsNullOrWhiteSpace(condition))
            return false;

        var (propertyName, op, targetValue) = ParseCondition(condition);
        if (propertyName == null)
            return false;

        var propValue = item.GetPropertyValue(propertyName);
        return CompareValues(propValue, targetValue, op);
    }

    /// <summary>
    /// Parses a condition string into its components: the property name, operator, and target value.
    /// </summary>
    /// <param name="condition">
    /// A condition string in the format: `Property Operator Value`.
    /// Example: `"Status == Active"`, `"Age >= 18"`, `"IsEnabled != false"`.
    /// </param>
    /// <returns>
    /// A tuple containing:
    /// - `propertyName` → The name of the property.
    /// - `op` → The comparison operator (`==`, `!=`, `>`, `<`, `>=`, `<=`).
    /// - `targetValue` → The target value for comparison.
    /// </returns>
    private static (string? propertyName, string? op, string? targetValue) ParseCondition(string condition)
    {
        var parts = condition.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
            return (null, null, null);

        return (parts[0], parts[1], parts[2]);
    }

    /// <summary>
    /// Compares the actual property value with the target value using the given operator.
    /// Supports numeric types, booleans, enums, and strings.
    /// </summary>
    /// <param name="actualValue">The current value of the property.</param>
    /// <param name="targetValue">The target value as a string.</param>
    /// <param name="op">The comparison operator (`==`, `!=`, `>`, `<`, `>=`, `<=`).</param>
    /// <returns>
    /// <see langword="true"/> if the comparison is valid based on the operator; otherwise, <see langword="false"/>.
    /// </returns>
    private static bool CompareValues(object? actualValue, string? targetValue, string? op)
    {
        if (actualValue == null || targetValue == null || op == null)
            return false;

        try
        {
            return op switch
            {
                "==" => actualValue.ToString() == targetValue,
                "!=" => actualValue.ToString() != targetValue,
                ">" when double.TryParse(targetValue, out var num) => System.Convert.ToDouble(actualValue) > num,
                "<" when double.TryParse(targetValue, out var num) => System.Convert.ToDouble(actualValue) < num,
                ">=" when double.TryParse(targetValue, out var num) => System.Convert.ToDouble(actualValue) >= num,
                "<=" when double.TryParse(targetValue, out var num) => System.Convert.ToDouble(actualValue) <= num,
                _ => false
            };
        }
        catch
        {
            return false;
        }
    }
}
