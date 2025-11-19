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

        var lines = csv.Trim().Split('\n');
        Assert.True(lines.Length >= 2);

        var header = lines[0].TrimEnd('\r');
        var data = lines[1].TrimEnd('\r');

        Assert.Equal("Identifier;Name;Date;Value;Nullable", header);

        var cells = data.Split(';');
        Assert.Equal("1", cells[0]);
        Assert.Equal("A", cells[1]);

        var parsedDate = DateTime.Parse(cells[2], CultureInfo.InvariantCulture);
        Assert.Equal(new DateTime(2024, 1, 1), parsedDate);

        Assert.Equal("1.23", cells[3]);
    }

    [Fact]
    public void ToCsv_ReturnsCsvWithHeaders_WithoutDescriptionAttribute()
    {
        var items = new[]
        {
            new TestClass { Id = 2, Name = "B", Date = new DateTime(2024, 2, 2), Value = 2.34, Nullable = "X" }
        };

        var csv = StswFormatParser.ToCsv(items, separator: ',', includeHeaders: true, useDescriptionAttribute: false, culture: CultureInfo.InvariantCulture);

        var lines = csv.Trim().Split('\n');
        Assert.True(lines.Length >= 2);

        var header = lines[0].TrimEnd('\r');
        var data = lines[1].TrimEnd('\r');

        Assert.Equal("Id,Name,Date,Value,Nullable", header);

        var cells = data.Split(',');
        Assert.Equal("2", cells[0]);
        Assert.Equal("B", cells[1]);

        var parsedDate = DateTime.Parse(cells[2], CultureInfo.InvariantCulture);
        Assert.Equal(new DateTime(2024, 2, 2), parsedDate);

        Assert.Equal("2.34", cells[3]);
        Assert.Equal("X", cells[4]);
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
        var trimmed = csv.Trim();
        Assert.StartsWith("Identifier;Name;Date;Value;Nullable", trimmed);
        Assert.Single(trimmed.Split('\n')); // Only header
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

    [Fact]
    public void FromCsv_ThrowsArgumentNullException_WhenCsvIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => StswFormatParser.FromCsv<TestClass>(null!));
    }

    [Fact]
    public void FromCsv_ReturnsObjectsWithHeaders_UsingDescriptionAttribute()
    {
        var csv = "Identifier;Name;Date;Value;Nullable\n1;Alice;2024-01-01;1.23;\n2;Bob;2024-02-02;4.56;Optional";

        var result = StswFormatParser.FromCsv<TestClass>(csv, separator: ';', hasHeaders: true, useDescriptionAttribute: true, culture: CultureInfo.InvariantCulture).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal(4.56, result[1].Value, 3);
    }

    [Fact]
    public void FromCsv_ReturnsObjectsWithHeaders_WithoutDescriptionAttribute()
    {
        var csv = "Id,Name,Date,Value,Nullable\n3,Charlie,2024-03-03,7.89,Y";

        var result = StswFormatParser.FromCsv<TestClass>(csv, separator: ',', hasHeaders: true, useDescriptionAttribute: false, culture: CultureInfo.InvariantCulture).Single();

        Assert.Equal(3, result.Id);
        Assert.Equal("Charlie", result.Name);
        Assert.Equal("Y", result.Nullable);
    }

    [Fact]
    public void FromCsv_ReturnsObjectsWithoutHeaders()
    {
        var csv = "4;Delta;2024-04-04;9.01;Z";

        var result = StswFormatParser.FromCsv<TestClass>(csv, separator: ';', hasHeaders: false, culture: CultureInfo.InvariantCulture).Single();

        Assert.Equal(4, result.Id);
        Assert.Equal("Delta", result.Name);
    }

    [Fact]
    public void FromCsv_ParsesQuotedValues()
    {
        var csv = "Id;Name;Date;Value;Nullable\n5;\"Escaped;Name\";2024-05-05;10.11;\"Quoted\"";

        var result = StswFormatParser.FromCsv<TestClass>(csv, separator: ';', hasHeaders: true, culture: CultureInfo.InvariantCulture).Single();

        Assert.Equal("Escaped;Name", result.Name);
        Assert.Equal("Quoted", result.Nullable);
    }

    [Fact]
    public void FromCsv_UsesProvidedCulture()
    {
        var csv = "Id;Name;Date;Value;Nullable\n6;Culture;2024-06-06;1,23;";

        var culture = new CultureInfo("pl-PL", useUserOverride: false);
        var result = StswFormatParser.FromCsv<TestClass>(csv, separator: ';', hasHeaders: true, culture: culture).Single();

        Assert.Equal(1.23, result.Value, 3);
    }
}
