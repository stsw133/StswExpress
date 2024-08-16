using ClosedXML.Excel;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;

namespace StswExpress;
/// <summary>
/// Provides methods for exporting data to various formats and importing data from these formats.
/// </summary>
public static class StswExport
{
    #region CSV
    /// <summary>
    /// Exports a collection of data to a CSV file.
    /// </summary>
    /// <param name="data">The data collection to be exported.</param>
    /// <param name="filePath">The path to save the exported file. If null, a SaveFileDialog will be shown.</param>
    /// <param name="delimiter">The delimiter to use in the CSV file.</param>
    /// <param name="additionalParameters">Additional parameters for the export operation.</param>
    public static void ExportToCsv(IEnumerable data, string? filePath, char delimiter = ',', StswExportParameters? additionalParameters = null)
    {
        if (filePath == null)
        {
            var dialog = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = additionalParameters?.FileDialogDefaultExt ?? "csv",
                FileName = additionalParameters?.RecommendedFileName ?? string.Empty,
                Filter = additionalParameters?.FileDialogFilter ?? "CSV file (*.csv)|*.csv"
            };

            if (dialog.ShowDialog() == true)
                filePath = dialog.FileName;
            else return;
        }

        using (var writer = new StreamWriter(filePath))
        {
            var elementType = data.GetType().GetGenericArguments().FirstOrDefault() ?? data.GetType().GetElementType();
            if (elementType == null)
                return;

            var properties = elementType.GetProperties();

            /// write header
            writer.WriteLine(string.Join(delimiter, properties.Select(p => p.Name)));

            /// write data
            foreach (var item in data)
                writer.WriteLine(string.Join(delimiter, properties.Select(p => StswBaseDataHandler.EscapeCsvValue(p.GetValue(item)?.ToString()))));
        }

