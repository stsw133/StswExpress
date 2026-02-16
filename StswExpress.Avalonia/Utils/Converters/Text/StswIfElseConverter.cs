using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using System.ComponentModel;
using System.Globalization;

namespace StswExpress.Avalonia;
/// <summary>
/// A value converter that conditionally returns different values based on an input condition.
/// Supports tilde-separated chains using the syntax: cond1~then1~cond2~then2~...~else.
/// Conditions can use '||' (OR), e.g. "admin||owner", and support `{x:Null}` and empty string checks.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;TextBlock Text="{Binding UserRole, Converter={x:Static se:StswIfElseConverter.Instance}, ConverterParameter='Admin~Yes~Editor~Partially~No'}"/&gt;
/// &lt;TextBlock Text="{Binding Status, Converter={x:Static se:StswIfElseConverter.Instance}, ConverterParameter='{x:Null}~Unset~~Ready~In progress'}"/&gt;
/// &lt;TextBlock Visibility="{Binding Role, Converter={x:Static se:StswIfElseConverter.Instance}, ConverterParameter='Admin||Owner~Visible~Collapsed'}"/&gt;
/// </code>
/// </example>
public class StswIfElseConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswIfElseConverter Instance => _instance ??= new StswIfElseConverter();
    private static StswIfElseConverter? _instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        targetType ??= typeof(object);

        if (parameter == null)
            return GetDefaultValue(targetType);

        var param = parameter.ToString();
        if (string.IsNullOrEmpty(param))
            return GetDefaultValue(targetType);

        var val = value?.ToString();

        var result = EvaluateTildeChain(param!, value, val);
        return ConvertTokenToTarget(result, targetType, culture);
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => BindingOperations.DoNothing;

    /// <summary>
    /// Parses and evaluates a tilde-separated chain of conditions and values.
    /// </summary>
    /// <param name="param">The parameter string containing conditions and corresponding values separated by tildes (`~`).</param>
    /// <param name="rawValue">The original value provided to the converter.</param>
    /// <param name="valueText">The string representation of the value.</param>
    /// <returns>The evaluated token based on the input condition, or <see langword="null"/> if no conditions matched and no else clause was provided.</returns>
    private static string? EvaluateTildeChain(string param, object? rawValue, string? valueText)
    {
        var parts = param.Split('~');

        for (var i = 0; i + 1 < parts.Length; i += 2)
        {
            var cond = parts[i].Trim();
            var thenVal = parts[i + 1];
            if (EvaluateCondition(cond, rawValue, valueText))
                return thenVal;
        }

        if (parts.Length % 2 == 1)
            return parts[^1];

        return null;
    }

    /// <summary>
    /// Evaluates a condition string against the input value.
    /// </summary>
    /// <param name="condition">The condition string, which may contain '||' (OR) terms.</param>
    /// <param name="rawValue">The original value provided to the converter.</param>
    /// <param name="valueText">The string representation of the value.</param>
    /// <returns><see langword="true"/> if the condition matches the input; otherwise, <see langword="false"/>.</returns>
    private static bool EvaluateCondition(string condition, object? rawValue, string? valueText)
    {
        var orTerms = condition.Split(["||"], StringSplitOptions.None);
        foreach (var term in orTerms)
        {
            var atom = term.Trim();

            if (IsNullLiteral(atom))
            {
                if (rawValue is null)
                    return true;
                continue;
            }

            if (atom.Length == 0)
            {
                if (rawValue is string stringValue && stringValue.Length == 0)
                    return true;
                if (valueText != null && valueText.Length == 0)
                    return true;
                continue;
            }

            if (rawValue is string stringRaw)
            {
                if (string.Equals(stringRaw, atom, StringComparison.Ordinal))
                    return true;
            }
            else if (string.Equals(valueText, atom, StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Converts the given token string to the specified target type.
    /// </summary>
    /// <param name="token">The token string to convert.</param>
    /// <param name="targetType">The target type to convert to.</param>
    /// <param name="culture">The culture info for conversion.</param>
    /// <returns>The converted object.</returns>
    private static object? ConvertTokenToTarget(string? token, Type targetType, CultureInfo culture)
    {
        targetType ??= typeof(object);

        if (token is null)
            return GetDefaultValue(targetType);

        var trimmed = token.Trim();

        if (trimmed.Length == 0 && targetType == typeof(string))
            return string.Empty;

        if (IsNullLiteral(trimmed))
            return null;

        if (targetType == typeof(string) || targetType == typeof(object))
            return trimmed;

        var converter = TypeDescriptor.GetConverter(targetType);
        if (converter?.CanConvertFrom(typeof(string)) == true)
            return converter.ConvertFrom(null, culture, trimmed);

        return trimmed;
    }

    /// <summary>
    /// Gets the default value for the specified target type.
    /// </summary>
    /// <param name="targetType">The target type.</param>
    /// <returns>The default value for the target type.</returns>
    private static object? GetDefaultValue(Type targetType)
    {
        if (!targetType.IsValueType)
            return null;

        return Activator.CreateInstance(targetType);
    }

    /// <summary>
    /// Determines if the given string represents a null literal (`{x:Null}`).
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns><see langword="true"/> if the string is a null literal; otherwise, <see langword="false"/>.</returns>
    private static bool IsNullLiteral(string value) => string.Equals(value, "{x:Null}", StringComparison.OrdinalIgnoreCase);
}
