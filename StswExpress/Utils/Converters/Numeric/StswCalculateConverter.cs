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
/// <br/>
/// Example usages:  
/// - `"*2"` (multiplies the input value by 2)  
/// - `"+5"` (adds 5 to the input value)  
/// - `"*0.5"` (scales the value by 50%)  
/// - `"*1.2,1.2,1.2,1.2"` (scales all `CornerRadius` values by 1.2)  
/// </summary>
[Stsw("0.9.0", Changes = StswPlannedChanges.None)]
public class StswCalculateConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswCalculateConverter Instance => instance ??= new StswCalculateConverter();
    private static StswCalculateConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Performs a mathematical operation on the input value.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">The type of the target property.</param>
    /// <param name="parameter">A string defining the operation (e.g., "*2" or "+10,10,10,10").</param>
    /// <param name="culture">The culture to use in the conversion.</param>
    /// <returns>The result of the mathematical operation.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter is not string pmr || pmr.Length < 2)
            return Binding.DoNothing;

        var operation = pmr[0].ToString();
        if (!StswCalculator.IsOperator(operation) && value.GetType() != typeof(DateTime))
            return Binding.DoNothing;

        var paramValues = pmr[1..].Split([' ', ','], StringSplitOptions.RemoveEmptyEntries)
                                  .Select(p => double.TryParse(p, NumberStyles.Number, culture, out var num) ? num : (double?)null)
                                  .Where(p => p.HasValue)
                                  .Select(p => p!.Value)
                                  .ToArray();

        return value switch
        {
            CornerRadius cr => ApplyCornerRadiusOperation(cr, operation, paramValues),
            DateTime dt => StswCalculator.ApplyOperator(operation, dt, (int)paramValues.FirstOrDefault()).ConvertTo(targetType),
            GridLength gl => ApplyGridLengthOperation(gl, operation, paramValues.FirstOrDefault()),
            Thickness th => ApplyThicknessOperation(th, operation, paramValues),
            double d when targetType == typeof(CornerRadius) => new CornerRadius(StswCalculator.ApplyOperator(operation, d, paramValues.FirstOrDefault())),
            double d when targetType == typeof(Thickness) => new Thickness(StswCalculator.ApplyOperator(operation, d, paramValues.FirstOrDefault())),
            double d when targetType == typeof(GridLength) => new GridLength(StswCalculator.ApplyOperator(operation, d, paramValues.FirstOrDefault())),
            _ when paramValues.Length > 0 => Math.Round(StswCalculator.ApplyOperator(operation, System.Convert.ToDouble(value, culture), paramValues[0]), 10).ConvertTo(targetType),
            _ => value.ConvertTo(targetType)
        };
    }

    /// <summary>
    /// This converter does not support converting back from target value to source value.
    /// </summary>
    /// <param name="value">The value produced by the binding target.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns><see cref="Binding.DoNothing"/> as the converter does not support converting back.</returns>
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
                StswCalculator.ApplyOperator(operation, cr.TopLeft, parameters[0]),
                StswCalculator.ApplyOperator(operation, cr.TopRight, parameters[1]),
                StswCalculator.ApplyOperator(operation, cr.BottomRight, parameters[2]),
                StswCalculator.ApplyOperator(operation, cr.BottomLeft, parameters[3])),
            2 => new CornerRadius(
                StswCalculator.ApplyOperator(operation, cr.TopLeft, parameters[0]),
                StswCalculator.ApplyOperator(operation, cr.TopRight, parameters[1]),
                StswCalculator.ApplyOperator(operation, cr.BottomRight, parameters[0]),
                StswCalculator.ApplyOperator(operation, cr.BottomLeft, parameters[1])),
            1 => new CornerRadius(StswCalculator.ApplyOperator(operation, cr.TopLeft, parameters[0])),
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

        return new GridLength(StswCalculator.ApplyOperator(operation, gl.Value, parameter), gl.GridUnitType);
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
                StswCalculator.ApplyOperator(operation, th.Left, parameters[0]),
                StswCalculator.ApplyOperator(operation, th.Top, parameters[1]),
                StswCalculator.ApplyOperator(operation, th.Right, parameters[2]),
                StswCalculator.ApplyOperator(operation, th.Bottom, parameters[3])),
            2 => new Thickness(
                StswCalculator.ApplyOperator(operation, th.Left, parameters[0]),
                StswCalculator.ApplyOperator(operation, th.Top, parameters[1]),
                StswCalculator.ApplyOperator(operation, th.Right, parameters[0]),
                StswCalculator.ApplyOperator(operation, th.Bottom, parameters[1])),
            1 => new Thickness(StswCalculator.ApplyOperator(operation, th.Left, parameters[0])),
            _ => th
        };
}

/*

<Border BorderThickness="{Binding MarginValue, Converter={x:Static se:StswCalculateConverter.Instance}, ConverterParameter='*1.2'}"/>

<Button CornerRadius="{Binding CornerSize, Converter={x:Static se:StswCalculateConverter.Instance}, ConverterParameter='+5'}"/>

<RowDefinition Height="{Binding RowHeight, Converter={x:Static se:StswCalculateConverter.Instance}, ConverterParameter='*1.5'}"/>

*/
