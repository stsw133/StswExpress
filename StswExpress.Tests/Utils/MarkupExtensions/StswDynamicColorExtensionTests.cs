using System.Windows;
using System.Windows.Media;

namespace StswExpress.Tests;
public class StswDynamicColorExtensionTests
{
    [Fact]
    public void ProvideValue_ReturnsColor_WhenResourceExists()
    {
        // Arrange
        var expectedColor = Colors.Red;
        var resourceKey = "TestBrush";
        Application.Current.Resources[resourceKey] = new SolidColorBrush(expectedColor);

        var extension = new StswDynamicColorExtension(resourceKey);

        // Act
        var result = extension.ProvideValue(null);

        // Assert
        Assert.IsType<Color>(result);
        Assert.Equal(expectedColor, (Color)result);
    }

    [Fact]
    public void ProvideValue_ThrowsInvalidOperationException_WhenResourceNotFound()
    {
        // Arrange
        var resourceKey = "NonExistentBrush";
        var extension = new StswDynamicColorExtension(resourceKey);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => extension.ProvideValue(null));
    }

    [Fact]
    public void ProvideValue_ThrowsInvalidOperationException_WhenResourceIsNotSolidColorBrush()
    {
        // Arrange
        var resourceKey = "NotABrush";
        Application.Current.Resources[resourceKey] = "Not a brush";
        var extension = new StswDynamicColorExtension(resourceKey);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => extension.ProvideValue(null));
    }

    [Fact]
    public void Constructor_SetsResourceKey()
    {
        // Arrange
        var resourceKey = "SomeKey";
        var extension = new StswDynamicColorExtension(resourceKey);

        // Assert
        Assert.Equal(resourceKey, extension.ResourceKey);
    }
}
