using System.Globalization;
using System.Numerics;

namespace StswExpress.Commons;

/// <summary>
/// Provides mathematical utility methods, including safe division and expression evaluation.
/// </summary>
[StswInfo("0.21.0")]
public static class StswMath
{
    /// <summary>
    /// Shifts the index by the specified step within a defined count, with optional looping.
    /// </summary>
    /// <param name="index">The current index.</param>
    /// <param name="step">The step value for shifting.</param>
    /// <param name="count">The total count of items (must be greater than 0).</param>
    /// <param name="loop">Specifies whether looping is allowed when shifting past boundaries.</param>
    /// <returns>The new shifted index, respecting looping and boundary conditions.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when count is less than or equal to 0.</exception>
    [StswInfo("0.6.0", "0.21.0")]
    public static int AdvanceIndex(int index, int step, int count, bool loop = true)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must be > 0.");

        if (loop)
        {
            var sum = (long)index + step;
            return (int)EuclidMod(sum, count);
        }
        else
        {
            var sum = (long)index + step;
            return (int)Math.Clamp(sum, 0, count - 1);
        }
    }

    /// <summary>
    /// Shifts the value by the specified step within a defined range, with optional looping.
    /// </summary>
    /// <param name="value">The current value.</param>
    /// <param name="step">The step value for shifting.</param>
    /// <param name="min">The minimum possible value (inclusive lower bound).</param>
    /// <param name="maxExclusive">The maximum possible value (exclusive upper bound).</param>
    /// <param name="loop">Specifies whether looping is allowed when shifting past boundaries.</param>
    /// <returns>The new shifted value, respecting looping and boundary conditions.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxExclusive is less than or equal to min.</exception>
    [StswInfo("0.21.0")]
    public static int AdvanceInRange(int value, int step, int min, int maxExclusive, bool loop = true)
    {
        if (maxExclusive <= min) throw new ArgumentOutOfRangeException(nameof(maxExclusive), "maxExclusive must be > min.");
        var span = maxExclusive - min;

        if (loop)
        {
            var sum = (long)value + step - min;
            return min + (int)EuclidMod(sum, span);
        }
        else
        {
            var sum = (long)value + step;
            return (int)Math.Clamp(sum, min, maxExclusive - 1);
        }
    }

    /// <summary>
    /// Computes the Euclidean modulo of a by n, ensuring a non-negative result.
    /// </summary>
    /// <param name="a">The dividend.</param>
    /// <param name="n">The modulus.</param>
    /// <returns>The Euclidean modulo result.</returns>
    [StswInfo("0.21.0")]
    private static long EuclidMod(long a, long n)
    {
        var r = a % n;
        return r < 0 ? r + n : r;
    }

    /// <summary>
    /// Performs a division and returns zero if the denominator is zero.
    /// </summary>
    /// <typeparam name="T">A numeric type that implements <see cref="INumber{T}"/>.</typeparam>
    /// <param name="num">The numerator.</param>
    /// <param name="den">The denominator.</param>
    /// <returns>The result of the division, or zero if the denominator is zero.</returns>
    [StswInfo("0.21.0")]
    public static T Div0<T>(T num, T den) where T : INumber<T> => den == T.Zero ? T.Zero : num / den;

    /// <summary>
    /// Performs a division and returns a default value if the denominator is zero.
    /// </summary>
    /// <typeparam name="T">A numeric type that implements <see cref="INumber{T}"/>.</typeparam>
    /// <param name="num">The numerator.</param>
    /// <param name="den">The denominator.</param>
    /// <returns>The result of the division, or the specified default value if the denominator is zero.</returns>
    [StswInfo("0.21.0")]
    public static T Div0<T>(T num, T den, T defaultValue) where T : INumber<T> => den == T.Zero ? defaultValue : num / den;

    /// <summary>
    /// Evaluates a mathematical expression provided as a string and returns the result as a <see cref="double"/>.
    /// </summary>
    /// <param name="expression">The mathematical expression to evaluate.</param>
    /// <returns>The result of the evaluated expression as a <see cref="double"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the expression is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the expression is invalid.</exception>
    [StswInfo("0.9.0")]
    public static double Compute(string expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        try
        {
            return Calculator.EvaluateInfix(expression.AsSpan());
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            throw new ArgumentException($"Invalid mathematical expression: {expression}", ex);
        }
    }

    /// <summary>
    /// Attempts to evaluate a mathematical expression provided as a string and returns the result as a <see cref="double"/>.
    /// </summary>
    /// <param name="expression">The mathematical expression to evaluate.</param>
    /// <param name="result">The result of the evaluated expression.</param>
    /// <returns><see langword="true"/> if the expression was successfully evaluated; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.9.0")]
    public static bool TryCompute(string expression, out double result)
    {
        result = default;
        if (expression is null)
            return false;

        try
        {
            result = Calculator.EvaluateInfix(expression.AsSpan());
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// A simple calculator for evaluating mathematical expressions in infix notation.
    /// </summary>
    [StswInfo("0.21.0")]
    private static class Calculator
    {
        /// <summary>
        /// Defines supported operators.
        /// </summary>
        [StswInfo("0.21.0")]
        private enum Op : byte
        {
            Add, Sub, Mul, Div, Mod, Pow, // binary
            UnaryPlus, UnaryMinus,        // unary
            LParen                        // marker
        }

        /// <summary>
        /// Gets the precedence of the operator.
        /// </summary>
        /// <param name="op">The operator.</param>
        /// <returns>The precedence level as an integer.</returns>
        [StswInfo("0.21.0")]
        private static int Precedence(Op op) => op switch
        {
            Op.UnaryPlus or Op.UnaryMinus => 4,
            Op.Pow => 3,
            Op.Mul or Op.Div or Op.Mod => 2,
            Op.Add or Op.Sub => 1,
            _ => 0
        };

        /// <summary>
        /// Determines if the operator is right associative.
        /// </summary>
        /// <param name="op">The operator.</param>
        /// <returns><see langword="true"/> if the operator is right associative; otherwise, <see langword="false"/>.</returns>
        [StswInfo("0.21.0")]
        private static bool IsRightAssociative(Op op) => op is Op.Pow || op is Op.UnaryPlus || op is Op.UnaryMinus;

        /// <summary>
        /// Applies the operator to the given operands.
        /// </summary>
        /// <param name="op">The operator.</param>
        /// <param name="b">The second operand (for binary operators) or ignored (for unary operators).</param>
        /// <param name="a">The first operand.</param>
        /// <returns>The result of applying the operator.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the operator is unsupported.</exception>
        [StswInfo("0.21.0")]
        private static double Apply(Op op, double b, double a) => op switch
        {
            Op.Add => a + b,
            Op.Sub => a - b,
            Op.Mul => a * b,
            Op.Div => a / b,
            Op.Mod => a % b,
            Op.Pow => Math.Pow(a, b),
            Op.UnaryPlus => +a,
            Op.UnaryMinus => -a,
            _ => throw new InvalidOperationException($"Unsupported operator: {op}")
        };

        /// <summary>
        /// Evaluates a mathematical expression in infix notation.
        /// </summary>
        /// <param name="s">The expression to evaluate.</param>
        /// <returns>The result of the evaluated expression as a <see cref="double"/>.</returns>
        /// <exception cref="FormatException">Thrown if the expression is invalid.</exception>
        [StswInfo("0.21.0")]
        internal static double EvaluateInfix(ReadOnlySpan<char> s)
        {
            var values = new Stack<double>(16);
            var ops = new Stack<Op>(16);

            var i = 0;
            var expectUnary = true;

            while (i < s.Length)
            {
                var c = s[i];
                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                if (IsNumberStart(c))
                {
                    if (TryReadNumber(s, ref i, out double number))
                    {
                        values.Push(number);
                        expectUnary = false;
                        continue;
                    }
                    throw new FormatException($"Invalid number near index {i}.");
                }

                if (c == '(')
                {
                    ops.Push(Op.LParen);
                    i++;
                    expectUnary = true;
                    continue;
                }
                if (c == ')')
                {
                    i++;
                    while (ops.Count > 0 && ops.Peek() != Op.LParen)
                        PopAndApply(values, ops.Pop());
                    if (ops.Count == 0 || ops.Pop() != Op.LParen)
                        throw new FormatException("Mismatched parentheses.");
                    expectUnary = false;
                    continue;
                }

                if (TryMapOperator(c, expectUnary, out var op))
                {
                    i++;
                    while (ops.Count > 0 && ops.Peek() != Op.LParen)
                    {
                        var top = ops.Peek();
                        var pTop = Precedence(top);
                        var pCur = Precedence(op);
                        if (pTop > pCur || (pTop == pCur && !IsRightAssociative(op)))
                            PopAndApply(values, ops.Pop());
                        else break;
                    }

                    ops.Push(op);
                    expectUnary = true;
                    continue;
                }

                throw new FormatException($"Unexpected character '{c}' at index {i}.");
            }

            while (ops.Count > 0)
            {
                var op = ops.Pop();
                if (op == Op.LParen) throw new FormatException("Mismatched parentheses.");
                PopAndApply(values, op);
            }

            if (values.Count != 1) throw new FormatException("Invalid expression (stack not reduced to single value).");
            return values.Pop();
        }

        /// <summary>
        /// Pops operands from the value stack and applies the operator, then pushes the result back onto the stack.
        /// </summary>
        /// <param name="values">The stack of values.</param>
        /// <param name="op">The operator to apply.</param>
        /// <exception cref="FormatException">Thrown if there are insufficient operands for the operator.</exception>
        [StswInfo("0.21.0")]
        private static void PopAndApply(Stack<double> values, Op op)
        {
            if (op is Op.UnaryPlus or Op.UnaryMinus)
            {
                if (values.Count < 1)
                    throw new FormatException("Missing operand for unary operator.");

                var a = values.Pop();
                values.Push(Apply(op, 0d, a));
                return;
            }

            if (values.Count < 2)
                throw new FormatException("Missing operand for binary operator.");

            var b = values.Pop();
            var a2 = values.Pop();
            values.Push(Apply(op, b, a2));
        }

        /// <summary>
        /// Maps a character to an operator, considering whether a unary operator is expected.
        /// </summary>
        /// <param name="c">The character to map.</param>
        /// <param name="expectUnary">Indicates if a unary operator is expected.</param>
        /// <param name="op">The mapped operator if successful; otherwise, undefined.</param>
        /// <returns><see langword="true"/> if the character was successfully mapped to an operator; otherwise, <see langword="false"/>.</returns>
        [StswInfo("0.21.0")]
        private static bool TryMapOperator(char c, bool expectUnary, out Op op)
        {
            op = c switch
            {
                '+' => expectUnary ? Op.UnaryPlus : Op.Add,
                '-' => expectUnary ? Op.UnaryMinus : Op.Sub,
                '*' => Op.Mul,
                '/' => Op.Div,
                '%' => Op.Mod,
                '^' => Op.Pow,
                _ => (Op)255
            };
            return op != (Op)255;
        }

        /// <summary>
        /// Determines if the character can start a number, considering if a unary operator is expected.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns><see langword="true"/> if the character can start a number; otherwise, <see langword="false"/>.</returns>
        [StswInfo("0.21.0")]
        private static bool IsNumberStart(char c)
        {
            if (char.IsDigit(c) || c == '.')
                return true;

            return false;
        }

        /// <summary>
        /// Attempts to read a number from the span starting at the given index.
        /// </summary>
        /// <param name="s">The span to read from.</param>
        /// <param name="i">The current index, which will be updated to the position after the number if successful.</param>
        /// <param name="value">The parsed number if successful; otherwise, undefined.</param>
        /// <returns><see langword="true"/> if a number was successfully read; otherwise, <see langword="false"/>.</returns>
        [StswInfo("0.21.0")]
        private static bool TryReadNumber(ReadOnlySpan<char> s, ref int i, out double value)
        {
            var start = i;
            var hasDot = false;
            var hasExp = false;

            while (i < s.Length)
            {
                var ch = s[i];
                if (char.IsDigit(ch))
                {
                    i++;
                    continue;
                }

                if (ch == '.' && !hasDot && !hasExp)
                {
                    hasDot = true; i++; continue;
                }

                if ((ch == 'e' || ch == 'E') && !hasExp)
                {
                    hasExp = true;
                    i++;

                    if (i < s.Length && (s[i] == '+' || s[i] == '-'))
                        i++;

                    if (i >= s.Length || !char.IsDigit(s[i]))
                    {
                        value = default;
                        return false;
                    }

                    continue;
                }

                break;
            }

            var span = s[start..i];
            if (span.Length == 1 && span[0] == '.')
            {
                value = default;
                return false;
            }

            return double.TryParse(span, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }
    }
}
