using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StswExpress
{
    public static class Fn
    {
        #region App & Database
        /// App: name & version & name + version & copyright
        public static string? AppName() => Assembly.GetEntryAssembly()?.GetName().Name;
        public static string? AppVersion() => Assembly.GetEntryAssembly()?.GetName().Version?.ToString()?.TrimEnd(".0".ToCharArray());
        public static string AppNameAndVersion => $"{AppName()} {(AppVersion() != "1" ? AppVersion() : string.Empty)}";
        public static string? AppCopyright => $"{FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).LegalCopyright}";

        /// App: database connection & mail config
        public static DB? AppDatabase { get; set; } = new();
        public static MC? AppMailConfig { get; set; } = new();
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
        /// Gets color of system color chosen by user
        /// </summary>
        public static Color GetWindowsThemeColor => SystemParameters.WindowGlassColor;

        /// <summary>
        /// Loads image from byte[] to BitmapImage.
        /// </summary>
        /// <param name="imageData">Byte array data</param>
        /// <returns>Image</returns>
        public static BitmapImage? LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();

            return image;
        }

        /// <summary>
        /// Opens context menu of framework element.
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

        #region filters
        /// <summary>
        /// Gets column filters from DataGrid.
        /// </summary>
        /// <param name="dg">DataGrid</param>
        /// <param name="filter">SQL filter as string</param>
        /// <param name="parameters">SQL parameters</param>
        public static void GetColumnFilters(DataGrid dg, out string filter, out List<(string name, object val)> parameters)
        {
            filter = string.Empty;
            parameters = new List<(string name, object val)>();

            foreach (var col in dg.Columns)
            {
                if (col.Header is ColumnFilter cf1)
                {
                    if (cf1.FilterSQL != null)
                    {
                        filter += " and " + cf1.FilterSQL;
                        if (cf1.Value1 != null)
                            parameters.Add((cf1.ParamSQL.Substring(0, cf1.ParamSQL.Length > 120 ? 120 : cf1.ParamSQL.Length) + "1", (cf1.Value1 is List<object> ? null : cf1.Value1) ?? DBNull.Value));
                        if (cf1.Value2 != null)
                            parameters.Add((cf1.ParamSQL.Substring(0, cf1.ParamSQL.Length > 120 ? 120 : cf1.ParamSQL.Length) + "2", (cf1.Value2 is List<object> ? null : cf1.Value2) ?? DBNull.Value));
                    }
                }
                else if (col.Header is DependencyObject)
                {
                    foreach (var cf2 in Extensions.FindVisualChildren<ColumnFilter>((DependencyObject)col.Header).Where(x => x.FilterSQL != null))
                    {
                        filter += " and " + cf2.FilterSQL;
                        if (cf2.Value1 != null)
                            parameters.Add((cf2.ParamSQL.Substring(0, cf2.ParamSQL.Length > 120 ? 120 : cf2.ParamSQL.Length) + "1", (cf2.Value1 is List<object> ? null : cf2.Value1) ?? DBNull.Value));
                        if (cf2.Value2 != null)
                            parameters.Add((cf2.ParamSQL.Substring(0, cf2.ParamSQL.Length > 120 ? 120 : cf2.ParamSQL.Length) + "2", (cf2.Value2 is List<object> ? null : cf2.Value2) ?? DBNull.Value));
                    }
                }
            }

            if (filter.StartsWith(" and "))
                filter = filter[5..];
            if (string.IsNullOrWhiteSpace(filter))
                filter = "1=1";
        }

        /// <summary>
        /// Clears column filters in DataGrid.
        /// </summary>
        /// <param name="dg">DataGrid</param>
        public static void ClearColumnFilters(DataGrid dg)
        {
            foreach (var col in dg.Columns)
            {
                if (col.Header is ColumnFilter cf1)
                {
                    cf1.Value1 = cf1.ValueDef;
                    cf1.Value2 = cf1.ValueDef;
                }
                else if (col.Header is DependencyObject)
                {
                    foreach (var cf2 in Extensions.FindVisualChildren<ColumnFilter>((DependencyObject)col.Header).Where(x => x.FilterSQL != null))
                    {
                        cf2.Value1 = cf2.ValueDef;
                        cf2.Value2 = cf2.ValueDef;
                    }
                }
            }
        }

        /// GetFilters
        public static void GetFilters(DependencyObject panel, out string filter, out List<(string name, object val)> parameters)
        {
            filter = string.Empty;
            parameters = new List<(string name, object val)>();

            foreach (var cf in Extensions.FindVisualChildren<ColumnFilter>(panel).Where(x => x.FilterSQL != null))
            {
                filter += " and " + cf.FilterSQL;
                if (cf.Value1 != null)
                    parameters.Add((cf.ParamSQL.Substring(0, cf.ParamSQL.Length > 120 ? 120 : cf.ParamSQL.Length) + "1", (cf.Value1 is List<object> ? null : cf.Value1) ?? DBNull.Value));
                if (cf.Value2 != null)
                    parameters.Add((cf.ParamSQL.Substring(0, cf.ParamSQL.Length > 120 ? 120 : cf.ParamSQL.Length) + "2", (cf.Value2 is List<object> ? null : cf.Value2) ?? DBNull.Value));
            }

            if (filter.StartsWith(" and "))
                filter = filter[5..];
            if (string.IsNullOrWhiteSpace(filter))
                filter = "1=1";
        }

        /// ClearFilters
        public static void ClearFilters(DependencyObject panel)
        {
            foreach (var cf in Extensions.FindVisualChildren<ColumnFilter>(panel).Where(x => x.FilterSQL != null))
            {
                cf.Value1 = cf.ValueDef;
                cf.Value2 = cf.ValueDef;
            }
        }
        #endregion
    }
}
