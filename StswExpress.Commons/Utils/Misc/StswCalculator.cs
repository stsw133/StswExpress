﻿namespace StswExpress.Commons;
/// <summary>
/// Provides utility methods for mathematical calculations including infix to postfix conversion and postfix evaluation.
/// </summary>
[StswInfo("0.9.0")]
public static class StswCalculator
{
    /// <summary>
    /// Evaluates a mathematical expression represented as a string.
    /// Supports basic arithmetic operations such as +, -, *, /, and parentheses.
    /// </summary>
    /// <param name="expression">The mathematical expression to evaluate.</param>
    /// <returns>The result of the evaluated expression as a <see cref="double"/>.</returns>
    /// <exception cref="FormatException">Thrown if the expression contains invalid syntax.</exception>
    public static double Compute(string expression)
    {
        try
        {
            return EvaluatePostfix(ConvertToPostfix(expression));
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Invalid mathematical expression: {expression}", ex);
        }
    }

    /// <summary>
    /// Attempts to evaluate a mathematical expression written in infix notation.
    /// Supports basic arithmetic operations such as +, -, *, /, and parentheses.
    /// </summary>
    /// <param name="expression">The mathematical expression to evaluate.</param>
    /// <param name="result">The result of the evaluated expression.</param>
    /// <returns><see langword="true"/> if the expression was successfully evaluated; otherwise, <see langword="false"/>.</returns>
    public static bool TryCompute(string expression, out double result)
    {
        try
        {
            result = EvaluatePostfix(ConvertToPostfix(expression));
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Converts an infix expression to postfix notation (Reverse Polish Notation).
    /// </summary>
    /// <param name="expression">The infix expression to convert.</param>
    /// <returns>A queue of tokens in postfix notation.</returns>
    private static Queue<string> ConvertToPostfix(string expression)
    {
        var output = new Queue<string>();
        var operators = new Stack<string>();
        var tokens = Tokenize(expression);

        foreach (var token in tokens)
        {
            if (double.TryParse(token, out _))
            {
                output.Enqueue(token);
            }
            else if (token == "(")
            {
                operators.Push(token);
            }
            else if (token == ")")
            {
                while (operators.Peek() != "(")
                    output.Enqueue(operators.Pop());
                operators.Pop();
            }
            else if (IsOperator(token))
            {
                while (operators.Count > 0 && Precedence(operators.Peek()) >= Precedence(token))
                    output.Enqueue(operators.Pop());
                operators.Push(token);
            }
        }

        while (operators.Count > 0)
            output.Enqueue(operators.Pop());

        return output;
    }

    /// <summary>
    /// Evaluates a postfix expression.
    /// </summary>
    /// <param name="postfix">A queue of tokens in postfix notation.</param>
    /// <returns>The result of the evaluated postfix expression.</returns>
    private static double EvaluatePostfix(Queue<string> postfix)
    {
        var stack = new Stack<double>();

        while (postfix.Count > 0)
        {
            var token = postfix.Dequeue();

            if (double.TryParse(token, out double number))
            {
                stack.Push(number);
            }
            else if (IsOperator(token))
            {
                double b = stack.Pop();
                double a = stack.Pop();
                stack.Push(ApplyOperator(token, a, b));
            }
        }

        return stack.Pop();
    }

    /// <summary>
    /// Tokenizes a mathematical expression into a list of string tokens.
    /// </summary>
    /// <param name="expression">The mathematical expression to tokenize.</param>
    /// <returns>A list of string tokens.</returns>
    private static List<string> Tokenize(string expression)
    {
        var tokens = new List<string>();
        var number = "";

        foreach (var ch in expression)
        {
            if (char.IsDigit(ch) || ch == '.')
            {
                number += ch;
            }
            else
            {
                if (!string.IsNullOrEmpty(number))
                {
                    tokens.Add(number);
                    number = "";
                }

                if (ch == ' ') continue;

                tokens.Add(ch.ToString());
            }
        }

        if (!string.IsNullOrEmpty(number))
            tokens.Add(number);

        return tokens;
    }

    /// <summary>
    /// Checks if a given token is an operator.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns>True if the token is an operator; otherwise, false.</returns>
    public static bool IsOperator(string token) => token == "+" || token == "-" || token == "*" || token == "/" || token == "^" || token == "%";

    /// <summary>
    /// Returns the precedence of a given operator.
    /// </summary>
    /// <param name="op">The operator whose precedence is to be determined.</param>
    /// <returns>The precedence of the operator.</returns>
    private static int Precedence(string op) => op switch
    {
        "+" or "-" => 1,
        "*" or "/" or "%" => 2,
        "^" => 3,
        _ => 0,
    };

    /// <summary>
    /// Applies a given operator to two operands and returns the result.
    /// </summary>
    /// <param name="op">The operator to apply.</param>
    /// <param name="a">The first operand.</param>
    /// <param name="b">The second operand.</param>
    /// <returns>The result of the operation.</returns>
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
    /// Applies a given operator to two operands and returns the result.
    /// </summary>
    /// <param name="op">The operator to apply.</param>
    /// <param name="a">The first operand.</param>
    /// <param name="b">The second operand.</param>
    /// <returns>The result of the operation.</returns>
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
