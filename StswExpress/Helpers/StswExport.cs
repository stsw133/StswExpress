using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace StswExpress;

public static class StswExport
{
    /// <summary>
    /// Gets all rows and visible columns from DataGrid and puts into new Excel instance.
    /// </summary>
    /// <param name="dg">DataGrid</param>
    public static void ExportToCsv<T>(this IEnumerable<T> list, string? filePath, bool openFile, string columnSeparator)
    {
        var result = string.Empty;

        if (filePath == null)
        {
            var dialog = new SaveFileDialog()
            {
                Filter = "Pliki CSV|*.csv"
            };
            if (dialog.ShowDialog() == true)
                filePath = dialog.FileName;
            else
                return;
        }

        var properties = typeof(T).GetProperties().Where(x => Attribute.IsDefined(x, typeof(StswExcelAttribute)));
        result = string.Join(columnSeparator, properties.Select(x => x.GetCustomAttribute<StswExcelAttribute>()?.columnName));

        foreach (var elem in list)
        {
            result += Environment.NewLine;

            var line = string.Empty;
            foreach (var property in properties)
            {
                var val = property.GetValue(elem);
                var format = property.GetCustomAttribute<StswExcelAttribute>()?.columnFormat;
                var isFormattable = val is IFormattable f;
                line += ((isFormattable ? (val as dynamic)?.ToString(format) : format != null ? string.Format(format, val.GetHashCode()) : val?.ToString()) ?? string.Empty) + columnSeparator;
            }
            line = line.TrimEnd(columnSeparator);

            result += line;
        }
        using (var sw = new StreamWriter(filePath, false))
            sw.Write(result);

        if (openFile)
            StswFn.OpenFile(filePath);
    }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class StswExcelAttribute : Attribute
{
    public readonly string columnName;
    public readonly string? columnFormat;

    public StswExcelAttribute(string columnName, string? columnFormat = null)
    {
        this.columnName = columnName;
        this.columnFormat = columnFormat;
    }
}
