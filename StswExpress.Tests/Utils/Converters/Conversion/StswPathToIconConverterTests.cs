using Moq;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace StswExpress.Tests;
public class StswPathToIconConverterTests
{
    private readonly StswPathToIconConverter _converter = StswPathToIconConverter.Instance;

    [Fact]
    public void ProvideValue_ReturnsSingletonInstance()
    {
        var instance = _converter.ProvideValue(null);
        Assert.Same(StswPathToIconConverter.Instance, instance);
    }

    [Fact]
    public void Convert_NullOrWhitespacePath_ReturnsBindingDoNothing()
    {
        Assert.Equal(Binding.DoNothing, _converter.Convert(null, typeof(ImageSource), null, CultureInfo.InvariantCulture));
        Assert.Equal(Binding.DoNothing, _converter.Convert("", typeof(ImageSource), null, CultureInfo.InvariantCulture));
        Assert.Equal(Binding.DoNothing, _converter.Convert("   ", typeof(ImageSource), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Convert_NonStringValue_ReturnsBindingDoNothing()
    {
        Assert.Equal(Binding.DoNothing, _converter.Convert(123, typeof(ImageSource), null, CultureInfo.InvariantCulture));
        Assert.Equal(Binding.DoNothing, _converter.Convert(new object(), typeof(ImageSource), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Convert_ValidPath_ReturnsImageSource()
    {
        // Arrange
        var expectedImageSource = Mock.Of<ImageSource>();
        var iconMock = new Mock<IStswIcon>();
        iconMock.Setup(i => i.ToImageSource()).Returns(expectedImageSource);

        // Patch StswFnUI.ExtractAssociatedIcon to return our mock icon
        StswFnUI.ExtractAssociatedIcon = path => iconMock.Object;

        // Act
        var result = _converter.Convert("C:\\Windows\\explorer.exe", typeof(ImageSource), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(expectedImageSource, result);

        // Cleanup
        StswFnUI.ExtractAssociatedIcon = null;
    }

    [Fact]
    public void Convert_ValidPath_IconIsNull_ReturnsBindingDoNothing()
    {
        // Patch StswFnUI.ExtractAssociatedIcon to return null
        StswFnUI.ExtractAssociatedIcon = path => null;

        var result = _converter.Convert("C:\\Windows\\explorer.exe", typeof(ImageSource), null, CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);

        // Cleanup
        StswFnUI.ExtractAssociatedIcon = null;
    }

    [Fact]
    public void ConvertBack_Always_ReturnsBindingDoNothing()
    {
        var result = _converter.ConvertBack(new object(), typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    // Patchable static for testability
    private static class StswFnUI
    {
        public static Func<string, IStswIcon?> ExtractAssociatedIcon;
    }
}

// You may need to add these test helpers/mocks in your test project:
public interface IStswIcon
{
    ImageSource ToImageSource();
}
