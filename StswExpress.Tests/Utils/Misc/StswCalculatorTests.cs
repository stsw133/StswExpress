namespace StswExpress.Commons.Tests;
public class StswCalculatorTests
{
    [Theory]
    [InlineData("1+2", 3)]
    [InlineData("2*3+4", 10)]
    [InlineData("2*(3+4)", 14)]
    [InlineData("10/2", 5)]
    [InlineData("2^3", 8)]
    [InlineData("7%4", 3)]
    [InlineData("3+4*2/(1-5)^2", 3.5)]
    public void Compute_ValidExpressions_ReturnsExpectedResult(string expression, double expected)
    {
        var result = StswCalculator.Compute(expression);
        Assert.Equal(expected, result, 5);
    }

    [Theory]
    [InlineData("1/0")]
    [InlineData("2++2")]
    [InlineData("abc")]
    [InlineData("((2+3)")]
    public void Compute_InvalidExpressions_ThrowsArgumentException(string expression)
    {
        Assert.Throws<ArgumentException>(() => StswCalculator.Compute(expression));
    }

    [Theory]
    [InlineData("1+2", 3, true)]
    [InlineData("2*3+4", 10, true)]
    [InlineData("2*(3+4)", 14, true)]
    [InlineData("1/0", 0, false)]
    [InlineData("bad", 0, false)]
    public void TryCompute_Expressions_ReturnsExpectedResult(string expression, double expected, bool shouldSucceed)
    {
        var success = StswCalculator.TryCompute(expression, out var result);
        Assert.Equal(shouldSucceed, success);
        if (shouldSucceed)
            Assert.Equal(expected, result, 5);
    }

    [Theory]
    [InlineData("+", true)]
    [InlineData("-", true)]
    [InlineData("*", true)]
    [InlineData("/", true)]
    [InlineData("^", true)]
    [InlineData("%", true)]
    [InlineData("x", false)]
    [InlineData("(", false)]
    public void IsOperator_ReturnsExpected(string token, bool expected)
    {
        Assert.Equal(expected, StswCalculator.IsOperator(token));
    }

    [Theory]
    [InlineData("+", 1, 2, 3)]
    [InlineData("-", 5, 3, 2)]
    [InlineData("*", 2, 4, 8)]
    [InlineData("/", 8, 2, 4)]
    [InlineData("^", 2, 3, 8)]
    [InlineData("%", 7, 3, 1)]
    public void ApplyOperator_NumericOperators_ReturnsExpected(string op, double a, double b, double expected)
    {
        var result = StswCalculator.ApplyOperator(op, a, b);
        Assert.Equal(expected, result, 5);
    }

    [Fact]
    public void ApplyOperator_DateTimeOperators_ReturnsExpected()
    {
        var date = new DateTime(2020, 1, 1);
        Assert.Equal(new DateTime(2021, 1, 1), StswCalculator.ApplyOperator("y", date, 1));
        Assert.Equal(new DateTime(2020, 2, 1), StswCalculator.ApplyOperator("M", date, 1));
        Assert.Equal(new DateTime(2020, 1, 2), StswCalculator.ApplyOperator("d", date, 1));
        Assert.Equal(new DateTime(2020, 1, 1, 1, 0, 0), StswCalculator.ApplyOperator("H", date, 1));
        Assert.Equal(new DateTime(2020, 1, 1, 0, 1, 0), StswCalculator.ApplyOperator("m", date, 1));
        Assert.Equal(new DateTime(2020, 1, 1, 0, 0, 1), StswCalculator.ApplyOperator("s", date, 1));
        Assert.Equal(new DateTime(2020, 1, 2), StswCalculator.ApplyOperator("+", date, 1));
        Assert.Equal(new DateTime(2019, 12, 31), StswCalculator.ApplyOperator("-", date, 1));
    }

    [Fact]
    public void ApplyOperator_DateTime_InvalidOperator_Throws()
    {
        var date = DateTime.Now;
        Assert.Throws<InvalidOperationException>(() => StswCalculator.ApplyOperator("X", date, 1));
    }

    [Fact]
    public void ApplyOperator_Numeric_InvalidOperator_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => StswCalculator.ApplyOperator("X", 1, 2));
    }
}
