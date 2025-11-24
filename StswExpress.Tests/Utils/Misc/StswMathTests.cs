using System;
using System.Globalization;
using Xunit;

namespace StswExpress.Commons.Tests.Utils.Misc;
public class StswMathTests
{
    [Fact]
    public void AdvanceIndex_LoopTrue_WrapsAround()
    {
        var result = StswMath.AdvanceIndex(0, -1, 5, loop: true);
        Assert.Equal(4, result);
    }

    [Fact]
    public void AdvanceIndex_LoopFalse_ClampsToBounds()
    {
        Assert.Equal(0, StswMath.AdvanceIndex(0, -1, 5, loop: false));
        Assert.Equal(4, StswMath.AdvanceIndex(4, 1, 5, loop: false));
    }

    [Fact]
    public void AdvanceIndex_WrapsForwardCorrectly()
    {
        Assert.Equal(1, StswMath.AdvanceIndex(4, 2, 5, loop: true));
    }

    [Fact]
    public void AdvanceInRange_LoopTrue_WrapsWithinRange()
    {
        var result = StswMath.AdvanceInRange(10, -1, 10, 15, loop: true);
        Assert.Equal(14, result);
    }

    [Fact]
    public void AdvanceInRange_LoopFalse_ClampsWithinRange()
    {
        Assert.Equal(10, StswMath.AdvanceInRange(10, -100, 10, 15, loop: false));
        Assert.Equal(14, StswMath.AdvanceInRange(14, 100, 10, 15, loop: false));
    }

    [Fact]
    public void EuclidMod_PositiveAndNegative()
    {
        Assert.Equal(2, StswMath.EuclidMod(7, 5));
        Assert.Equal(4, StswMath.EuclidMod(-1, 5));
    }

    [Fact]
    public void EuclidMod_ThrowsOnNonPositiveModulus()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => StswMath.EuclidMod(1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => StswMath.EuclidMod(1L, 0L));
    }

    [Fact]
    public void Div0_IntAndDouble_Behavior()
    {
        Assert.Equal(5, StswMath.Div0<int>(10, 2));
        Assert.Equal(0, StswMath.Div0<int>(10, 0));
        Assert.Equal(99, StswMath.Div0<int>(10, 0, 99));

        Assert.Equal(2.5, StswMath.Div0<double>(5.0, 2.0));
        Assert.Equal(0.0, StswMath.Div0<double>(5.0, 0.0));
    }

    [Fact]
    public void Lerp_And_InverseLerp_Work()
    {
        Assert.Equal(5.0, StswMath.Lerp(0.0, 10.0, 0.5));
        Assert.Equal(0.5, StswMath.InverseLerp(0.0, 10.0, 5.0));
    }

    [Fact]
    public void InverseLerp_Throws_WhenStartEqualsEnd()
    {
        Assert.Throws<ArgumentException>(() => StswMath.InverseLerp(1.0, 1.0, 1.0));
    }

    [Fact]
    public void Compute_EvaluatesBasicExpressions()
    {
        Assert.Equal(7.0, StswMath.Compute("1+2*3"));
        Assert.Equal(-5.0, StswMath.Compute("-(2+3)"));
        Assert.Equal(512.0, StswMath.Compute("2^3^2")); // right associative
        Assert.Equal(1.0, StswMath.Compute("10%3"));
    }

    [Fact]
    public void Compute_WithFormatProvider_ParsesCultureSpecificNumbers()
    {
        var french = new CultureInfo("fr-FR");
        // In fr-FR decimal separator is ','
        Assert.Equal(3.0, StswMath.Compute("1,5+1,5", french));
    }

    [Fact]
    public void Compute_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => StswMath.Compute(null!));
    }

    [Fact]
    public void TryCompute_ReturnsFalseOnInvalidOrNull()
    {
        Assert.False(StswMath.TryCompute(null!, out _));
        Assert.False(StswMath.TryCompute("1+;+2", out _));
    }

    [Fact]
    public void TryCompute_ReturnsTrueAndOutputsValueOnValidInput()
    {
        Assert.True(StswMath.TryCompute("2*(3+4)", out var result));
        Assert.Equal(14.0, result);
    }
}