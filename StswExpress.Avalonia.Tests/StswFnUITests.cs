namespace StswExpress.Avalonia.Tests.Utils;
public class StswFnUITests
{
    [Fact]
    public void AppNameAndVersion_ReturnsExpectedFormat()
    {
        var result = StswFnUI.AppNameAndVersion;
        Assert.False(string.IsNullOrWhiteSpace(result));
    }
}
