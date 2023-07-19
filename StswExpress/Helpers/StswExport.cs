using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StswExpress;

/// <summary>
/// Provides a method for exporting data to various file formats.
/// </summary>
public static class StswExport
{
    /// <summary>
    /// Exports an <see cref="IEnumerable"/> to an Excel file in form of a table.
    /// </summary>
    public static void ExportToExcel(IDictionary<string, IEnumerable> sheetsWithData, string? filePath, bool openFile)
    {
        if (filePath == null)
        {
            var dialog = new SaveFileDialog()
            {
                Filter = "Excel file (*.xlsx)|*.xlsx"
            };
            if (dialog.ShowDialog() == true)
                filePath = dialog.FileName;
            else return;
        }

        using (var wb = new XLWorkbook())
        {
            /// sheets
            foreach (var sheet in sheetsWithData)
            {
                var ws = wb.AddWorksheet(sheet.Key);

                var elementType = GetElementType(sheet.Value);
                if (elementType != null)
                {
                    /// properties
                    var properties = elementType.GetProperties()
                        .Select(p => new
                        {
                            Property = p,
                            Attribute = p.GetCustomAttribute<StswExportAttribute>(),
                            Order = p.GetCustomAttribute<StswExportAttribute>()?.Order ?? 0
                        })
                        .Where(p => p.Attribute == null || !p.Attribute.IsColumnIgnored)
                        .OrderBy(p => p.Order)
                        .Select(p => p.Property)
                        .ToList();

                    int row = 0;

                    /// headers + elements
                    for (int col = 0; col < properties.Count; col++)
                    {
                        var attribute = properties[col].GetCustomAttribute<StswExportAttribute>();

                        row = 1;
                        ws.Cell(row, col + 1).Value = attribute?.ColumnName ?? properties[col].Name;

                        /// elements
                        foreach (var item in sheet.Value)
                        {
                            ++row;

                            var value = properties[col].GetValue(item);

                            string valueAsString;
                            if (attribute?.ColumnFormat != null)
                            {
                                if (value is IFormattable f)
                                    valueAsString = (value as dynamic).ToString(attribute?.ColumnFormat);
                                else if (value != null)
                                    valueAsString = string.Format(attribute.ColumnFormat, value.GetHashCode());
                                else
                                    valueAsString = value?.ToString() ?? string.Empty;
                            }
                            else
                                valueAsString = value?.ToString() ?? string.Empty;

                            ws.Cell(row, col + 1).Value = valueAsString;
                        }
                    }

                    /// table and auto column width
                    var table = ws.Range(1, 1, row, properties.Count).CreateTable();
                    ws.Columns().AdjustToContents();
                }
            }

            /// save
            wb.SaveAs(filePath);
        }
        if (openFile)
            StswFn.OpenFile(filePath);
    }
    /// <summary>
    /// Exports an <see cref="IEnumerable"/> to an Excel file in form of a table.
    /// </summary>
    public static void ExportToExcel((string name, IEnumerable data) sheetWithData, string? filePath, bool openFile)
    {
        var dict = new Dictionary<string, IEnumerable>
        {
            { sheetWithData.name, sheetWithData.data }
        };
        ExportToExcel(dict, filePath, openFile);
    }

    /// <summary>
    /// Gets type of IEnumerable.
    /// </summary>
    private static Type? GetElementType(IEnumerable enumerable)
    {
        var enumerableType = enumerable.GetType();
        if (enumerableType.IsGenericType)
        {
            var typeArgs = enumerableType.GetGenericArguments();
            if (typeArgs.Length > 0)
                return typeArgs[0];
        }
        return null;
    }
}

/// <summary>
/// A class that is a attribute used for <see cref="StswExport"/> function to control exporting behavior for specific properties.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class StswExportAttribute : Attribute
{
    public StswExportAttribute(string? columnName = null, string? columnFormat = null, bool isColumnIgnored = false, int order = 0)
    {
        ColumnName = columnName;
        ColumnFormat = columnFormat;
        IsColumnIgnored = isColumnIgnored;
        Order = order;
    }

    /// <summary>
    /// Gets or sets the custom name to be used as the column header in the exported Excel file.
    /// </summary>
    public string? ColumnName { get; set; }

    /// <summary>
    /// Gets or sets the custom format string to be used for formatting the property's value in the exported Excel file.
    /// </summary>
    public string? ColumnFormat { get; set; }

    /// <summary>
    /// Gets or sets a flag indicating whether to ignore exporting the property as a column in the Excel file.
    /// </summary>
    public bool IsColumnIgnored { get; set; }

    /// <summary>
    /// Gets or sets the order in which the column should appear in the exported Excel file.
    /// </summary>
    public int Order { get; set; }
}
