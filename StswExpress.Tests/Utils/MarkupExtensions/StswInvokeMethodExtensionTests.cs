using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace StswExpress.Tests;
public static class TestStaticMethods
{
    public static string ConcatStrings(string a, string b) => a + b;
    public static int AddInts(int x, int y) => x + y;
    public static string FormatName(string first, string last) => $"{first} {last}";
    public static string NoParams() => "NoParams";
}

public class StswInvokeMethodExtensionTests
{
    [Fact]
    public void ProvideValue_WithConstantParameters_InvokesStaticMethod()
    {
        var ext = new StswInvokeMethodExtension
        {
            MethodName = $"{typeof(TestStaticMethods).FullName}.ConcatStrings",
            Parameters = new Collection<object> { "Hello", "World" }
        };

        var value = ext.ProvideValue(null);
        Assert.IsType<MultiBinding>(value as object);
        var mb = (MultiBinding)value;
        var converter = (StswInvokeMethodExtension.InvokeMethodConverter)mb.Converter;
        var result = converter.Convert(new object[] { "Hello", "World" }, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("HelloWorld", result);
    }

    [Fact]
    public void ProvideValue_WithBindingParameter_UsesObjectValueProvider()
    {
        var ext = new StswInvokeMethodExtension
        {
            MethodName = $"{typeof(TestStaticMethods).FullName}.ConcatStrings",
            Parameters = new Collection<object> { new Binding("A"), "B" }
        };

        var value = ext.ProvideValue(null);
        Assert.IsType<MultiBinding>(value as object);
        var mb = (MultiBinding)value;
        Assert.IsType<Binding>(mb.Bindings[0]);
        Assert.IsType<Binding>(mb.Bindings[1]);
    }

    [Fact]
    public void InvokeMethodConverter_ThrowsIfMethodNameNotQualified()
    {
        var converter = new StswInvokeMethodExtension.InvokeMethodConverter
        {
            MethodName = "NotQualified"
        };
        Assert.Throws<ArgumentException>(() =>
            converter.Convert(new object[] { }, typeof(object), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void InvokeMethodConverter_ThrowsIfTypeNotFound()
    {
        var converter = new StswInvokeMethodExtension.InvokeMethodConverter
        {
            MethodName = "FakeNamespace.FakeClass.Method"
        };
        Assert.Throws<InvalidOperationException>(() =>
            converter.Convert(new object[] { }, typeof(object), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void InvokeMethodConverter_ThrowsIfMethodNotFound()
    {
        var converter = new StswInvokeMethodExtension.InvokeMethodConverter
        {
            MethodName = $"{typeof(TestStaticMethods).FullName}.MissingMethod"
        };
        Assert.Throws<MissingMethodException>(() =>
            converter.Convert(new object[] { }, typeof(object), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void InvokeMethodConverter_InvokesMethodWithNoParameters()
    {
        var converter = new StswInvokeMethodExtension.InvokeMethodConverter
        {
            MethodName = $"{typeof(TestStaticMethods).FullName}.NoParams"
        };
        var result = converter.Convert(Array.Empty<object>(), typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("NoParams", result);
    }

    [Fact]
    public void ObjectValueProvider_HoldsValue()
    {
        var obj = new StswInvokeMethodExtension.ObjectValueProvider("test");
        Assert.Equal("test", obj.Value);
    }

    [Fact]
    public void ConvertBack_ThrowsNotSupportedException()
    {
        var converter = new StswInvokeMethodExtension.InvokeMethodConverter();
        Assert.Throws<NotSupportedException>(() =>
            converter.ConvertBack("value", new Type[] { typeof(string) }, null, CultureInfo.InvariantCulture));
    }
}
