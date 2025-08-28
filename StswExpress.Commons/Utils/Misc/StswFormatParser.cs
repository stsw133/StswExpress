using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace StswExpress.Commons;

/// <summary>
/// Provides utility methods for parsing and formatting data in various formats.
/// </summary>
[StswInfo("0.20.0")]
public static class StswFormatParser
{
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
    [StswInfo("0.20.0", IsTested = false)]
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
