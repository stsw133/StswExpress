using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace StswExpress.Tests.Utils.Converters;
public class StswHasAttributeConverterTests
{
    private readonly StswHasAttributeConverter _converter = StswHasAttributeConverter.Instance;

    public enum TestEnum
    {
        [Obsolete]
        Deprecated,
        Normal
    }

    public class TestClass
    {
        [Description("Test property")]
        public string WithDescription { get; set; } = "desc";

        public string WithoutDescription { get; set; } = "no-desc";
    }

    [Theory]
    [InlineData(typeof(ObsoleteAttribute), TestEnum.Deprecated, typeof(Visibility), Visibility.Visible)]
    [InlineData(typeof(ObsoleteAttribute), TestEnum.Normal, typeof(Visibility), Visibility.Collapsed)]
    [InlineData(typeof(ObsoleteAttribute), TestEnum.Deprecated, typeof(bool), true)]
    [InlineData(typeof(ObsoleteAttribute), TestEnum.Normal, typeof(bool), false)]
    public void Convert_EnumValue_ReturnsExpected(Type attributeType, TestEnum value, Type targetType, object expected)
    {
        var result = _converter.Convert(value, targetType, attributeType.AssemblyQualifiedName, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(typeof(DescriptionAttribute), "WithDescription", typeof(Visibility), Visibility.Visible)]
    [InlineData(typeof(DescriptionAttribute), "WithoutDescription", typeof(Visibility), Visibility.Collapsed)]
    [InlineData(typeof(DescriptionAttribute), "WithDescription", typeof(bool), true)]
    [InlineData(typeof(DescriptionAttribute), "WithoutDescription", typeof(bool), false)]
    public void Convert_PropertyValue_ReturnsExpected(Type attributeType, string propertyName, Type targetType, object expected)
    {
        var obj = new TestClass();
        var property = obj.GetType().GetProperty(propertyName);
        var value = property?.GetValue(obj);

        var result = _converter.Convert(value, targetType, attributeType.AssemblyQualifiedName, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, typeof(Visibility), Visibility.Collapsed)]
    [InlineData(null, typeof(bool), false)]
    [InlineData(null, typeof(string), null)]
    public void Convert_NullValueOrParameter_ReturnsDefault(object? value, Type targetType, object? expected)
    {
        var result = _converter.Convert(value, targetType, null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_InvalidAttributeType_ReturnsDefault()
    {
        var result = _converter.Convert(TestEnum.Deprecated, typeof(bool), "NonExistentAttribute", CultureInfo.InvariantCulture);
        Assert.Equal(false, result);
    }

    [Fact]
    public void Convert_EnumFieldNotFound_ReturnsDefault()
    {
        var fakeEnum = (TestEnum)999;
        var result = _converter.Convert(fakeEnum, typeof(bool), typeof(ObsoleteAttribute).AssemblyQualifiedName, CultureInfo.InvariantCulture);
        Assert.Equal(false, result);
    }

    [Fact]
    public void Convert_PropertyNotFound_ReturnsDefault()
    {
        var obj = new TestClass();
        var result = _converter.Convert("nonexistent", typeof(bool), typeof(DescriptionAttribute).AssemblyQualifiedName, CultureInfo.InvariantCulture);
        Assert.Equal(false, result);
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
        Assert.Same(StswHasAttributeConverter.Instance, instance);
    }
}
