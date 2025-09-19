using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress.Tests.Utils.MarkupExtensions;
public class StswDynamicResourceExtensionTests
{
    [Fact]
    public void Constructor_NullResourceKey_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new StswDynamicResourceExtension(null!));
    }

    [Fact]
    public void ResourceKey_IsSetCorrectly()
    {
        var ext = new StswDynamicResourceExtension("MyResource");
        Assert.Equal("MyResource", ext.ResourceKey);
    }

    [Fact]
    public void ProvideValue_ReturnsBindingValue_ForFrameworkElement()
    {
        var ext = new StswDynamicResourceExtension("TestResource");
        var serviceProvider = new TestServiceProvider(new TestProvideValueTarget(new TestFrameworkElement()));
        var value = ext.ProvideValue(serviceProvider);
        Assert.NotNull(value);
    }

    [Fact]
    public void ProvideValue_ReturnsMultiBindingValue_WhenNotFrameworkElement()
    {
        var ext = new StswDynamicResourceExtension("TestResource");
        var serviceProvider = new TestServiceProvider(new TestProvideValueTarget(new object()));
        var value = ext.ProvideValue(serviceProvider);
        Assert.NotNull(value);
    }

    [Fact]
    public void ProvideValue_SetsConverterAndStringFormat()
    {
        var ext = new StswDynamicResourceExtension("TestResource")
        {
            Converter = new TestValueConverter(),
            StringFormat = "Value: {0}"
        };
        var serviceProvider = new TestServiceProvider(new TestProvideValueTarget(new TestFrameworkElement()));
        var value = ext.ProvideValue(serviceProvider);
        Assert.NotNull(value);
    }

    [Fact]
    public void WrapperConvert_UsesTargetNullValue_WhenNull()
    {
        var ext = new StswDynamicResourceExtension("TestResource")
        {
            TargetNullValue = "Default"
        };
        var result = ext.GetType()
            .GetMethod("WrapperConvert", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(ext, new object[] { new object[] { null, null, null }, typeof(string), null, CultureInfo.InvariantCulture });
        Assert.Equal("Default", result);
    }

    [Fact]
    public void WrapperConvert_FormatsString_WhenStringFormatProvided()
    {
        var ext = new StswDynamicResourceExtension("TestResource")
        {
            StringFormat = "Value: {0}"
        };
        var result = ext.GetType()
            .GetMethod("WrapperConvert", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(ext, new object[] { new object[] { 42, null, null }, typeof(string), null, CultureInfo.InvariantCulture });
        Assert.Equal("Value: 42", result);
    }

    private class TestFrameworkElement : FrameworkElement
    {
        public TestFrameworkElement()
        {
            Resources = new ResourceDictionary();
        }
    }

    private class TestServiceProvider : IServiceProvider
    {
        private readonly object _service;
        public TestServiceProvider(object service) => _service = service;
        public object? GetService(Type serviceType) => _service;
    }

    private class TestProvideValueTarget : IProvideValueTarget
    {
        public object TargetObject { get; }
        public object? TargetProperty => null;
        public TestProvideValueTarget(object targetObject) => TargetObject = targetObject;
    }

    private class TestValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => "converted";
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
