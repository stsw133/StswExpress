using ClosedXML.Excel;
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
public static class StswExcelFn
{
    /// <summary>
    /// Exports an <see cref="IEnumerable"/> to an Excel file in form of a table.
    /// </summary>
    public static void ExportTo(IDictionary<string, IEnumerable> sheetsWithData, string? filePath, bool openFile, StswExportParameters? additionalParameters = null)
    {
        if (filePath == null)
        {
            var dialog = new SaveFileDialog()
            {
                AddExtension = true,
                DefaultExt = "xlsx",
                FileName = additionalParameters?.RecommendedFileName ?? string.Empty,
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
                        .Where(p => p.Attribute?.IsColumnIgnored == false || (additionalParameters?.IncludeNonAttributed == true && p.Attribute?.IsColumnIgnored != true))
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
                            if (properties[col].PropertyType.IsNumericType())
                                ws.Cell(row, col + 1).SetValue(Convert.ToDecimal(value));
                            else if (properties[col].PropertyType.In(typeof(DateTime), typeof(DateTime?)))
                                ws.Cell(row, col + 1).SetValue(Convert.ToDateTime(value));
                            else
                                ws.Cell(row, col + 1).SetValue(Convert.ToString(value));

                            if (attribute?.ColumnFormat != null)
                                ws.Cell(row, col + 1).Style.NumberFormat.Format = attribute.ColumnFormat;
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
    public static void ExportTo((string name, IEnumerable data) sheetWithData, string? filePath, bool openFile, StswExportParameters? parameters = null)
    {
        var dict = new Dictionary<string, IEnumerable>
        {
            { sheetWithData.name, sheetWithData.data }
        };
        ExportTo(dict, filePath, openFile, parameters);
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
/// A class that is a attribute used for <see cref="StswExcelFn"/> function to control exporting behavior for specific properties.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class StswExportAttribute(string? columnName = null, string? columnFormat = null, bool isColumnIgnored = false, int order = 0) : Attribute
{
    /// <summary>
    /// Gets or sets the custom name to be used as the column header in the exported Excel file.
    /// </summary>
    public string? ColumnName { get; set; } = columnName;

    /// <summary>
    /// Gets or sets the custom format string to be used for formatting the property's value in the exported Excel file.
    /// </summary>
    public string? ColumnFormat { get; set; } = columnFormat;

    /// <summary>
    /// Gets or sets a flag indicating whether to ignore exporting the property as a column in the Excel file.
    /// </summary>
    public bool IsColumnIgnored { get; set; } = isColumnIgnored;

    /// <summary>
    /// Gets or sets the order in which the column should appear in the exported Excel file.
    /// </summary>
    public int Order { get; set; } = order;
}

/// <summary>
/// A class for additional export parameters such as default name of file.
/// </summary>
public class StswExportParameters
{
    public bool IncludeNonAttributed { get; set; } = false;
    public string RecommendedFileName { get; set; } = string.Empty;
}