        if (additionalParameters?.OpenFile == true)
            StswFn.OpenFile(filePath);
    }

    /// <summary>
    /// Imports data from a CSV file into a collection of objects.
    /// </summary>
    /// <typeparam name="T">The type of objects to import the data into.</typeparam>
    /// <param name="filePath">The path to the CSV file to import from. If null, an OpenFileDialog will be shown.</param>
    /// <param name="delimiter">The delimiter to use in the CSV file.</param>
    /// <param name="additionalParameters">Additional parameters for the import operation.</param>
    /// <returns>A list of objects of type T populated with data from the CSV file.</returns>
    public static List<T> ImportFromCsv<T>(string? filePath = null, char delimiter = ',', StswExportParameters? additionalParameters = null) where T : new()
    {
        if (filePath == null)
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = additionalParameters?.FileDialogDefaultExt ?? "csv",
                Filter = additionalParameters?.FileDialogFilter ?? "CSV file (*.csv)|*.csv"
            };

            if (dialog.ShowDialog() == true)
                filePath = dialog.FileName;
            else return [];
        }

        var result = new List<T>();
        var properties = typeof(T).GetProperties();

        using (var reader = new StreamReader(filePath))
        {
            var header = reader.ReadLine()?.Split(delimiter);
            if (header == null)
                return result;

            var propertyMap = header.Select(h => properties.FirstOrDefault(p => p.Name.Equals(h, StringComparison.OrdinalIgnoreCase))).ToArray();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null)
                    continue;

                var values = line.Split(delimiter);
                var obj = new T();

                for (int i = 0; i < values.Length; i++)
                {
                    var property = propertyMap[i];
                    if (property != null)
                    {
                        var value = Convert.ChangeType(values[i], property.PropertyType);
                        property.SetValue(obj, value);
                    }
                }

                result.Add(obj);
            }
        }

        return result;
    }
    #endregion

    #region Excel
    /// <summary>
    /// Exports a collection of data sheets to an Excel file.
    /// </summary>
    /// <param name="sheetsWithData">A dictionary containing sheet names and their respective data collections.</param>
    /// <param name="filePath">The path to save the exported file. If null, a SaveFileDialog will be shown.</param>
    /// <param name="additionalParameters">Additional parameters for the export operation.</param>
    public static void ExportToExcel(IDictionary<string, IEnumerable> sheetsWithData, string? filePath, StswExportParameters? additionalParameters = null)
    {
        if (filePath == null)
        {
            var dialog = new SaveFileDialog()
            {
                AddExtension = true,
                DefaultExt = additionalParameters?.FileDialogDefaultExt ?? "xlsx",
                FileName = additionalParameters?.RecommendedFileName ?? string.Empty,
                Filter = additionalParameters?.FileDialogFilter ?? "Excel file (*.xlsx)|*.xlsx"
            };

            if (dialog.ShowDialog() == true)
                filePath = dialog.FileName;
            else return;
        }

        /// export
        using var wb = new XLWorkbook();
        foreach (var sheet in sheetsWithData)
        {
            var ws = wb.AddWorksheet(sheet.Key);
            var elementType = StswBaseDataHandler.GetElementType(sheet.Value);

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

                /// set headers
                for (int col = 0; col < properties.Count; col++)
                {
                    var attribute = properties[col].GetCustomAttribute<StswExportAttribute>();
                    ws.Cell(1, col + 1).Value = attribute?.ColumnName ?? properties[col].Name;
                }

                /// set data
                var row = 2;
                foreach (var item in sheet.Value)
                {
                    for (int col = 0; col < properties.Count; col++)
                    {
                        var value = properties[col].GetValue(item);
                        var cell = ws.Cell(row, col + 1);
                        var attribute = properties[col].GetCustomAttribute<StswExportAttribute>();

                        if (properties[col].PropertyType.IsNumericType())
                        {
                            if (attribute?.ColumnFormat != null)
                                cell.SetValue(Convert.ToDecimal(value).ToString(attribute.ColumnFormat));
                            else
                                cell.SetValue(Convert.ToDecimal(value));
                        }
                        else if (properties[col].PropertyType.In(typeof(DateTime), typeof(DateTime?)))
                        {
                            if (attribute?.ColumnFormat != null)
                                cell.SetValue(Convert.ToDateTime(value).ToString(attribute.ColumnFormat));
                            else
                                cell.SetValue(Convert.ToDateTime(value));
                        }
                        else if (properties[col].PropertyType == typeof(bool) || properties[col].PropertyType == typeof(bool?))
                        {
                            cell.SetValue(StswBaseDataHandler.FormatBool(value, attribute?.ColumnFormat));
                        }
                        else
                        {
                            cell.SetValue(value?.ToString());
                        }
                    }
                    row++;
                }

                /// table and auto column width
                var table = ws.Range(1, 1, row - 1, properties.Count).CreateTable();
                ws.Columns().AdjustToContents();
            }
        }
        wb.SaveAs(filePath);

        if (additionalParameters?.OpenFile == true)
            StswFn.OpenFile(filePath);

        //TODO - memory stream type opening
        /*
        using (var memoryStream = new MemoryStream())
        {
            wb.SaveAs(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            var tempFilePath = Path.GetTempFileName() + ".xlsx";
            File.WriteAllBytes(tempFilePath, memoryStream.ToArray());
            StswFn.OpenFile(tempFilePath);
        }
        */
    }

    /// <summary>
    /// Exports a single data sheet to an Excel file.
    /// </summary>
    /// <param name="sheetName">The name of the sheet to be created in the Excel file.</param>
    /// <param name="sheetData">The data collection to be exported.</param>
    /// <param name="filePath">The path to save the exported file. If null, a SaveFileDialog will be shown.</param>
    /// <param name="parameters">Additional parameters for the export operation.</param>
    public static void ExportToExcel(string sheetName, IEnumerable sheetData, string? filePath, StswExportParameters? parameters = null)
    {
        var sheetsWithData = new Dictionary<string, IEnumerable> { { sheetName, sheetData } };
        ExportToExcel(sheetsWithData, filePath, parameters);
    }

    /// <summary>
    /// Imports data from an Excel file into a collection of objects.
    /// </summary>
    /// <typeparam name="T">The type of objects to import the data into.</typeparam>
    /// <param name="filePath">The path to the Excel file to import from. If null, an OpenFileDialog will be shown.</param>
    /// <param name="sheetName">The name of the sheet to import data from. If null, the first sheet will be used.</param>
    /// <param name="additionalParameters">Additional parameters for the import operation.</param>
    /// <returns>A list of objects of type T populated with data from the Excel file.</returns>
    public static List<T> ImportFromExcel<T>(string? filePath = null, string? sheetName = null, StswExportParameters? additionalParameters = null) where T : new()
    {
        if (filePath == null)
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = additionalParameters?.FileDialogDefaultExt ?? "xlsx",
                Filter = additionalParameters?.FileDialogFilter ?? "Excel file (*.xlsx)|*.xlsx"
            };

            if (dialog.ShowDialog() == true)
                filePath = dialog.FileName;
            else return [];
        }

        var result = new List<T>();

        using var wb = new XLWorkbook(filePath);
        var ws = sheetName == null ? wb.Worksheets.First() : wb.Worksheets.Worksheet(sheetName);
        var properties = typeof(T).GetProperties().Where(p => !p.GetCustomAttributes<StswExportAttribute>().Any(a => a.IsColumnIgnored)).ToList();
        var headerRow = ws.FirstRowUsed();
        var columnMapping = new Dictionary<int, PropertyInfo>();

        foreach (var cell in headerRow.CellsUsed())
        {
            var header = cell.GetString();
            var property = properties.FirstOrDefault(p =>
                p.Name.Equals(header, StringComparison.OrdinalIgnoreCase) ||
                p.GetCustomAttributes<StswExportAttribute>().Any(a => a.ColumnName?.Equals(header, StringComparison.OrdinalIgnoreCase) == true)
            );

            if (property != null)
                columnMapping[cell.Address.ColumnNumber] = property;
        }

        var dataRows = ws.RowsUsed().Skip(1);
        foreach (var dataRow in dataRows)
        {
            var obj = new T();

            foreach (var kvp in columnMapping)
            {
                var cell = dataRow.Cell(kvp.Key);
                var property = kvp.Value;
                var attribute = property.GetCustomAttribute<StswExportAttribute>();
                var value = StswBaseDataHandler.ConvertCellValue(cell, property.PropertyType, attribute?.ColumnFormat);

                property.SetValue(obj, value);
            }

            result.Add(obj);
        }

        return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="worksheet"></param>
    /// <returns></returns>
    public static string ExportToHtml(this IXLWorksheet worksheet)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<table style='border-collapse:collapse;'>");

        var usedRange = worksheet.RangeUsed();
        var firstColumn = usedRange.FirstColumnUsed().ColumnNumber();
        var lastColumn = usedRange.LastColumnUsed().ColumnNumber();

        foreach (var row in worksheet.RowsUsed())
        {
            sb.AppendLine("<tr>");

            for (int colIndex = firstColumn; colIndex <= lastColumn; colIndex++)
            {
                var cell = row.Cell(colIndex);

                if (cell.IsMerged() && !cell.Address.Equals(cell.MergedRange().FirstCell().Address))
                    continue;

                var font = cell.Style.Font;
                var alignment = cell.Style.Alignment;
                var border = cell.Style.Border;
                var isMerged = cell.IsMerged();

                var style = new StringBuilder();

                if (font.Bold) style.Append("font-weight:bold;");
                if (font.Italic) style.Append("font-style:italic;");
                if (font.FontColor.HasValue)
                    style.Append($"color:{StswBaseDataHandler.ColorToHex(font.FontColor)};");
                style.Append($"font-size:{font.FontSize}px;");

                var textAlign = alignment.Horizontal.ToString().ToLower();

                if (cell.DataType == XLDataType.Number)
                    textAlign = "right";

                style.Append($"text-align:{textAlign};");
                style.Append($"vertical-align:{alignment.Vertical.ToString().ToLower()};");

                style.Append($"border:{StswBaseDataHandler.GetBorderWidth(border.TopBorder)} {StswBaseDataHandler.GetBorderStyle(border.TopBorder)} {StswBaseDataHandler.ColorToHex(border.TopBorderColor)};");

                var colSpan = isMerged ? cell.MergedRange().ColumnCount() : 1;
                var rowSpan = isMerged ? cell.MergedRange().RowCount() : 1;

                sb.AppendFormat("<td style='{0}' colspan='{1}' rowspan='{2}'>", style.ToString(), colSpan, rowSpan);
                sb.Append(cell.GetValue<string>());
                sb.AppendLine("</td>");
            }
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</table>");
        return sb.ToString();
    }
    #endregion

    #region JSON
    /// <summary>
    /// Exports a collection of data to a JSON file.
    /// </summary>
    /// <param name="data">The data collection to be exported.</param>
    /// <param name="filePath">The path to save the exported file. If null, a SaveFileDialog will be shown.</param>
    /// <param name="options">Options to control the behavior during serialization to JSON.</param>
    /// <param name="additionalParameters">Additional parameters for the export operation.</param>
    public static void ExportToJson(IEnumerable data, string? filePath, JsonSerializerOptions? options = null, StswExportParameters? additionalParameters = null)
    {
        if (filePath == null)
        {
            var dialog = new SaveFileDialog()
            {
                AddExtension = true,
                DefaultExt = additionalParameters?.FileDialogDefaultExt ?? "json",
                FileName = additionalParameters?.RecommendedFileName ?? string.Empty,
                Filter = additionalParameters?.FileDialogFilter ?? "JSON file (*.json)|*.json"
            };

            if (dialog.ShowDialog() == true)
                filePath = dialog.FileName;
            else return;
        }

        var jsonData = JsonSerializer.Serialize(data, options ?? new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, jsonData);

        if (additionalParameters?.OpenFile == true)
            StswFn.OpenFile(filePath);
    }

    /// <summary>
    /// Imports data from a JSON file into a collection of objects.
    /// </summary>
    /// <typeparam name="T">The type of objects to import the data into.</typeparam>
    /// <param name="filePath">The path to the JSON file to import from. If null, an OpenFileDialog will be shown.</param>
    /// <param name="additionalParameters">Additional parameters for the import operation.</param>
    /// <returns>A list of objects of type T populated with data from the JSON file.</returns>
    public static List<T> ImportFromJson<T>(string? filePath = null, StswExportParameters? additionalParameters = null) where T : new()
    {
        if (filePath == null)
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = additionalParameters?.FileDialogDefaultExt ?? "json",
                Filter = additionalParameters?.FileDialogFilter ?? "JSON file (*.json)|*.json"
            };

            if (dialog.ShowDialog() == true)
                filePath = dialog.FileName;
            else return [];
        }

        var jsonData = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<List<T>>(jsonData) ?? [];
    }
    #endregion

    #region XML
    /// <summary>
    /// Exports a collection of data to an XML file.
    /// </summary>
    /// <param name="data">The data collection to be exported.</param>
    /// <param name="filePath">The path to save the exported file. If null, a SaveFileDialog will be shown.</param>
    /// <param name="additionalParameters">Additional parameters for the export operation.</param>
    public static void ExportToXml(IEnumerable data, string? filePath, StswExportParameters? additionalParameters = null)
    {
        if (filePath == null)
        {
            var dialog = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = additionalParameters?.FileDialogDefaultExt ?? "xml",
                FileName = additionalParameters?.RecommendedFileName ?? string.Empty,
                Filter = additionalParameters?.FileDialogFilter ?? "XML file (*.xml)|*.xml"
            };

            if (dialog.ShowDialog() == true)
                filePath = dialog.FileName;
            else return;
        }

        var elementType = data.GetType().GetGenericArguments().FirstOrDefault() ?? data.GetType().GetElementType();
        if (elementType == null)
            return;

        var serializer = new XmlSerializer(typeof(List<>).MakeGenericType(elementType));
        using (var writer = new StreamWriter(filePath))
            serializer.Serialize(writer, data.Cast<object>().ToList());

        if (additionalParameters?.OpenFile == true)
            StswFn.OpenFile(filePath);
    }

    /// <summary>
    /// Imports data from an XML file into a collection of objects.
    /// </summary>
    /// <typeparam name="T">The type of objects to import the data into.</typeparam>
    /// <param name="filePath">The path to the XML file to import from. If null, an OpenFileDialog will be shown.</param>
    /// <param name="additionalParameters">Additional parameters for the export operation.</param>
    /// <returns>A list of objects of type T populated with data from the XML file.</returns>
    public static List<T> ImportFromXml<T>(string? filePath = null, StswExportParameters? additionalParameters = null) where T : new()
    {
        if (filePath == null)
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = additionalParameters?.FileDialogDefaultExt ?? "xml",
                Filter = additionalParameters?.FileDialogFilter ?? "XML file (*.xml)|*.xml"
            };

            if (dialog.ShowDialog() == true)
                filePath = dialog.FileName;
            else return [];
        }

        var serializer = new XmlSerializer(typeof(List<T>));
        using var reader = new StreamReader(filePath);
        return (List<T>?)serializer.Deserialize(reader) ?? [];
    }
    #endregion
}
