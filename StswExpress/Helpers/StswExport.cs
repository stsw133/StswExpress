using ClosedXML.Excel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StswExpress;

public static class StswExport
{
    /// ExportToExcel
    public static void ExportToExcel<T>(IEnumerable<T> data, string? filePath, bool openFile, List<StswExportColumn>? customColumns)
    {
        if (filePath == null)
        {
            var dialog = new SaveFileDialog()
            {
                Filter = "Excel file (XLSX)|*.xlsx"
            };
            if (dialog.ShowDialog() == true)
                filePath = dialog.FileName;
            else
                return;
        }

        using (var wb = new XLWorkbook())
        {
            /// new workbook and sheets
            var ws = wb.AddWorksheet("Sheet1");

            /// properties of T
            var properties = typeof(T).GetProperties().Where(x => customColumns == null || x.Name.In(customColumns.Select(y => y.FieldName))).ToList();
            
            var row = 1;

            /// headers
            for (int i = 0; i < properties.Count; i++)
                ws.Cell(row, i + 1).Value = customColumns == null ? properties[i].Name : customColumns.First(x => x.FieldName == properties[i].Name).ColumnName ?? string.Empty;
            
            /// elements
            foreach (var item in data)
            {
                ++row;
                for (int i = 0; i < properties.Count; i++)
                {
                    var value = properties[i].GetValue(item);
                    var format = customColumns?.FirstOrDefault(x => x.FieldName == properties[i].Name)?.ColumnFormat;

                    string valueAsString;
                    if (format != null && value is IFormattable f)
                        valueAsString = (value as dynamic).ToString(format);
                    else if (format != null && value != null)
                        valueAsString = string.Format(format, value.GetHashCode());
                    else
                        valueAsString = value?.ToString() ?? string.Empty;

                    ws.Cell(row, i + 1).Value = valueAsString;
                }
            }

            /// table and auto column width
            var table = ws.Range(1, 1, row, properties.Count).CreateTable();
            ws.Columns().AdjustToContents();

            /// save
            wb.SaveAs(filePath);
        }
        if (openFile)
            StswFn.OpenFile(filePath);
    }
}

public class StswExportColumn
{
    public string? FieldName { get; set; }
    public string? ColumnName { get; set; }
    public string? ColumnFormat { get; set; }
}
