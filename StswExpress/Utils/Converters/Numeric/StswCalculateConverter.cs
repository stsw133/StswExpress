using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A value converter that performs mathematical operations on the input value based on the provided parameter.
/// <br/>
/// - Supports operations: `+`, `-`, `*`, `/`, `%`, `^`.  
/// - Supports numeric types (`int`, `double`), as well as `CornerRadius`, `Thickness`, and `GridLength`.  
/// - The parameter must start with the operator (`+`, `-`, etc.) followed by a number or multiple values (for `CornerRadius` and `Thickness`). 
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;Border BorderThickness="{Binding MarginValue, Converter={x:Static se:StswCalculateConverter.Instance}, ConverterParameter='*1.2'}"/&gt;
/// &lt;Button CornerRadius="{Binding CornerSize, Converter={x:Static se:StswCalculateConverter.Instance}, ConverterParameter='+5'}"/&gt;
/// &lt;RowDefinition Height="{Binding RowHeight, Converter={x:Static se:StswCalculateConverter.Instance}, ConverterParameter='*1.5'}"/&gt;
/// &lt;Border CornerRadius="{Binding CornerSize, Converter={x:Static se:StswCalculateConverter.Instance}, ConverterParameter='*1.2,1.2,1.2,1.2'}"/&gt;
/// </code>
/// </example>
public class StswCalculateConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswCalculateConverter Instance => instance ??= new StswCalculateConverter();
    private static StswCalculateConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter is not string pmr || pmr.Length < 2)
            return Binding.DoNothing;

        var operation = pmr[0].ToString();
        if (!IsOperator(operation) && value.GetType() != typeof(DateTime))
            return Binding.DoNothing;

        var paramValues = pmr[1..].Split([' ', ','], StringSplitOptions.RemoveEmptyEntries)
                                  .Select(p => double.TryParse(p, NumberStyles.Number, culture, out var num) ? num : (double?)null)
                                  .Where(p => p.HasValue)
                                  .Select(p => p!.Value)
                                  .ToArray();

        return value switch
        {
            CornerRadius cr => ApplyCornerRadiusOperation(cr, operation, paramValues),
            DateTime dt => ApplyOperator(operation, dt, (int)paramValues.FirstOrDefault()).ConvertTo(targetType),
            GridLength gl => ApplyGridLengthOperation(gl, operation, paramValues.FirstOrDefault()),
            Thickness th => ApplyThicknessOperation(th, operation, paramValues),
            double d when targetType == typeof(CornerRadius) => new CornerRadius(ApplyOperator(operation, d, paramValues.FirstOrDefault())),
            double d when targetType == typeof(Thickness) => new Thickness(ApplyOperator(operation, d, paramValues.FirstOrDefault())),
            double d when targetType == typeof(GridLength) => new GridLength(ApplyOperator(operation, d, paramValues.FirstOrDefault())),
            _ when paramValues.Length > 0 => Math.Round(ApplyOperator(operation, System.Convert.ToDouble(value, culture), paramValues[0]), 10).ConvertTo(targetType),
            _ => value.ConvertTo(targetType)
        };
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;

    /// <summary>
    /// Applies a mathematical operation to a <see cref="CornerRadius"/> value.
    /// </summary>
    /// <param name="value">The value to be converted. It must be of type <see cref="CornerRadius"/>.</param>
    /// <param name="operation">The mathematical operation to perform (e.g., "+", "-", "*", "/").</param>
    /// <param name="parameters">An array of numbers used in the operation.</param>
    /// <returns>A new <see cref="CornerRadius"/> with the applied operation, or the original value if invalid.</returns>
    private static CornerRadius ApplyCornerRadiusOperation(CornerRadius cr, string operation, double[] parameters)
        => parameters.Length switch
        {
            4 => new CornerRadius(
                ApplyOperator(operation, cr.TopLeft, parameters[0]),
                ApplyOperator(operation, cr.TopRight, parameters[1]),
                ApplyOperator(operation, cr.BottomRight, parameters[2]),
                ApplyOperator(operation, cr.BottomLeft, parameters[3])),
            2 => new CornerRadius(
                ApplyOperator(operation, cr.TopLeft, parameters[0]),
                ApplyOperator(operation, cr.TopRight, parameters[1]),
                ApplyOperator(operation, cr.BottomRight, parameters[0]),
                ApplyOperator(operation, cr.BottomLeft, parameters[1])),
            1 => new CornerRadius(ApplyOperator(operation, cr.TopLeft, parameters[0])),
            _ => cr
        };

    /// <summary>
    /// Applies a mathematical operation to a <see cref="GridLength"/> value.
    /// </summary>
    /// <param name="value">The value to be converted. It must be of type <see cref="GridLength"/>.</param>
    /// <param name="operation">The mathematical operation to perform (e.g., "+", "-", "*", "/").</param>
    /// <param name="parameter">The number used in the operation.</param>
    /// <returns>A new <see cref="GridLength"/> with the applied operation, or the original value if invalid.</returns>
    private static GridLength ApplyGridLengthOperation(GridLength gl, string operation, double parameter)
    {
        if (gl.IsAuto || gl.IsStar)
            return gl;

        return new GridLength(ApplyOperator(operation, gl.Value, parameter), gl.GridUnitType);
    }

    /// <summary>
    /// Applies a mathematical operation to a <see cref="Thickness"/> value.
    /// </summary>
    /// <param name="value">The value to be converted. It must be of type <see cref="Thickness"/>.</param>
    /// <param name="operation">The mathematical operation to perform (e.g., "+", "-", "*", "/").</param>
    /// <param name="parameters">An array of numbers used in the operation.</param>
    /// <returns>A new <see cref="Thickness"/> with the applied operation, or the original value if invalid.</returns>
    private static Thickness ApplyThicknessOperation(Thickness th, string operation, double[] parameters)
        => parameters.Length switch
        {
            4 => new Thickness(
                ApplyOperator(operation, th.Left, parameters[0]),
                ApplyOperator(operation, th.Top, parameters[1]),
                ApplyOperator(operation, th.Right, parameters[2]),
                ApplyOperator(operation, th.Bottom, parameters[3])),
            2 => new Thickness(
                ApplyOperator(operation, th.Left, parameters[0]),
                ApplyOperator(operation, th.Top, parameters[1]),
                ApplyOperator(operation, th.Right, parameters[0]),
                ApplyOperator(operation, th.Bottom, parameters[1])),
            1 => new Thickness(ApplyOperator(operation, th.Left, parameters[0])),
            _ => th
        };

    /// <summary>
    /// Checks if a given token is an operator.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns><see langword="true"/> if the token is an operator; otherwise, <see langword="false"/>.</returns>
    public static bool IsOperator(string token) => token is "+" or "-" or "*" or "/" or "^" or "%";

    /// <summary>
    /// Applies a given operator to two operands and returns the result.
    /// </summary>
    /// <param name="op">The operator to apply.</param>
    /// <param name="a">The first operand.</param>
    /// <param name="b">The second operand.</param>
    /// <returns>The result of the operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the operator is unsupported.</exception>
    public static double ApplyOperator(string op, double a, double b) => op switch
    {
        "+" => a + b,
        "-" => a - b,
        "*" => a * b,
        "/" => a / b,
        "^" => Math.Pow(a, b),
        "%" => a % b,
        _ => throw new InvalidOperationException($"Unsupported operator: {op}"),
    };

    /// <summary>
    /// Applies a given operator to a DateTime and an integer, returning the resulting DateTime.
    /// </summary>
    /// <param name="op">The operator to apply. Supported operators are "y" (years), "M" (months), "d" (days), "H" (hours), "m" (minutes), "s" (seconds), "+" (add days), and "-" (subtract days).</param>
    /// <param name="a">The DateTime operand.</param>
    /// <param name="b">The integer operand.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static DateTime ApplyOperator(string op, DateTime a, int b) => op switch
    {
        "y" => a.AddYears(b),
        "M" => a.AddMonths(b),
        "d" => a.AddDays(b),
        "H" => a.AddHours(b),
        "m" => a.AddMinutes(b),
        "s" => a.AddSeconds(b),
        "+" => a.AddDays(b),
        "-" => a.AddDays(-b),
        _ => throw new InvalidOperationException($"Unsupported operator: {op}"),
    };
}
