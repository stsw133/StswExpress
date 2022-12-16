using DynamicAero2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace StswExpress;

public static class StswFn
{
    #region app & database & mailConfig
    /// App: name & version & name + version & copyright
    public static string? AppName() => Assembly.GetEntryAssembly()?.GetName().Name;
    public static string? AppVersion() => Assembly.GetEntryAssembly()?.GetName().Version?.ToString()?.TrimEnd(".0").TrimEnd(".0").TrimEnd(".0");
    public static string AppNameAndVersion => $"{AppName()} {(AppVersion() != "1" ? AppVersion() : string.Empty)}";
    public static string? AppCopyright => $"{FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).LegalCopyright}";

    /// App: database connection & mail config
    public static StswDB? AppDB { get; set; } = new();
    public static StswMC? AppMC { get; set; } = new();
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

    /// Starting functions that should be placed in constructor of App class
    public static void AppStart(Application app)
    {
        if (!app.Resources.MergedDictionaries.Any(x => x is Theme))
            app.Resources.MergedDictionaries.Add(new Theme());
        ((Theme)app.Resources.MergedDictionaries.First(x => x is Theme)).Color = (ThemeColor)Settings.Default.Theme;

        app.Exit += (sender, e) => Settings.Default.Save();
    }

    /// ImageFromClipboardDib
    public static ImageSource ImageFromClipboard()
    {
        MemoryStream ms = Clipboard.GetData("DeviceIndependentBitmap") as MemoryStream;
        if (ms != null)
        {
            byte[] dibBuffer = new byte[ms.Length];
            ms.Read(dibBuffer, 0, dibBuffer.Length);

            BITMAPINFOHEADER infoHeader =
                BinaryStructConverter.FromByteArray<BITMAPINFOHEADER>(dibBuffer);

            int fileHeaderSize = Marshal.SizeOf(typeof(BITMAPFILEHEADER));
            int infoHeaderSize = infoHeader.biSize;
            int fileSize = fileHeaderSize + infoHeader.biSize + infoHeader.biSizeImage;

            BITMAPFILEHEADER fileHeader = new BITMAPFILEHEADER();
            fileHeader.bfType = BITMAPFILEHEADER.BM;
            fileHeader.bfSize = fileSize;
            fileHeader.bfReserved1 = 0;
            fileHeader.bfReserved2 = 0;
            fileHeader.bfOffBits = fileHeaderSize + infoHeaderSize + infoHeader.biClrUsed * 4;

            byte[] fileHeaderBytes =
                BinaryStructConverter.ToByteArray<BITMAPFILEHEADER>(fileHeader);

            MemoryStream msBitmap = new MemoryStream();
            msBitmap.Write(fileHeaderBytes, 0, fileHeaderSize);
            msBitmap.Write(dibBuffer, 0, dibBuffer.Length);
            msBitmap.Seek(0, SeekOrigin.Begin);

            return BitmapFrame.Create(msBitmap);
        }
        return null;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    private struct BITMAPFILEHEADER
    {
        public static readonly short BM = 0x4d42; // BM

        public short bfType;
        public int bfSize;
        public short bfReserved1;
        public short bfReserved2;
        public int bfOffBits;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFOHEADER
    {
        public int biSize;
        public int biWidth;
        public int biHeight;
        public short biPlanes;
        public short biBitCount;
        public int biCompression;
        public int biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public int biClrUsed;
        public int biClrImportant;
    }

    public static class BinaryStructConverter
    {
        public static T FromByteArray<T>(byte[] bytes) where T : struct
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int size = Marshal.SizeOf(typeof(T));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(bytes, 0, ptr, size);
                object obj = Marshal.PtrToStructure(ptr, typeof(T));
                return (T)obj;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        public static byte[] ToByteArray<T>(T obj) where T : struct
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int size = Marshal.SizeOf(typeof(T));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(obj, ptr, true);
                byte[] bytes = new byte[size];
                Marshal.Copy(ptr, bytes, 0, size);
                return bytes;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }
    }

    /// Opens context menu of a framework element
    public static void OpenContextMenu(object sender)
    {
        if (sender is FrameworkElement f)
        {
            f.ContextMenu.PlacementTarget = f;
            f.ContextMenu.IsOpen = true;
        }
    }

    /// Opens file from path
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
        var dict = new ExtDictionary<string, ColumnFilter>();

        /// DependencyObject's children are ColumnFilter
        foreach (var cf in StswExtensions.FindVisualChildren<ColumnFilter>(panel).Where(x => x.SqlString != null))
            dict.Add(new KeyValuePair<string, ColumnFilter>(Guid.NewGuid().ToString(), cf));

        dict.GetColumnFilters(out filter, out parameters);
    }

    /// ClearFilters
    public static void ClearFilters(DependencyObject panel)
    {
        var dict = new ExtDictionary<string, ColumnFilter>();

        /// DependencyObject's children are ColumnFilter
        foreach (var cf in StswExtensions.FindVisualChildren<ColumnFilter>(panel).Where(x => x.SqlString != null))
            dict.Add(new KeyValuePair<string, ColumnFilter>(Guid.NewGuid().ToString(), cf));

        dict.ClearColumnFilters();
    }
    #endregion
}
