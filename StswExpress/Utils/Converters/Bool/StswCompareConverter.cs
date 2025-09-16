using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Compares a numeric, string, or enum value against a condition specified via parameter.
/// Supported operator prefixes: ">", ">=", "<", "<=", "=", "!", "&" (bitwise AND), "@" (case-insensitive equals).
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
        if (parameter is not string rawParam || rawParam.Length == 0)
            return Binding.DoNothing;

        var (op, rhs) = ParseOperatorAndRhs(rawParam);
        var result = false;

        // --- ENUMS ---
        if (value is Enum enumVal)
        {
            if (op == "&")
            {
                if (TryParseEnum(enumVal.GetType(), rhs, out var rhsEnumObj))
                {
                    var lhsInt = System.Convert.ToInt64(enumVal, culture);
                    var rhsInt = System.Convert.ToInt64(rhsEnumObj!, culture);
                    result = (lhsInt & rhsInt) != 0;
                }
                else if (long.TryParse(rhs, NumberStyles.Integer, culture, out var rhsNum))
                {
                    var lhsInt = System.Convert.ToInt64(enumVal, culture);
                    result = (lhsInt & rhsNum) != 0;
                }
            }
            else
            {
                // '=' by default when op == ""
                var isNot = op == "!";
                if (TryParseEnum(enumVal.GetType(), rhs, out var rhsEnum))
                {
                    result = isNot ? !Equals(enumVal, rhsEnum) : Equals(enumVal, rhsEnum);
                }
                else
                {
                    var name = enumVal.ToString() ?? string.Empty;
                    result = isNot
                        ? !string.Equals(name, rhs, StringComparison.Ordinal)
                        : string.Equals(name, rhs, StringComparison.Ordinal);
                }
            }
        }
        // --- NUMERICS ---
        else if (TryToDouble(value, culture, out var valNum))
        {
            switch (op)
            {
                case "":
                case "=":
                case "!":
                case ">":
                case "<":
                case ">=":
                case "<=":
                    {
                        if (!double.TryParse(rhs, NumberStyles.Number, culture, out var rhsNum))
                            return Binding.DoNothing;

                        result = op switch
                        {
                            ">" => valNum > rhsNum,
                            ">=" => valNum >= rhsNum,
                            "<" => valNum < rhsNum,
                            "<=" => valNum <= rhsNum,
                            "!" => Math.Abs(valNum - rhsNum) > double.Epsilon,
                            _ => Math.Abs(valNum - rhsNum) <= double.Epsilon // default '='
                        };
                        break;
                    }

                case "&":
                    {
                        if (double.TryParse(rhs, NumberStyles.Number, culture, out var rhsNum))
                            result = (((int)valNum) & ((int)rhsNum)) != 0;
                        break;
                    }

                default:
                    // '@' (case-insensitive) has no numeric meaning, etc.
                    return Binding.DoNothing;
            }
        }
        // --- STRINGS ---
        else
        {
            var input = value?.ToString() ?? string.Empty;

            if (op == "")
            {
                // no operator -> treat as '=' against the full parameter text
                result = string.Equals(input, rawParam, StringComparison.Ordinal);
            }
            else
            {
                result = op switch
                {
                    "=" => string.Equals(input, rhs, StringComparison.Ordinal),
                    "!" => !string.Equals(input, rhs, StringComparison.Ordinal),
                    "@" => string.Equals(input, rhs, StringComparison.OrdinalIgnoreCase),
                    "&" => false, // no string meaning
                    ">" or "<" or ">=" or "<=" => false, // no string meaning
                    _ => string.Equals(input, parameter?.ToString() ?? string.Empty, StringComparison.Ordinal)
                };
            }
        }

        return targetType == typeof(Visibility)
            ? (result ? Visibility.Visible : Visibility.Collapsed)
            : result.ConvertTo(targetType);
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;

    /// <summary>
    /// Parses operator and RHS. Recognizes: ">=", "<=", ">", "<", "=", "!", "&", "@".
    /// If no operator is found, returns ("", raw).
    /// For example:
    ///   "==test" -> ("=", "=test")
    ///   "Admin"  -> ("", "Admin")
    /// </summary>
    [StswInfo("0.20.1")]
    private static (string op, string rhs) ParseOperatorAndRhs(string s)
    {
        if (s.Length >= 2 && ((s[0] == '>' && s[1] == '=') || (s[0] == '<' && s[1] == '=')))
            return (s[..2], s[2..]);

        if (s.Length >= 1 && (s[0] is '>' or '<' or '=' or '!' or '&' or '@'))
            return (s[..1], s[1..]);

        return ("", s); // default '='
    }

    /// <summary>
    /// Tries to convert the given value to double, using the specified culture.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="culture">The culture to use for conversion.</param>
    /// <param name="number">When this method returns, contains the double value equivalent of the input value, if the conversion succeeded, or zero if the conversion failed.</param>
    /// <returns><see langword="true"/> if conversion succeeded; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.20.1")]
    private static bool TryToDouble(object? value, CultureInfo culture, out double number)
    {
        if (value is null)
        {
            number = default;
            return false;
        }

        if (value is IConvertible)
        {
            try
            {
                number = System.Convert.ToDouble(value, culture);
                return true;
            }
            catch { /* fall through */ }
        }

        return double.TryParse(value.ToString(), NumberStyles.Number, culture, out number);
    }

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
        // numeric underlying value
        if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var asNumber))
        {
            enumObj = Enum.ToObject(enumType, asNumber);
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
