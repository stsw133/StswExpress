using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Compares a numeric, string, or enum value against a condition specified via parameter.
/// Supported operator prefixes: "&gt;", "&gt;=", "&lt;", "&lt;=", "=", "!", "&amp;" (bitwise AND), "@" (case-insensitive equals).
/// If no operator is provided, "=" is assumed (e.g., "Admin" ≡ "=Admin").
/// To compare a literal that starts with an operator, prefix it with "=" (e.g., "==test" compares to "=test").
/// When target type is <see cref="Visibility"/>, true -> Visible, false -> Collapsed; otherwise returns bool (converted to targetType if possible).
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;TextBlock Text="Only for small values" Visibility="{Binding SomeNumber, Converter={x:Static se:StswCompareConverter.Instance}, ConverterParameter='&lt;=10'}"/&gt;
/// &lt;Button Content="Administration panel" Visibility="{Binding UserRole, Converter={x:Static se:StswCompareConverter.Instance}, ConverterParameter='=Admin'}"/&gt;
/// &lt;Button Content="Delete" Visibility="{Binding UserRole, Converter={x:Static se:StswCompareConverter.Instance}, ConverterParameter='!Guest'}"/&gt;
/// &lt;Button Content="Advanced options" Visibility="{Binding UserPermissions, Converter={x:Static se:StswCompareConverter.Instance}, ConverterParameter='&amp;2'}"/&gt;
/// </code>
/// </example>
[StswInfo(null, "0.20.1")]
public class StswCompareConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswCompareConverter Instance => instance ??= new StswCompareConverter();
    private static StswCompareConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <inheritdoc/>
    [StswInfo(null, "0.20.1")]
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        /// Fast path: enum parameter passed via {x:Static ...}
        if (parameter is Enum enumParam)
        {
            bool eq;
            if (value is Enum enumVal)
            {
                eq = Equals(enumVal, enumParam);
            }
            else if (value is IConvertible)
            {
                try
                {
                    var lhs = System.Convert.ToInt64(value, culture);
                    var rhs = System.Convert.ToInt64(enumParam, culture);
                    eq = lhs == rhs;
                }
                catch
                {
                    eq = string.Equals(value?.ToString() ?? string.Empty, enumParam.ToString(), StringComparison.Ordinal);
                }
            }
            else
            {
                eq = string.Equals(value?.ToString() ?? string.Empty, enumParam.ToString(), StringComparison.Ordinal);
            }

            return targetType == typeof(Visibility)
                ? (eq ? Visibility.Visible : Visibility.Collapsed)
                : eq.ConvertTo(targetType);
        }

        /// Slow path: string parameter
        if (parameter is not string raw || raw.Length == 0)
            return Binding.DoNothing;

        // Normalize operator + RHS:
        // - "==x" → op '=' and rhs "=x" (literal compare to "=x")
        // - no operator → op '=' and rhs raw
        char op;
        ReadOnlySpan<char> rhsSpan;

        if (raw.Length >= 2 && raw[0] == '=' && raw[1] == '=')
        {
            op = '=';
            rhsSpan = raw.AsSpan(1);
        }
        else if (raw.Length >= 2 && ((raw[0] == '>' || raw[0] == '<') && raw[1] == '='))
        {
            op = raw[0];
            rhsSpan = raw.AsSpan(2);
        }
        else if (raw.Length >= 1 && (raw[0] is '>' or '<' or '=' or '!' or '&' or '@'))
        {
            op = raw[0];
            rhsSpan = raw.AsSpan(1);
        }
        else
        {
            op = '=';
            rhsSpan = raw.AsSpan();
        }

        var result = false;
        var inputStr = value?.ToString() ?? string.Empty;

        /// Enum comparison (=, !, &)
        if (value is Enum enumValStr)
        {
            var et = enumValStr.GetType();

            if (op == '&')
            {
                if (TryParseEnum(et, rhsSpan.ToString(), out var rhsEnumObj))
                {
                    var lhs = System.Convert.ToInt64(enumValStr, culture);
                    var rhs = System.Convert.ToInt64(rhsEnumObj!, culture);
                    result = (lhs & rhs) != 0;
                }
                else if (long.TryParse(rhsSpan, NumberStyles.Integer, culture, out var rhsNum))
                {
                    var lhs = System.Convert.ToInt64(enumValStr, culture);
                    result = (lhs & rhsNum) != 0;
                }
            }
            else
            {
                var negate = op == '!';
                if (TryParseEnum(et, rhsSpan.ToString(), out var rhsEnum))
                {
                    var eq = Equals(enumValStr, rhsEnum);
                    result = negate ? !eq : eq;
                }
                else
                {
                    var name = enumValStr.ToString() ?? string.Empty;
                    var eq = string.Equals(name, rhsSpan.ToString(), StringComparison.Ordinal);
                    result = negate ? !eq : eq;
                }
            }
        }
        /// Number comparison (>, >=, <, <=, =, !, &)
        else if (double.TryParse(inputStr, NumberStyles.Number, culture, out var valNum))
        {
            if (op == '&')
            {
                if (double.TryParse(rhsSpan, NumberStyles.Number, culture, out var rhsNumAnd))
                    result = (((int)valNum) & ((int)rhsNumAnd)) != 0;
            }
            else
            {
                if (!double.TryParse(rhsSpan, NumberStyles.Number, culture, out var rhsNum))
                    return Binding.DoNothing;

                result = op switch
                {
                    '>' when raw.Length > 1 && raw[1] == '=' => valNum >= rhsNum,
                    '>' => valNum > rhsNum,
                    '<' when raw.Length > 1 && raw[1] == '=' => valNum <= rhsNum,
                    '<' => valNum < rhsNum,
                    '!' => Math.Abs(valNum - rhsNum) > double.Epsilon,
                    '=' => Math.Abs(valNum - rhsNum) <= double.Epsilon,
                    '\0' => Math.Abs(valNum - rhsNum) <= double.Epsilon,
                    _ => result
                };
            }
        }
        /// String comparison (=, !, @) or default equality
        else
        {
            var rhsStr = rhsSpan.ToString();
            result = op switch
            {
                '=' => inputStr.Equals(rhsStr, StringComparison.Ordinal),
                '!' => !inputStr.Equals(rhsStr, StringComparison.Ordinal),
                '@' => inputStr.Equals(rhsStr, StringComparison.OrdinalIgnoreCase),
                _ => inputStr == raw
            };
        }

        return targetType == typeof(Visibility)
            ? (result ? Visibility.Visible : Visibility.Collapsed)
            : result.ConvertTo(targetType);
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;

    /// <summary>
    /// Tries to parse the given text as an enum of the specified type.
    /// </summary>
    /// <param name="enumType">The type of the enum.</param>
    /// <param name="text">The text to parse.</param>
    /// <param name="enumObj">When this method returns, contains the enum value equivalent of the input text, if the parsing succeeded, or <see langword="null"/> if the parsing failed.</param>
    /// <returns><see langword="true"/> if parsing succeeded; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.20.1")]
    private static bool TryParseEnum(Type enumType, string text, out object? enumObj)
    {
        if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
        {
            enumObj = Enum.ToObject(enumType, n);
            return true;
        }

        try
        {
            enumObj = Enum.Parse(enumType, text, ignoreCase: true);
            return true;
        }
        catch
        {
            enumObj = null;
            return false;
        }
    }
}
