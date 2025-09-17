using System;
using System.Globalization;
using System.Windows;

namespace StswExpress.Tests;
public class StswCompareConverterTests
{
    private readonly StswCompareConverter _converter = StswCompareConverter.Instance;
    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    public enum TestEnum
    {
        None = 0,
        First = 1,
        Second = 2,
        Third = 4
    }

    [Theory]
    [InlineData(5, typeof(bool), "<=10", true)]
    [InlineData(15, typeof(bool), "<=10", false)]
    [InlineData(10, typeof(bool), ">=10", true)]
    [InlineData(9, typeof(bool), ">=10", false)]
    [InlineData(5, typeof(bool), ">3", true)]
    [InlineData(2, typeof(bool), ">3", false)]
    [InlineData(5, typeof(bool), "=5", true)]
    [InlineData(5, typeof(bool), "!5", false)]
    [InlineData(6, typeof(bool), "!5", true)]
    [InlineData(2, typeof(bool), "&2", true)]
    [InlineData(4, typeof(bool), "&2", false)]
    public void NumberComparisons(double value, Type targetType, string parameter, bool expected)
    {
        var result = _converter.Convert(value, targetType, parameter, _culture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Admin", typeof(bool), "=Admin", true)]
    [InlineData("User", typeof(bool), "=Admin", false)]
    [InlineData("Admin", typeof(bool), "!Admin", false)]
    [InlineData("User", typeof(bool), "!Admin", true)]
    [InlineData("ADMIN", typeof(bool), "@Admin", true)]
    [InlineData("admin", typeof(bool), "@Admin", true)]
    [InlineData("user", typeof(bool), "@Admin", false)]
    [InlineData("==test", typeof(bool), "==test", false)]
    [InlineData("=test", typeof(bool), "==test", true)]
    public void StringComparisons(string value, Type targetType, string parameter, bool expected)
    {
        var result = _converter.Convert(value, targetType, parameter, _culture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(TestEnum.First, typeof(bool), "=First", true)]
    [InlineData(TestEnum.Second, typeof(bool), "=First", false)]
    [InlineData(TestEnum.First, typeof(bool), "!First", false)]
    [InlineData(TestEnum.Second, typeof(bool), "!First", true)]
    [InlineData(TestEnum.Third, typeof(bool), "&4", true)]
    [InlineData(TestEnum.Second, typeof(bool), "&4", false)]
    [InlineData(TestEnum.Second, typeof(bool), "&2", true)]
    [InlineData(TestEnum.First, typeof(bool), "&2", false)]
    public void EnumComparisons(TestEnum value, Type targetType, string parameter, bool expected)
    {
        var result = _converter.Convert(value, targetType, parameter, _culture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(true, typeof(Visibility), "=True", Visibility.Visible)]
    [InlineData(false, typeof(Visibility), "=True", Visibility.Collapsed)]
    [InlineData("Admin", typeof(Visibility), "=Admin", Visibility.Visible)]
    [InlineData("User", typeof(Visibility), "=Admin", Visibility.Collapsed)]
    [InlineData(5, typeof(Visibility), "<=10", Visibility.Visible)]
    [InlineData(15, typeof(Visibility), "<=10", Visibility.Collapsed)]
    public void VisibilityTargetType(object value, Type targetType, string parameter, Visibility expected)
    {
        var result = _converter.Convert(value, targetType, parameter, _culture);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EnumParameterViaStatic()
    {
        var result = _converter.Convert(TestEnum.First, typeof(bool), TestEnum.First, _culture);
        Assert.True((bool)result);

        result = _converter.Convert(TestEnum.Second, typeof(bool), TestEnum.First, _culture);
        Assert.False((bool)result);
    }

    [Fact]
    public void ConvertBackAlwaysReturnsBindingDoNothing()
    {
        var result = _converter.ConvertBack("any", typeof(string), null, _culture);
        Assert.Equal(System.Windows.Data.Binding.DoNothing, result);
    }

    [Fact]
    public void InvalidParameterReturnsBindingDoNothing()
    {
        var result = _converter.Convert("value", typeof(bool), null, _culture);
        Assert.Equal(System.Windows.Data.Binding.DoNothing, result);

        result = _converter.Convert("value", typeof(bool), "", _culture);
        Assert.Equal(System.Windows.Data.Binding.DoNothing, result);
    }
}
