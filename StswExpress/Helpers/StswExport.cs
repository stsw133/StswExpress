using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StswExpress;

public static class StswExport
{
    /// ExportToExcel
    public static void ExportToExcel<T>(IEnumerable<T> data, string? filePath, bool openFile, List<StswExcelColumns>? columns)
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

        using (var document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
        {
            /// new workbook and sheets
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();
            worksheetPart.Worksheet = new Worksheet(sheetData);

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            var sheet = new Sheet()
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Sheet1"
            };
            sheets.Append(sheet);

            /// properties of T
            var properties = typeof(T).GetProperties().Where(x => columns == null || x.Name.In(columns.Select(y => y.FieldName)));

            /// headers
            var row = new Row();
            foreach (var property in properties)
            {
                row.Append(new Cell
                {
                    DataType = CellValues.String,
                    CellValue = new CellValue(columns == null ? property.Name : columns.First(x => x.FieldName == property.Name).ColumnName ?? string.Empty)
                });
            }
            sheetData.AppendChild(row);

            /// elements
            foreach (var item in data)
            {
                row = new Row();
                foreach (var property in properties)
                {
                    var value = property.GetValue(item);
                    var format = columns?.FirstOrDefault(x => x.FieldName == property.Name)?.ColumnFormat;

                    string valueAsString;
                    if (format != null && value is IFormattable f)
                        valueAsString = (value as dynamic).ToString(format);
                    else if (format != null && value != null)
                        valueAsString = string.Format(format, value.GetHashCode());
                    else
                        valueAsString = value?.ToString() ?? string.Empty;
                    
                    row.Append(new Cell
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(valueAsString)
                    });
                }
                sheetData.AppendChild(row);
            }

            /// save
            workbookPart.Workbook.Save();
        }
        if (openFile)
            StswFn.OpenFile(filePath);
    }
}

public class StswExcelColumns
{
    public string? FieldName { get; set; }
    public string? ColumnName { get; set; }
    public string? ColumnFormat { get; set; }
}
