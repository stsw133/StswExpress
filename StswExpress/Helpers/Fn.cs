using DynamicAero2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace StswExpress;

public static class Fn
{
    #region app & database & mailConfig
    /// App: name & version & name + version & copyright
    public static string? AppName() => Assembly.GetEntryAssembly()?.GetName().Name;
    public static string? AppVersion() => Assembly.GetEntryAssembly()?.GetName().Version?.ToString()?.TrimEnd(".0").TrimEnd(".0").TrimEnd(".0");
    public static string AppNameAndVersion => $"{AppName()} {(AppVersion() != "1" ? AppVersion() : string.Empty)}";
    public static string? AppCopyright => $"{FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).LegalCopyright}";

    /// App: database connection & mail config
    public static DB? AppDB { get; set; } = new();
    public static MC? AppMC { get; set; } = new();
    #endregion

    /*
    /// <summary>
    /// Gets all rows and visible columns from DataGrid and puts into new Excel instance.
    /// </summary>
    /// <param name="dg">DataGrid</param>
    public static void DataGridIntoExcel(ref Window win, ref DataGrid dg)
    {
        var excel = new Excel.Application();
        var wb = excel.Workbooks.Add();
        var ws = (Excel.Worksheet)wb.ActiveSheet;

        win.Cursor = Cursors.Wait;

        int col = 0, row = 0;

        var visColumns = dg.Columns.Where(x => x.Visibility == Visibility.Visible && !string.IsNullOrEmpty(((x as DataGridBoundColumn)?.Binding as Binding)?.Path?.Path ?? "")).ToList();
        for (col = 0; col < visColumns.Count(); col++)
            ws.Range["A1"].Offset[0, col].Value = (visColumns[col]?.Header as ColumnFilter)?.Header ?? "";

        ws.Range["A1", ws.Cells[1, col]].AutoFilter(1);
        ws.Range["A1", ws.Cells[1, col]].Interior.Color = 12_566_463;
        ws.Range["A1", ws.Cells[1, col]].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
        ws.Range["A1", ws.Cells[1, col]].Borders.Weight = 2d;
        ws.Range["A1", ws.Cells[1, col]].Font.Bold = true;
        ws.Range["A2", ws.Cells[ws.Rows.Count, col]].NumberFormat = "@";

        for (row = 0; row < dg.Items.Count; row++)
            for (col = 0; col < visColumns.Count(); col++)
            {
                var binding = (visColumns[col] as DataGridBoundColumn)?.Binding as Binding;
                var value = (dg.Items[row] as DataRowView).Row[(binding?.Path?.Path ?? "")];
                var format = binding?.StringFormat?.Substring(3)?.Replace("}", "");
                ws.Range["A2"].Offset[row, col].Value = $"{(format?.Length > 0 ? $"{(Convert.ToDateTime(value)).ToString(format)}" : value)}";
            }

        excel.ActiveWindow.Zoom = 80;
        ws.Columns.AutoFit();
        excel.Visible = true;
        wb.Activate();

        Marshal.ReleaseComObject(ws);
        Marshal.ReleaseComObject(wb);
        Marshal.ReleaseComObject(excel);

        //wb = null;
        //ws = null;
        //excel = null;
        //GC.Collect();

        win.Cursor = Cursors.Arrow;
    }
    */

    /// <summary>
    /// Starting functions that should be placed in constructor of App class
    /// </summary>
    /// <param name="app">Application class</param>
    public static void AppStart(Application app)
    {
        if (!app.Resources.MergedDictionaries.Any(x => x is Theme))
            app.Resources.MergedDictionaries.Add(new Theme());
        ((Theme)app.Resources.MergedDictionaries.First(x => x is Theme)).Color = (ThemeColor)Settings.Default.Theme;
    }

    /// <summary>
    /// Gets system color chosen by user.
    /// </summary>
    public static Color GetWindowsThemeColor => SystemParameters.WindowGlassColor;

    /// <summary>
    /// Opens context menu of a framework element.
    /// </summary>
    /// <param name="sender">Framework element</param>
    public static void OpenContextMenu(object sender)
    {
        if (sender is FrameworkElement f)
        {
            f.ContextMenu.PlacementTarget = f;
            f.ContextMenu.IsOpen = true;
        }
    }

    /// <summary>
    /// Opens file from path.
    /// </summary>
    /// <param name="path">Path to file</param>
    public static void OpenFile(string path)
    {
        var process = new Process();
        process.StartInfo.FileName = path;
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.Verb = "open";
        process.Start();
    }

    #region ColumnFilters
    /// GetFilters
    public static void GetFilters(DependencyObject panel, out string filter, out List<(string name, object val)> parameters)
    {
        filter = string.Empty;
        parameters = new List<(string name, object val)>();

        foreach (var cf in Extensions.FindVisualChildren<ColumnFilter>(panel).Where(x => x.SqlString != null))
        {
            filter += " and " + cf.SqlString;
            if (cf.Value1 != null)
                parameters.Add((cf.SqlParam[..(cf.SqlParam.Length > 120 ? 120 : cf.SqlParam.Length)] + "1", (cf.Value1 is List<object> ? null : cf.Value1) ?? DBNull.Value));
            if (cf.Value2 != null)
                parameters.Add((cf.SqlParam[..(cf.SqlParam.Length > 120 ? 120 : cf.SqlParam.Length)] + "2", (cf.Value2 is List<object> ? null : cf.Value2) ?? DBNull.Value));
        }

        if (filter.StartsWith(" and "))
            filter = filter[5..];
        if (string.IsNullOrWhiteSpace(filter))
            filter = "1=1";
    }

    /// ClearFilters
    public static void ClearFilters(DependencyObject panel)
    {
        foreach (var cf in Extensions.FindVisualChildren<ColumnFilter>(panel).Where(x => x.SqlString != null))
        {
            cf.Value1 = cf.ValueDef;
            cf.Value2 = cf.ValueDef;
        }
    }
    #endregion
}
