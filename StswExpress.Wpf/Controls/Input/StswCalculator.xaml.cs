using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress.Wpf;
/// <summary>
/// Represents a calculator control with a built-in numeric keypad, allowing users to input simple arithmetic expressions
/// and view the current operation and result.
/// </summary>
public class StswCalculator : Control, IStswCornerControl
{
    static StswCalculator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswCalculator), new FrameworkPropertyMetadata(typeof(StswCalculator)));
    }
    public StswCalculator()
    {
        DigitCommand = new StswCommand<string?>(AppendDigit);
        DecimalSeparatorCommand = new StswCommand(AppendDecimalSeparator);
        OperatorCommand = new StswCommand<string?>(ApplyOperator);
        EqualsCommand = new StswCommand(CalculateResult);
        ClearCommand = new StswCommand(ClearAll);
        BackspaceCommand = new StswCommand(RemoveLast);

        DisplayText = _currentInput;
        ExpressionText = string.Empty;
    }

    #region Dependency properties
    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswCalculator)
        );

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswCalculator)
        );

    /// <summary>
    /// Gets or sets the text displayed as the current input or result.
    /// </summary>
    public string DisplayText
    {
        get => (string)GetValue(DisplayTextProperty);
        set => SetValue(DisplayTextProperty, value);
    }
    public static readonly DependencyProperty DisplayTextProperty
        = DependencyProperty.Register(
            nameof(DisplayText),
            typeof(string),
            typeof(StswCalculator),
            new PropertyMetadata("0")
        );

    /// <summary>
    /// Gets or sets the text that displays the current operation.
    /// </summary>
    public string? ExpressionText
    {
        get => (string?)GetValue(ExpressionTextProperty);
        set => SetValue(ExpressionTextProperty, value);
    }
    public static readonly DependencyProperty ExpressionTextProperty
        = DependencyProperty.Register(
            nameof(ExpressionText),
            typeof(string),
            typeof(StswCalculator),
            new PropertyMetadata(string.Empty)
        );
    #endregion

    #region Logic
    private string _currentInput = "0";
    private double? _accumulator;
    private string? _pendingOperator;
    private bool _resetInput = true;

    /// <summary>
    /// Gets the command that appends digits to the current input.
    /// </summary>
    public ICommand DigitCommand { get; }

    /// <summary>
    /// Gets the command that appends the decimal separator to the current input.
    /// </summary>
    public ICommand DecimalSeparatorCommand { get; }

    /// <summary>
    /// Gets the command that applies an arithmetic operator.
    /// </summary>
    public ICommand OperatorCommand { get; }

    /// <summary>
    /// Gets the command that calculates the result of the current expression.
    /// </summary>
    public ICommand EqualsCommand { get; }

    /// <summary>
    /// Gets the command that clears all values and resets the calculator.
    /// </summary>
    public ICommand ClearCommand { get; }

    /// <summary>
    /// Gets the command that removes the last character from the current input.
    /// </summary>
    public ICommand BackspaceCommand { get; }

    /// <summary>
    /// Appends a digit to the current input.
    /// </summary>
    /// <param name="digit">The digit to append.</param>
    private void AppendDigit(string? digit)
    {
        if (string.IsNullOrWhiteSpace(digit))
            return;

        if (_resetInput || _currentInput == "0")
            _currentInput = digit;
        else
            _currentInput += digit;

        _resetInput = false;
        DisplayText = _currentInput;
    }

    /// <summary>
    /// Appends the decimal separator to the current input.
    /// </summary>
    private void AppendDecimalSeparator()
    {
        var separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        if (_resetInput)
        {
            _currentInput = $"0{separator}";
            _resetInput = false;
        }
        else if (!_currentInput.Contains(separator, StringComparison.Ordinal))
        {
            _currentInput += separator;
        }

        DisplayText = _currentInput;
    }

    /// <summary>
    /// Applies an arithmetic operator to the current input.
    /// </summary>
    /// <param name="operatorSymbol">The operator to apply.</param>
    private void ApplyOperator(string? operatorSymbol)
    {
        if (string.IsNullOrWhiteSpace(operatorSymbol))
            return;

        if (double.TryParse(_currentInput, NumberStyles.Any, CultureInfo.CurrentCulture, out var number))
        {
            if (_accumulator is null)
            {
                _accumulator = number;
            }
            else if (_pendingOperator is not null && !_resetInput)
            {
                _accumulator = ExecuteOperation(_accumulator.Value, number, _pendingOperator);
                DisplayText = FormatNumber(_accumulator);
            }
        }

        _pendingOperator = operatorSymbol;
        ExpressionText = _accumulator is not null ? $"{FormatNumber(_accumulator)} {_pendingOperator}" : operatorSymbol;
        _resetInput = true;
    }

    /// <summary>
    /// Calculates the result of the current expression.
    /// </summary>
    private void CalculateResult()
    {
        if (_pendingOperator is null || _accumulator is null)
        {
            ExpressionText = string.Empty;
            return;
        }

        if (!double.TryParse(_currentInput, NumberStyles.Any, CultureInfo.CurrentCulture, out var number))
            return;

        var result = ExecuteOperation(_accumulator.Value, number, _pendingOperator);

        ExpressionText = $"{FormatNumber(_accumulator)} {_pendingOperator} {FormatNumber(number)} =";

        if (double.IsFinite(result))
        {
            _currentInput = FormatNumber(result);
            _accumulator = result;
        }
        else
        {
            _currentInput = "Error";
            _accumulator = null;
        }

        _pendingOperator = null;
        _resetInput = true;
        DisplayText = _currentInput;
    }

    /// <summary>
    /// Clears all values and resets the calculator.
    /// </summary>
    private void ClearAll()
    {
        _accumulator = null;
        _pendingOperator = null;
        _currentInput = "0";
        _resetInput = true;
        ExpressionText = string.Empty;
        DisplayText = _currentInput;
    }

    /// <summary>
    /// Removes the last character from the current input.
    /// </summary>
    private void RemoveLast()
    {
        if (_resetInput)
        {
            _currentInput = DisplayText;
            _resetInput = false;
        }

        if (_currentInput.Length > 1)
            _currentInput = _currentInput[..^1];
        else
            _currentInput = "0";

        DisplayText = _currentInput;
    }

    /// <summary>
    /// Executes the specified arithmetic operation.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <param name="operatorSymbol">The operator to apply.</param>
    /// <returns>The result of the operation.</returns>
    private static double ExecuteOperation(double left, double right, string operatorSymbol) => operatorSymbol switch
    {
        "/" or "÷" => right == 0 ? double.NaN : left / right,
        "*" or "×" => left * right,
        "-" or "−" => left - right,
        "+" => left + right,
        _ => right
    };

    /// <summary>
    /// Formats a nullable double value according to the current culture.
    /// </summary>
    /// <param name="number">The number to format.</param>
    /// <returns>The formatted string representation of the number, or an empty string if <paramref name="number"/> is <see langword="null"/>.</returns>
    private static string FormatNumber(double? number) => number?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
    #endregion
}
