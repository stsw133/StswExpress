using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Checks whether a value is <see langword="null"/>, its default value, or an empty collection.
/// <br/>
/// Supported parameters:
/// - <c>null</c>: <see langword="true"/> if value is <see langword="null"/>.
/// - <c>default</c>: <see langword="true"/> if value is the default value for its type.
/// - <c>empty</c>: <see langword="true"/> if value is an empty <see cref="IEnumerable"/>.
/// <br/>
/// Prefixing a parameter with <c>!</c> negates its result.
/// If no parameter is provided, it defaults to <c>null</c> or <c>!null</c>.
/// <br/>
/// Examples:
/// - `"null"` → Returns <see langword="true"/> if value is <see langword="null"/>.
/// - `"!null"` → Returns <see langword="true"/> if value is NOT <see langword="null"/>.
/// - `"empty"` → Returns <see langword="true"/> if value is an empty collection.
/// - `"default !empty"` → Returns <see langword="true"/> if value is either default or NOT empty.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;TextBlock Visibility="{Binding SomeValue, Converter={x:Static se:StswExistenceConverter.Instance}, ConverterParameter='null'}"/&gt;
/// &lt;TextBlock Visibility="{Binding SomeValue, Converter={x:Static se:StswExistenceConverter.Instance}, ConverterParameter='!null'}"/&gt;
/// &lt;TextBlock Visibility="{Binding SomeList, Converter={x:Static se:StswExistenceConverter.Instance}, ConverterParameter='empty null'}"/&gt;
/// &lt;TextBlock Visibility="{Binding SomeNumber, Converter={x:Static se:StswExistenceConverter.Instance}, ConverterParameter='!default'}"/&gt;
/// &lt;TextBlock Visibility="{Binding SomeList, Converter={x:Static se:StswExistenceConverter.Instance}, ConverterParameter='default !empty'}"/&gt;
/// </code>
/// </example>
public class StswExistenceConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswExistenceConverter Instance => instance ??= new StswExistenceConverter();
    private static StswExistenceConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter == null)
            return targetType == typeof(Visibility)
                ? (value != null ? Visibility.Visible : Visibility.Collapsed)
                : value != null;

        var conditions = ParseConditions(parameter);
        if (conditions.Count == 0)
            conditions.Add("null", true);

        var result = conditions.Any(condition => EvaluateCondition(value, condition.Key) ^ condition.Value);
        return targetType == typeof(Visibility)
            ? (result ? Visibility.Visible : Visibility.Collapsed)
            : result;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;

    /// <summary>
    /// Parses the input parameter string into a dictionary of conditions.
    /// </summary>
    /// <param name="parameter">A space- or comma-separated string containing condition names (e.g., "null empty !default").</param>
    /// <returns>A dictionary where keys are condition names, and values indicate if they should be negated.</returns>
    private static Dictionary<string, bool> ParseConditions(object? parameter)
    {
        var conditions = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        var paramString = parameter?.ToString() ?? string.Empty;

        foreach (var part in paramString.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries))
        {
            var isNegated = part.StartsWith('!');
            var condition = isNegated ? part[1..] : part;

            if (!conditions.ContainsKey(condition))
                conditions[condition] = isNegated;
        }

        return conditions;
    }

    /// <summary>
    /// Evaluates a specific condition for the given value.
    /// </summary>
    /// <param name="value">The value to evaluate.</param>
    /// <param name="condition">The condition name (`null`, `empty`, `default`).</param>
    /// <returns><see langword="true"/> if the condition is met, otherwise <see langword="false"/>.</returns>
    private static bool EvaluateCondition(object? value, string condition)
    {
        return condition.ToLowerInvariant() switch
        {
            "null" => value is null,
            "empty" => value is string str ? string.IsNullOrEmpty(str)
                     : value is ICollection collection ? collection.Count == 0
                     : value is IEnumerable enumerable && !enumerable.Cast<object>().Any(),
            "default" => IsDefaultValue(value),
            _ => false,
        };
    }

    /// <summary>
    /// Checks if the given value is the default value for its type.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns><see langword="true"/> if the value is the default for its type, otherwise <see langword="false"/>.</returns>
    private static bool IsDefaultValue(object? value)
    {
        if (value is null)
            return true;

        var type = value.GetType();
        return type.IsValueType && Equals(value, Activator.CreateInstance(type));
    }
}
