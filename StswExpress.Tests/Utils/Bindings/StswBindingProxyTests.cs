using System.Windows;

namespace StswExpress.Tests;
public class StswBindingProxyTests
{
    /*
    [Fact]
    public void CreateInstanceCore_ReturnsNewInstance()
    {
        // Arrange
        var proxy = new StswBindingProxy();

        // Act
        var instance = proxy.CreateInstanceCore();

        // Assert
        Assert.NotNull(instance);
        Assert.IsType<StswBindingProxy>(instance);
        Assert.NotSame(proxy, instance);
    }
    */
    [Fact]
    public void ProxyProperty_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var proxy = new StswBindingProxy();
        var testObject = new object();

        // Act
        proxy.Proxy = testObject;

        // Assert
        Assert.Same(testObject, proxy.Proxy);
    }

    [Fact]
    public void ProxyProperty_DefaultValue_IsNull()
    {
        // Arrange
        var proxy = new StswBindingProxy();

        // Act & Assert
        Assert.Null(proxy.Proxy);
    }

    [Fact]
    public void ProxyProperty_IsDependencyProperty()
    {
        // Assert
        Assert.NotNull(StswBindingProxy.ProxyProperty);
        Assert.Equal("Proxy", StswBindingProxy.ProxyProperty.Name);
        Assert.Equal(typeof(object), StswBindingProxy.ProxyProperty.PropertyType);
        Assert.Equal(typeof(StswBindingProxy), StswBindingProxy.ProxyProperty.OwnerType);
    }
}
