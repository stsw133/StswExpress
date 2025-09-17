using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StswExpress.Tests;
public class StswRadioConverterTests
{
    private readonly StswRadioConverter _converter = StswRadioConverter.Instance;

    [Theory]
    [InlineData("Tab1", typeof(bool), "Tab1", true)]
    [InlineData("Tab2", typeof(bool), "Tab1", false)]
    [InlineData("Tab1", typeof(bool), "!Tab1", false)]
    [InlineData("Tab2", typeof(bool), "!Tab1", true)]
    [InlineData(null, typeof(bool), "Tab1", false)]
    [InlineData("Tab1", typeof(bool), null, false)]
    [InlineData(null, typeof(bool), null, true)]
    public void Convert_Bool_ReturnsExpected(string input, Type targetType, object? parameter, bool expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("0", typeof(bool), "0", true)]
    [InlineData("1", typeof(bool), "0", false)]
    [InlineData("0", typeof(bool), "!0", false)]
    [InlineData("1", typeof(bool), "!0", true)]
    public void Convert_Bool_NumericString_ReturnsExpected(string input, Type targetType, object? parameter, bool expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Admin", typeof(Visibility), "Admin", Visibility.Visible)]
    [InlineData("User", typeof(Visibility), "Admin", Visibility.Collapsed)]
    [InlineData("Admin", typeof(Visibility), "!Admin", Visibility.Collapsed)]
    [InlineData("User", typeof(Visibility), "!Admin", Visibility.Visible)]
    [InlineData(null, typeof(Visibility), "Admin", Visibility.Collapsed)]
    [InlineData("Admin", typeof(Visibility), null, Visibility.Collapsed)]
    [InlineData(null, typeof(Visibility), null, Visibility.Visible)]
    public void Convert_Visibility_ReturnsExpected(string input, Type targetType, object? parameter, Visibility expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Tab1", typeof(string), "Tab1", "True")]
    [InlineData("Tab2", typeof(string), "Tab1", "False")]
    [InlineData("Tab1", typeof(string), "!Tab1", "False")]
    [InlineData("Tab2", typeof(string), "!Tab1", "True")]
    public void Convert_StringTargetType_ReturnsExpected(string input, Type targetType, object? parameter, string expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result?.ToString());
    }

    [Fact]
    public void ConvertBack_ReturnsParameter()
    {
        var parameter = "Tab1";
        var result = _converter.ConvertBack("Tab1", typeof(bool), parameter, CultureInfo.InvariantCulture);
        Assert.Equal(parameter, result);
    }

    [Fact]
    public void ProvideValue_ReturnsSingletonInstance()
    {
        var instance = _converter.ProvideValue(null);
        Assert.Same(StswRadioConverter.Instance, instance);
    }
}
