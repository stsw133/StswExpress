using System.ComponentModel;
using System.Globalization;

namespace StswExpress.Commons.Tests.Utils.Misc;
public class StswFormatParserTests
{
    private class TestClass
    {
        [Description("Identifier")]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public string? Nullable { get; set; }
    }

    [Fact]
    public void ToCsv_ThrowsArgumentNullException_WhenSourceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => StswFormatParser.ToCsv<TestClass>(null!));
    }

    [Fact]
    public void ToCsv_ReturnsCsvWithHeaders_UsingDescriptionAttribute()
    {
        var items = new[]
        {
            new TestClass { Id = 1, Name = "A", Date = new DateTime(2024, 1, 1), Value = 1.23, Nullable = null }
        };

        var csv = StswFormatParser.ToCsv(items, separator: ';', includeHeaders: true, useDescriptionAttribute: true, culture: CultureInfo.InvariantCulture);

        Assert.StartsWith("Identifier;Name;Date;Value;Nullable", csv.Trim());
        Assert.Contains("1;A;2024-01-01", csv);
    }

    [Fact]
    public void ToCsv_ReturnsCsvWithHeaders_WithoutDescriptionAttribute()
    {
        var items = new[]
        {
            new TestClass { Id = 2, Name = "B", Date = new DateTime(2024, 2, 2), Value = 2.34, Nullable = "X" }
        };

        var csv = StswFormatParser.ToCsv(items, separator: ',', includeHeaders: true, useDescriptionAttribute: false, culture: CultureInfo.InvariantCulture);

        Assert.StartsWith("Id,Name,Date,Value,Nullable", csv.Trim());
        Assert.Contains("2,B,2024-02-02", csv);
    }

    [Fact]
    public void ToCsv_ReturnsCsvWithoutHeaders()
    {
        var items = new[]
        {
            new TestClass { Id = 3, Name = "C", Date = new DateTime(2024, 3, 3), Value = 3.45, Nullable = "Y" }
        };

        var csv = StswFormatParser.ToCsv(items, includeHeaders: false);

        Assert.DoesNotContain("Id", csv);
        Assert.Contains("3;C", csv);
    }

    [Fact]
    public void ToCsv_EscapesValuesWithSeparatorOrQuotes()
    {
        var items = new[]
        {
            new TestClass { Id = 4, Name = "Name;With;Separator", Date = DateTime.Now, Value = 4.56, Nullable = "\"Quoted\"" }
        };

        var csv = StswFormatParser.ToCsv(items, separator: ';', includeHeaders: true);

        Assert.Contains("\"Name;With;Separator\"", csv);
        Assert.Contains("\"\"\"Quoted\"\"\"", csv);
    }

    [Fact]
    public void ToCsv_FormatsValues_UsingCulture()
    {
        var items = new[]
        {
            new TestClass { Id = 5, Name = "Culture", Date = new DateTime(2024, 5, 5), Value = 1234.56 }
        };

        var csvEn = StswFormatParser.ToCsv(items, separator: ';', culture: new CultureInfo("en-US"));
        var csvDe = StswFormatParser.ToCsv(items, separator: ';', culture: new CultureInfo("de-DE"));

        Assert.Contains("1234.56", csvEn);
        Assert.Contains("1234,56", csvDe);
    }

    [Fact]
    public void ToCsv_HandlesEmptyCollection()
    {
        var items = Array.Empty<TestClass>();
        var csv = StswFormatParser.ToCsv(items);

        Assert.NotNull(csv);
        Assert.True(csv.StartsWith("Id;Name;Date;Value;Nullable"));
        Assert.True(csv.Trim().Split('\n').Length == 1); // Only header
    }

    [Fact]
    public void ToCsv_HandlesNullPropertyValues()
    {
        var items = new[]
        {
            new TestClass { Id = 6, Name = null!, Date = DateTime.MinValue, Value = 0, Nullable = null }
        };

        var csv = StswFormatParser.ToCsv(items);

        var line = csv.Split('\n')[1];
        Assert.Contains(";;", line); // Empty cells for nulls
    }
}
