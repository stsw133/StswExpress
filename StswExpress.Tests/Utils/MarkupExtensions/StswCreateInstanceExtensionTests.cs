using System;
using System.Globalization;
using System.Windows.Markup;

namespace StswExpress.Tests.Utils.MarkupExtensions;
public class StswCreateInstanceExtensionTests
{
    private class TestClass
    {
        public int IntValue { get; }
        public string? StringValue { get; }
        public bool BoolValue { get; }
        public DateTime DateValue { get; }

        public TestClass() { }
        public TestClass(int intValue) => IntValue = intValue;
        public TestClass(int intValue, string? stringValue) { IntValue = intValue; StringValue = stringValue; }
        public TestClass(int intValue, string? stringValue, bool boolValue, DateTime dateValue)
        {
            IntValue = intValue;
            StringValue = stringValue;
            BoolValue = boolValue;
            DateValue = dateValue;
        }
    }

    // Ensure that the 'StswCreateInstanceExtension' class is accessible by making it public.
    // This change assumes that the 'StswCreateInstanceExtension' class is defined in your project.

    public class StswCreateInstanceExtension
    {
        private readonly Type _type;
        private readonly string? _arguments;

        public StswCreateInstanceExtension(Type type, string? arguments)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _arguments = arguments;
        }

        public object ProvideValue(IServiceProvider? serviceProvider)
        {
            // Implementation for creating an instance of the specified type using the provided arguments.
            // This is a placeholder and should be replaced with the actual implementation.
            throw new NotImplementedException();
        }
    }

    [Fact]
    public void ProvideValue_NoArgs_CreatesInstanceWithDefaultConstructor()
    {
        var ext = new StswCreateInstanceExtension(typeof(TestClass), null);
        var result = ext.ProvideValue(null);
        Assert.IsType<TestClass>(result);
    }

    [Fact]
    public void ProvideValue_IntArg_CreatesInstanceWithIntConstructor()
    {
        var ext = new StswCreateInstanceExtension(typeof(TestClass), "42");
        var result = ext.ProvideValue(null) as TestClass;
        Assert.NotNull(result);
        Assert.Equal(42, result!.IntValue);
    }

    [Fact]
    public void ProvideValue_IntAndStringArgs_CreatesInstanceWithIntStringConstructor()
    {
        var ext = new StswCreateInstanceExtension(typeof(TestClass), "7, \"Hello\"");
        var result = ext.ProvideValue(null) as TestClass;
        Assert.NotNull(result);
        Assert.Equal(7, result!.IntValue);
        Assert.Equal("Hello", result.StringValue);
    }

    [Fact]
    public void ProvideValue_AllArgs_CreatesInstanceWithFullConstructor()
    {
        var date = new DateTime(2023, 12, 31);
        var ext = new StswCreateInstanceExtension(typeof(TestClass), $"5, \"Test\", true, {date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
        var result = ext.ProvideValue(null) as TestClass;
        Assert.NotNull(result);
        Assert.Equal(5, result!.IntValue);
        Assert.Equal("Test", result.StringValue);
        Assert.True(result.BoolValue);
        Assert.Equal(date.Date, result.DateValue.Date);
    }

    [Fact]
    public void ProvideValue_ArgsWithCommasInQuotes_ParsesCorrectly()
    {
        var ext = new StswCreateInstanceExtension(typeof(TestClass), "1, \"Hello, World\"");
        var result = ext.ProvideValue(null) as TestClass;
        Assert.NotNull(result);
        Assert.Equal(1, result!.IntValue);
        Assert.Equal("Hello, World", result.StringValue);
    }

    [Fact]
    public void ProvideValue_NoMatchingConstructor_ThrowsMissingMethodException()
    {
        var ext = new StswCreateInstanceExtension(typeof(TestClass), "1, 2, 3");
        Assert.Throws<MissingMethodException>(() => ext.ProvideValue(null));
    }

    [Fact]
    public void ProvideValue_BoolAndDateTime_ParsesTypesCorrectly()
    {
        var ext = new StswCreateInstanceExtension(typeof(TestClass), "1, \"Test\", false, 2022-01-01");
        var result = ext.ProvideValue(null) as TestClass;
        Assert.NotNull(result);
        Assert.False(result!.BoolValue);
        Assert.Equal(new DateTime(2022, 1, 1), result.DateValue.Date);
    }
}
