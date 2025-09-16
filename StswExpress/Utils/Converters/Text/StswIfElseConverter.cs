using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A value converter that conditionally returns different values based on an input condition.
/// Supports two syntaxes:
/// 1) Tilde chain: cond1~then1~cond2~then2~...~else
/// 2) Ternary chain: cond1 ? then1 : cond2 ? then2 : else
/// Conditions can use '||' (OR), e.g. "admin||owner".
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;TextBlock Text="{Binding UserRole, Converter={x:Static se:StswIfElseConverter.Instance}, ConverterParameter='Admin~Yes~Editor~Partially~No'}"/&gt;
/// &lt;TextBlock Text="{Binding UserRole, Converter={x:Static se:StswIfElseConverter.Instance}, ConverterParameter='Admin ? Yes : Editor ? Partially : No'}"/&gt;
/// &lt;TextBlock Visibility="{Binding Role, Converter={x:Static se:StswIfElseConverter.Instance}, ConverterParameter='Admin||Owner ? Visible : Collapsed'}"/&gt;
/// </code>
/// </example>
[StswInfo(null, "0.20.0")]
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
        if (parameter == null)
            return null;

        var param = parameter.ToString()!;
        var val = value?.ToString() ?? string.Empty;

        if (param.Contains(" ? ") && param.Contains(" : "))
        {
            var result = EvaluateTernaryChain(param, val);
            return result;
        }
        else
        {
            var result = EvaluateTildeChain(param, val);
            return result;
        }
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;

    /// <summary>
    /// Parses and evaluates a tilde-separated chain of conditions and values.
    /// </summary>
    /// <param name="param">The parameter string containing conditions and corresponding values separated by tildes (`~`).</param>
    /// <param name="input">The input value to evaluate against the conditions.</param>
    /// <returns>The evaluated value based on the input condition, or <see langword="null"/> if no conditions matched and no else clause was provided.</returns>
    private static string? EvaluateTildeChain(string param, string input)
    {
        var parts = param.Split('~');

        for (var i = 0; i + 1 < parts.Length; i += 2)
        {
            var cond = parts[i].Trim();
            var thenVal = parts[i + 1].Trim();
            if (EvaluateCondition(cond, input))
                return thenVal;
        }

        if (parts.Length % 2 == 1)
            return parts[^1].Trim();

        return null;
    }

    /// <summary>
    /// Parses and evaluates a ternary chain of conditions and values.
    /// </summary>
    /// <param name="expr">The expression string containing conditions and corresponding values using ternary operators (`?` and `:`).</param>
    /// <param name="input">The input value to evaluate against the conditions.</param>
    /// <returns>The evaluated value based on the input condition, or <see langword="null"/> if no conditions matched and no else clause was provided.</returns>
    private static string? EvaluateTernaryChain(string expr, string input)
    {
        expr = expr.Trim();

        var qIndex = IndexOfTopLevelQuestion(expr);
        if (qIndex < 0)
            return expr.Trim();

        var colonIndex = FindMatchingColon(expr, qIndex);
        if (colonIndex < 0)
            return expr.Trim();

        var cond = expr[..qIndex].Trim();
        var thenPart = expr.Substring(qIndex + 1, colonIndex - qIndex - 1).Trim();
        var elsePart = expr[(colonIndex + 1)..].Trim();

        if (EvaluateCondition(cond, input))
            return EvaluateTernaryChain(thenPart, input);
        else
            return EvaluateTernaryChain(elsePart, input);
    }

    /// <summary>
    /// Finds the index of the top-level '?' in a ternary expression, ignoring nested ternary operators.
    /// </summary>
    /// <param name="s">The input string containing the ternary expression.</param>
    /// <returns>The index of the top-level '?', or -1 if not found.</returns>
    private static int IndexOfTopLevelQuestion(string s)
    {
        var depth = 0;
        for (var i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (c == '?')
            {
                if (depth == 0) return i;
                depth++;
            }
            else if (c == ':')
            {
                if (depth > 0) depth--;
            }
        }
        return -1;
    }

    /// <summary>
    /// Finds the index of the matching ':' for a '?' in a ternary expression, ignoring nested ternary operators.
    /// </summary>
    /// <param name="s">The input string containing the ternary expression.</param>
    /// <param name="questionIndex">The index of the '?' character to find the matching ':' for.</param>
    /// <returns>The index of the matching ':', or -1 if not found.</returns>
    private static int FindMatchingColon(string s, int questionIndex)
    {
        var depth = 0;
        for (var i = questionIndex + 1; i < s.Length; i++)
        {
            char c = s[i];
            if (c == '?') depth++;
            else if (c == ':')
            {
                if (depth == 0) return i;
                depth--;
            }
        }
        return -1;
    }

    /// <summary>
    /// Evaluates a condition string against the input value.
    /// </summary>
    /// <param name="condition">The condition string, which may contain '||' (OR).</param>
    /// <param name="input">The input value to evaluate against the condition.</param>
    /// <returns><see langword="true"/> if the condition matches the input; otherwise, <see langword="false"/>.</returns>
    private static bool EvaluateCondition(string condition, string input)
    {
        var orTerms = condition.Split(["||"], StringSplitOptions.None);
        foreach (var term in orTerms)
        {
            var atom = term.Trim();
            if (atom.Length == 0) continue;

            if (string.Equals(input, atom, StringComparison.Ordinal))
                return true;
        }
        return false;
    }
}
