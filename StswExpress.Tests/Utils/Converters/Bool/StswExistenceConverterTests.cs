using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StswExpress.Tests.Utils.Converters;
public class StswExistenceConverterTests
{
    private readonly StswExistenceConverter _converter = StswExistenceConverter.Instance;

    [Theory]
    [InlineData(null, typeof(bool), null, false)]
    [InlineData("value", typeof(bool), null, true)]
    [InlineData(null, typeof(Visibility), null, Visibility.Collapsed)]
    [InlineData("value", typeof(Visibility), null, Visibility.Visible)]
    public void Convert_DefaultParameter_ReturnsExpected(object? input, Type targetType, object? parameter, object expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, typeof(bool), "null", true)]
    [InlineData("value", typeof(bool), "null", false)]
    [InlineData(null, typeof(bool), "!null", false)]
    [InlineData("value", typeof(bool), "!null", true)]
    public void Convert_NullCondition_ReturnsExpected(object? input, Type targetType, object? parameter, bool expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("", typeof(bool), "empty", true)]
    [InlineData("notempty", typeof(bool), "empty", false)]
    [InlineData("", typeof(bool), "!empty", false)]
    [InlineData("notempty", typeof(bool), "!empty", true)]
    public void Convert_EmptyStringCondition_ReturnsExpected(string input, Type targetType, object? parameter, bool expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, typeof(bool), "default", true)]
    [InlineData(0, typeof(bool), "default", true)]
    [InlineData(1, typeof(bool), "default", false)]
    [InlineData(0, typeof(bool), "!default", false)]
    [InlineData(1, typeof(bool), "!default", true)]
    public void Convert_DefaultCondition_ReturnsExpected(object? input, Type targetType, object? parameter, bool expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(new int[0], typeof(bool), "empty", true)]
    [InlineData(new int[] { 1 }, typeof(bool), "empty", false)]
    [InlineData(new int[0], typeof(bool), "!empty", false)]
    [InlineData(new int[] { 1 }, typeof(bool), "!empty", true)]
    public void Convert_EmptyArrayCondition_ReturnsExpected(int[] input, Type targetType, object? parameter, bool expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_EmptyIEnumerableCondition_ReturnsExpected()
    {
        IEnumerable<object> emptyEnumerable = new List<object>();
        IEnumerable<object> nonEmptyEnumerable = new List<object> { 1 };
        Assert.True((bool)_converter.Convert(emptyEnumerable, typeof(bool), "empty", CultureInfo.InvariantCulture));
        Assert.False((bool)_converter.Convert(nonEmptyEnumerable, typeof(bool), "empty", CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData(null, typeof(bool), "default empty", true)]
    [InlineData(new int[0], typeof(bool), "default empty", true)]
    [InlineData(new int[] { 1 }, typeof(bool), "default empty", false)]
    [InlineData(0, typeof(bool), "default !empty", true)]
    [InlineData(new int[] { 1 }, typeof(bool), "default !empty", true)]
    public void Convert_MultipleConditions_ReturnsExpected(object? input, Type targetType, object? parameter, bool expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, typeof(Visibility), "null", Visibility.Visible)]
    [InlineData("value", typeof(Visibility), "null", Visibility.Collapsed)]
    [InlineData(null, typeof(Visibility), "!null", Visibility.Collapsed)]
    [InlineData("value", typeof(Visibility), "!null", Visibility.Visible)]
    public void Convert_Visibility_ReturnsExpected(object? input, Type targetType, object? parameter, Visibility expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ConvertBack_Always_ReturnsBindingDoNothing()
    {
        var result = _converter.ConvertBack(true, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void ProvideValue_ReturnsSingletonInstance()
    {
        var instance = _converter.ProvideValue(null);
        Assert.Same(StswExistenceConverter.Instance, instance);
    }
}
