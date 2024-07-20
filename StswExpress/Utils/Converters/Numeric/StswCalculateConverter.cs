using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Performs a mathematical operation on the value parameter based on the converter parameter.
/// Supports <see cref="CornerRadius"/>, <see cref="GridLength"/>, <see cref="Thickness"/> and any numeric type (for example <see cref="int"/> and <see cref="double"/>).
/// Supported operations: '+', '-', '*', '/', '%', '^'.
/// </summary>
public class StswCalculateConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswCalculateConverter Instance => instance ??= new StswCalculateConverter();
    private static StswCalculateConverter? instance;

    /// <summary>
    /// Provides the singleton instance of the converter.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The singleton instance of the converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Performs a mathematical operation on the value parameter based on the converter parameter.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use for the operation.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>The result of the mathematical operation.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var input = value?.ToString() ?? string.Empty;
        var pmr = parameter?.ToString() ?? string.Empty;

        if (pmr.Length < 2 || !StswCalculator.IsOperator(pmr[0].ToString()))
            return value;

        if (targetType.In(typeof(CornerRadius), typeof(CornerRadius?)))
            return HandleCornerRadius(value, pmr[0].ToString(), pmr[1..], culture);
        else if (targetType.In(typeof(Thickness), typeof(Thickness?)))
            return HandleThickness(value, pmr[0].ToString(), pmr[1..], culture);
        else if (targetType.In(typeof(GridLength), typeof(GridLength?)))
            return HandleGridLength(value, pmr[0].ToString(), pmr[1..], culture);
        else if (double.TryParse(input, NumberStyles.Number, culture, out var val) && double.TryParse(pmr[1..], NumberStyles.Number, culture, out var num))
            return Math.Round(StswCalculator.ApplyOperator(pmr[0].ToString(), val, num), 10).ConvertTo(targetType);
        else
            return value;
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
    /// Handles mathematical operations for CornerRadius type.
    /// </summary>
    /// <param name="value">The value to be converted.</param>
    /// <param name="operation">The mathematical operation to perform.</param>
    /// <param name="parameters">The parameters for the operation.</param>
    /// <param name="culture">The culture to use for parsing.</param>
    /// <returns>A new CornerRadius with the applied operation.</returns>
    private static object HandleCornerRadius(object? value, string operation, string parameters, CultureInfo culture)
    {
        var pmrArray = Array.ConvertAll(parameters.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries), i => System.Convert.ToDouble(i, culture));
        if (value is CornerRadius val1)
        {
            return pmrArray.Length switch
            {
                4 => new CornerRadius(
                    StswCalculator.ApplyOperator(operation, val1.TopLeft, pmrArray[0]),
                    StswCalculator.ApplyOperator(operation, val1.TopRight, pmrArray[1]),
                    StswCalculator.ApplyOperator(operation, val1.BottomRight, pmrArray[2]),
                    StswCalculator.ApplyOperator(operation, val1.BottomLeft, pmrArray[3])
                ),
                2 => new CornerRadius(
                    StswCalculator.ApplyOperator(operation, val1.TopLeft, pmrArray[0]),
                    StswCalculator.ApplyOperator(operation, val1.TopRight, pmrArray[1]),
                    StswCalculator.ApplyOperator(operation, val1.BottomRight, pmrArray[0]),
                    StswCalculator.ApplyOperator(operation, val1.BottomLeft, pmrArray[1])
                ),
                1 => new CornerRadius(StswCalculator.ApplyOperator(operation, val1.TopLeft, pmrArray[0])),
                _ => new CornerRadius(val1.TopLeft)
            };
        }
        else
        {
            var val2 = System.Convert.ToDouble(value, culture);
            return pmrArray.Length switch
            {
                4 => new CornerRadius(
                    StswCalculator.ApplyOperator(operation, val2, pmrArray[0]),
                    StswCalculator.ApplyOperator(operation, val2, pmrArray[1]),
                    StswCalculator.ApplyOperator(operation, val2, pmrArray[2]),
                    StswCalculator.ApplyOperator(operation, val2, pmrArray[3])
                ),
                2 => new CornerRadius(
                    StswCalculator.ApplyOperator(operation, val2, pmrArray[0]),
                    StswCalculator.ApplyOperator(operation, val2, pmrArray[1]),
                    StswCalculator.ApplyOperator(operation, val2, pmrArray[0]),
                    StswCalculator.ApplyOperator(operation, val2, pmrArray[1])
                ),
                1 => new CornerRadius(StswCalculator.ApplyOperator(operation, val2, pmrArray[0])),
                _ => new CornerRadius(val2)
            };
        }
    }

    /// <summary>
    /// Handles mathematical operations for Thickness type.
    /// </summary>
    /// <param name="value">The value to be converted.</param>
    /// <param name="operation">The mathematical operation to perform.</param>
    /// <param name="parameters">The parameters for the operation.</param>
    /// <param name="culture">The culture to use for parsing.</param>
    /// <returns>A new Thickness with the applied operation.</returns>
    private static object HandleThickness(object? value, string operation, string parameters, CultureInfo culture)
    {
        var pmrArray = Array.ConvertAll(parameters.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries), i => System.Convert.ToDouble(i, culture));
        if (value is Thickness val1)
        {
            return pmrArray.Length switch
            {
                4 => new Thickness(
                    StswCalculator.ApplyOperator(operation, val1.Left, pmrArray[0]),
                    StswCalculator.ApplyOperator(operation, val1.Top, pmrArray[1]),
                    StswCalculator.ApplyOperator(operation, val1.Right, pmrArray[2]),
                    StswCalculator.ApplyOperator(operation, val1.Bottom, pmrArray[3])
                ),
                2 => new Thickness(
                    StswCalculator.ApplyOperator(operation, val1.Left, pmrArray[0]),
                    StswCalculator.ApplyOperator(operation, val1.Top, pmrArray[1]),
                    StswCalculator.ApplyOperator(operation, val1.Right, pmrArray[0]),
                    StswCalculator.ApplyOperator(operation, val1.Bottom, pmrArray[1])
                ),
                1 => new Thickness(StswCalculator.ApplyOperator(operation, val1.Left, pmrArray[0])),
                _ => new Thickness(val1.Left)
            };
        }
        else
        {
            var val2 = System.Convert.ToDouble(value, culture);
            return pmrArray.Length switch
            {
                4 => new Thickness(
                    StswCalculator.ApplyOperator(operation, val2, pmrArray[0]),
                    StswCalculator.ApplyOperator(operation, val2, pmrArray[1]),
                    StswCalculator.ApplyOperator(operation, val2, pmrArray[2]),
                    StswCalculator.ApplyOperator(operation, val2, pmrArray[3])
                ),
                2 => new Thickness(
                    StswCalculator.ApplyOperator(operation, val2, pmrArray[0]),
                    StswCalculator.ApplyOperator(operation, val2, pmrArray[1]),
                    StswCalculator.ApplyOperator(operation, val2, pmrArray[0]),
                    StswCalculator.ApplyOperator(operation, val2, pmrArray[1])
                ),
                1 => new Thickness(StswCalculator.ApplyOperator(operation, val2, pmrArray[0])),
                _ => new Thickness(val2)
            };
        }
    }

    /// <summary>
    /// Handles mathematical operations for GridLength type.
    /// </summary>
    /// <param name="value">The value to be converted.</param>
    /// <param name="operation">The mathematical operation to perform.</param>
    /// <param name="parameters">The parameters for the operation.</param>
    /// <param name="culture">The culture to use for parsing.</param>
    /// <returns>A new GridLength with the applied operation.</returns>
    private static object HandleGridLength(object? value, string operation, string parameters, CultureInfo culture)
    {
        double.TryParse(parameters, NumberStyles.Number, culture, out var pmr);
        if (value is GridLength val1)
        {
            return new GridLength(StswCalculator.ApplyOperator(operation, val1.Value, pmr), val1.GridUnitType);
        }
        else
        {
            var val2 = System.Convert.ToDouble(value, culture);
            return new GridLength(StswCalculator.ApplyOperator(operation, val2, pmr));
        }
    }
}
