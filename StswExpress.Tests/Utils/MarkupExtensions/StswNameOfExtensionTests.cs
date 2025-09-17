using System;
using System.Reflection;
using System.Windows.Markup;

namespace StswExpress.Tests;
public class StswNameOfExtensionTests
{
    private class TestClass
    {
        public string MyProperty { get; set; }
        public static int StaticField;
    }

    [Fact]
    public void ProvideValue_ReturnsMember_WhenPropertyExists()
    {
        var ext = new StswExpress.StswNameOfExtension("MyProperty")
        {
            Type = typeof(TestClass)
        };

        var result = ext.ProvideValue(new DummyServiceProvider());
        Assert.Equal("MyProperty", result);
    }

    [Fact]
    public void ProvideValue_ReturnsMember_WhenFieldExists()
    {
        var ext = new StswExpress.StswNameOfExtension("StaticField")
        {
            Type = typeof(TestClass)
        };

        var result = ext.ProvideValue(new DummyServiceProvider());
        Assert.Equal("StaticField", result);
    }

    [Fact]
    public void ProvideValue_ThrowsArgumentException_WhenTypeIsNull()
    {
        var ext = new StswExpress.StswNameOfExtension("MyProperty");
        Assert.Throws<ArgumentException>(() => ext.ProvideValue(new DummyServiceProvider()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("My.Property")]
    public void ProvideValue_ThrowsArgumentException_WhenMemberIsInvalid(string member)
    {
        var ext = new StswExpress.StswNameOfExtension(member)
        {
            Type = typeof(TestClass)
        };
        Assert.Throws<ArgumentException>(() => ext.ProvideValue(new DummyServiceProvider()));
    }

    [Fact]
    public void ProvideValue_ThrowsArgumentException_WhenMemberNotFound()
    {
        var ext = new StswExpress.StswNameOfExtension("NonExistent")
        {
            Type = typeof(TestClass)
        };
        Assert.Throws<ArgumentException>(() => ext.ProvideValue(new DummyServiceProvider()));
    }

    [Fact]
    public void ProvideValue_ThrowsArgumentNullException_WhenServiceProviderIsNull()
    {
        var ext = new StswExpress.StswNameOfExtension("MyProperty")
        {
            Type = typeof(TestClass)
        };
        Assert.Throws<ArgumentNullException>(() => ext.ProvideValue(null!));
    }

    private class DummyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
