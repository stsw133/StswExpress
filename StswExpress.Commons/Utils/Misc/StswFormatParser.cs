using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace StswExpress.Commons;

/// <summary>
/// Provides utility methods for parsing and formatting data in various formats.
/// </summary>
public static class StswFormatParser
{
    /// <summary>
    /// Builds a collection of objects from a CSV string.
    /// </summary>
    /// <typeparam name="T">The type of the objects to create.</typeparam>
    /// <param name="csv">The CSV string to parse.</param>
    /// <param name="separator">The character used to separate values in the CSV. Default is ';'.</param>
    /// <param name="hasHeaders">Whether the CSV string includes a header row. Default is <see langword="true"/>.</param>
    /// <param name="useDescriptionAttribute">Whether to use the <see cref="DescriptionAttribute"/> for header mapping if available. Default is <see langword="true"/>.</param>
    /// <param name="culture">The culture to use for parsing values. Default is the current culture.</param>
    /// <returns>A collection of objects represented by the CSV rows.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="csv"/> is <see langword="null"/>.</exception>
    public static IEnumerable<T> FromCsv<T>(string csv, char separator = ';', bool hasHeaders = true, bool useDescriptionAttribute = true, CultureInfo? culture = null) where T : new()
    {
        ArgumentNullException.ThrowIfNull(csv);
        culture ??= CultureInfo.CurrentCulture;

        var props = typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0)
            .OrderBy(p => p.MetadataToken)
            .ToArray();

        static List<string[]> ParseRecords(string input, char separator)
        {
            var records = new List<string[]>();
            var field = new StringBuilder();
            var row = new List<string>();
            var inQuotes = false;

            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < input.Length && input[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == separator && !inQuotes)
                {
                    row.Add(field.ToString());
                    field.Clear();
                }
                else if ((c == '\r' || c == '\n') && !inQuotes)
                {
                    row.Add(field.ToString());
                    field.Clear();
                    records.Add([.. row]);
                    row.Clear();

                    if (c == '\r' && i + 1 < input.Length && input[i + 1] == '\n')
                        i++;
                }
                else
                {
                    field.Append(c);
                }
            }

            row.Add(field.ToString());
            var hasData = row.Count > 1 || (row.Count == 1 && row[0].Length > 0);
            if (hasData)
                records.Add([.. row]);

            return records;
        }

        var records = ParseRecords(csv, separator);
        if (records.Count == 0)
            return [];

        PropertyInfo?[] columnMappings;
        var dataStartIndex = 0;

        if (hasHeaders)
        {
            var headerRow = records[0];
            dataStartIndex = 1;
            columnMappings = new PropertyInfo?[headerRow.Length];

            static string? GetDescription(PropertyInfo property) => property.GetCustomAttribute<DescriptionAttribute>(inherit: true)?.Description;

            for (var i = 0; i < headerRow.Length; i++)
            {
                var header = headerRow[i];
                PropertyInfo? match = null;

                if (useDescriptionAttribute)
                {
                    match = props.FirstOrDefault(p => string.Equals(GetDescription(p), header, StringComparison.Ordinal));
                }

                match ??= props.FirstOrDefault(p => string.Equals(p.Name, header, StringComparison.Ordinal));
                columnMappings[i] = match;
            }
        }
        else
        {
            columnMappings = props;
        }

        var converterCache = new Dictionary<Type, TypeConverter>();
        var result = new List<T>(records.Count - dataStartIndex);

        for (var recordIndex = dataStartIndex; recordIndex < records.Count; recordIndex++)
        {
            var record = records[recordIndex];
            var instance = new T();

            for (var columnIndex = 0; columnIndex < record.Length; columnIndex++)
            {
                var property = hasHeaders
                    ? columnIndex < columnMappings.Length ? columnMappings[columnIndex] : null
                    : columnIndex < props.Length ? props[columnIndex] : null;

                if (property is null)
                    continue;

                var rawValue = record[columnIndex];
                if (string.IsNullOrEmpty(rawValue))
                {
                    property.SetValue(instance, property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) is null
                        ? Activator.CreateInstance(property.PropertyType)
                        : null);
                    continue;
                }

                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                if (!converterCache.TryGetValue(targetType, out var converter))
                {
                    converter = TypeDescriptor.GetConverter(targetType);
                    converterCache[targetType] = converter;
                }

                var convertedValue = converter.ConvertFrom(null, culture, rawValue);
                property.SetValue(instance, convertedValue);
            }

            result.Add(instance);
        }

        return result;
    }

    /// <summary>
    /// Builds a CSV string from a collection of objects.
    /// </summary>
    /// <typeparam name="T">The type of the objects in the collection.</typeparam>
    /// <param name="source">The collection of objects to convert to CSV.</param>
    /// <param name="separator">The character used to separate values in the CSV. Default is ';'.</param>
    /// <param name="includeHeaders">Whether to include a header row with property names. Default is <see langword="true"/>.</param>
    /// <param name="useDescriptionAttribute">Whether to use the <see cref="DescriptionAttribute"/> for headers if available. Default is <see langword="true"/>.</param>
    /// <param name="culture">The culture to use for formatting values. Default is the current culture.</param>
    /// <returns>A CSV string representing the collection of objects.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="source"/> is <see langword="null"/>.</exception>
    public static string ToCsv<T>(IEnumerable<T> source, char separator = ';', bool includeHeaders = true, bool useDescriptionAttribute = true, CultureInfo? culture = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        culture ??= CultureInfo.CurrentCulture;

        var props = typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .OrderBy(p => p.MetadataToken)
            .ToArray();

        static string Escape(string input, char separator)
        {
            var mustQuote = input.Contains(separator)
                || input.Contains('"')
                || input.Contains('\r')
                || input.Contains('\n')
                || (input.Length > 0 && (char.IsWhiteSpace(input[0]) || char.IsWhiteSpace(input[^1])));

            if (!mustQuote)
                return input;

            var escaped = input.Replace("\"", "\"\"");
            return $"\"{escaped}\"";
        }

        var sb = new StringBuilder(4096);
        if (includeHeaders)
        {
            var headers = props.Select(p =>
            {
                if (useDescriptionAttribute)
                {
                    var desc = p.GetCustomAttribute<DescriptionAttribute>(inherit: true)?.Description;
                    if (!string.IsNullOrWhiteSpace(desc))
                        return Escape(desc!, separator);
                }
                return Escape(p.Name, separator);
            });

            sb.AppendJoin(separator, headers);
            sb.AppendLine();
        }

        foreach (var item in source)
        {
            var cells = props.Select(p =>
            {
                var value = p.GetValue(item, null);
                if (value is null)
                    return string.Empty;

                var str = value is IFormattable f
                    ? f.ToString(null, culture)
                    : value.ToString();

                return Escape(str ?? string.Empty, separator);
            });

            sb.AppendJoin(separator, cells);
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
