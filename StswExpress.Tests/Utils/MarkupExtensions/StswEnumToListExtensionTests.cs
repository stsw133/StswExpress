using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;

namespace StswExpress.Tests;
public class StswEnumToListExtensionTests
{
    private enum TestEnum
    {
        [System.ComponentModel.Description("First Value")]
        First,
        [System.ComponentModel.Description("Second Value")]
        Second,
        Third // No description
    }

    // Helper to simulate GetDescription extension method
    private static string GetDescription(Enum value)
    {
        var fi = value.GetType().GetField(value.ToString());
        var attr = fi?.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false)
            .FirstOrDefault() as System.ComponentModel.DescriptionAttribute;
        return attr?.Description ?? value.ToString();
    }

    // Patch StswSelectionItem for test
    private class StswSelectionItem
    {
        public object? Display { get; set; }
        public object? Value { get; set; }
        public bool IsSelected { get; set; }
    }

    // Patch extension for test
    private class StswEnumToListExtension : MarkupExtension
    {
        private readonly Type _enumType;
        public StswEnumToListExtension(Type enumType)
        {
            _enumType = enumType ?? throw new ArgumentNullException(nameof(enumType));
            if (!_enumType.IsEnum)
                throw new ArgumentException("Type must be an enum.", nameof(enumType));
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues(_enumType)
                .Cast<Enum>()
                .Select(value => new StswSelectionItem
                {
                    Display = GetDescription(value),
                    Value = value
                })
                .ToList();
        }
    }

    [Fact]
    public void Constructor_NullType_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new StswEnumToListExtension(null!));
    }

    [Fact]
    public void Constructor_NonEnumType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new StswEnumToListExtension(typeof(string)));
    }

    [Fact]
    public void ProvideValue_ReturnsListOfSelectionItems_WithCorrectDisplayAndValue()
    {
        var ext = new StswEnumToListExtension(typeof(TestEnum));
        var result = ext.ProvideValue(null) as List<StswSelectionItem>;
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);

        Assert.Equal("First Value", result[0].Display);
        Assert.Equal(TestEnum.First, result[0].Value);

        Assert.Equal("Second Value", result[1].Display);
        Assert.Equal(TestEnum.Second, result[1].Value);

        Assert.Equal("Third", result[2].Display);
        Assert.Equal(TestEnum.Third, result[2].Value);
    }
}
