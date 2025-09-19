using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress.Tests.Utils.MarkupExtensions;
public class StswBindableParameterExtensionTests
{
    private class DummyConverter : IValueConverter
    {
        public object? LastValue;
        public object? LastParameter;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            LastValue = value;
            LastParameter = parameter;
            return $"{value}-{parameter}";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [Fact]
    public void Constructor_Default_SetsPropertiesToNull()
    {
        var ext = new StswExpress.StswBindableParameterExtension();
        Assert.Null(ext.Binding);
        Assert.Null(ext.Converter);
        Assert.Null(ext.ConverterParameter);
        Assert.Equal(default, ext.Mode);
    }

    [Fact]
    public void Constructor_Path_SetsBindingPath()
    {
        var ext = new StswExpress.StswBindableParameterExtension("TestPath");
        Assert.NotNull(ext.Binding);
        Assert.Equal("TestPath", ext.Binding.Path.Path);
    }

    [Fact]
    public void Constructor_Binding_SetsBinding()
    {
        var binding = new Binding("SomePath");
        var ext = new StswExpress.StswBindableParameterExtension(binding);
        Assert.Equal(binding, ext.Binding);
    }

    [Fact]
    public void ProvideValue_SingleBinding_ReturnsMultiBindingWithSingleValue()
    {
        var ext = new StswExpress.StswBindableParameterExtension("Value");
        var provider = new DummyServiceProvider();
        var value = ext.ProvideValue(provider);
        Assert.IsType<BindingExpressionBase>(value);
    }

    [Fact]
    public void ProvideValue_WithConverterParameter_UsesParameterInConverter()
    {
        var converter = new DummyConverter();
        var ext = new StswExpress.StswBindableParameterExtension("MainValue")
        {
            Converter = converter,
            ConverterParameter = new Binding("ParamValue")
        };
        var provider = new DummyServiceProvider();
        var value = ext.ProvideValue(provider);
        Assert.IsType<BindingExpressionBase>(value);
    }

    [Fact]
    public void MultiValueConverterAdapter_Convert_ReturnsConvertedValue()
    {
        var converter = new DummyConverter();
        var adapterType = typeof(StswExpress.StswBindableParameterExtension)
            .GetNestedType("MultiValueConverterAdapter", System.Reflection.BindingFlags.NonPublic);
        var adapter = Activator.CreateInstance(adapterType!);
        adapterType!.GetProperty("Converter")!.SetValue(adapter, converter);

        var result = adapterType.GetMethod("Convert")!.Invoke(adapter, new object[]
        {
            new object[] { "A", "B" }, typeof(string), null, CultureInfo.InvariantCulture
        });
        Assert.Equal("A-B", result);
        Assert.Equal("A", converter.LastValue);
        Assert.Equal("B", converter.LastParameter);
    }

    [Fact]
    public void MultiValueConverterAdapter_ConvertBack_ReturnsConvertedBackValue()
    {
        var converter = new DummyConverter();
        var adapterType = typeof(StswExpress.StswBindableParameterExtension)
            .GetNestedType("MultiValueConverterAdapter", System.Reflection.BindingFlags.NonPublic);
        var adapter = Activator.CreateInstance(adapterType!);
        adapterType!.GetProperty("Converter")!.SetValue(adapter, converter);

        var result = adapterType.GetMethod("ConvertBack")!.Invoke(adapter, new object[]
        {
            "C", new Type[] { typeof(string) }, null, CultureInfo.InvariantCulture
        });
        Assert.IsType<object[]>(result);
        Assert.Equal("C", ((object[])result)[0]);
    }

    private class DummyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
