using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StswExpress
{
    public static class Fn
    {
        #region App & Database
        /// App name
        public static string? AppName() => Assembly.GetEntryAssembly()?.GetName().Name;

        /// App version
        public static string? AppVersion() => Assembly.GetEntryAssembly()?.GetName().Version?.ToString()?.TrimEnd(".0".ToCharArray());

        /// App name + version
        public static string AppNameAndVersion => $"{AppName()} {(AppVersion() != "1" ? AppVersion() : string.Empty)}";

        /// App copyright
        public static string? AppCopyright => $"{FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).LegalCopyright}";

        /// App database
        public static DB AppDatabase { get; set; } = new DB();
        #endregion

        /// <summary>
        /// Adds char before upper letters in string.
        /// </summary>
        /// <param name="text">Text to edit</param>
        /// <param name="symbol">Char to add</param>
        /// <returns>Text after adding a char</returns>
        public static string AddCharBeforeUpperLetters(string text, char symbol)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);

            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && text[i - 1] != symbol)
                    newText.Append(symbol);
                newText.Append(text[i]);
            }

            return newText.ToString();
        }
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

        /// <summary>
        /// Gets color of system color chosen by user
        /// </summary>
        public static Color GetWindowsThemeColor() => SystemParameters.WindowGlassColor;
        
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
                else
                {
                    foreach (var cf2 in Extensions.FindVisualChildren<ColumnFilter>((DependencyObject)col.Header).ToList().Where(x => x.FilterSQL != null))
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
                else
                {
                    foreach (var cf2 in Extensions.FindVisualChildren<ColumnFilter>((DependencyObject)col.Header).ToList())
                    {
                        if (cf2.FilterSQL != null)
                        {
                            cf2.Value1 = cf2.ValueDef;
                            cf2.Value2 = cf2.ValueDef;
                        }
                    }
                }
            }
        }
        #endregion
        
        #region themes
        /// SetTheme
        public static void SetTheme(int theme)
        {
            Themes.Default.Theme = theme;

            switch (theme)
            {
                /// LIGHT THEME
                case 0:
                default:
                    Themes.Default.cMain = new conv_Color().Convert(new ColorConverter().ConvertToString(Fn.GetWindowsThemeColor()), typeof(Color), "0.6", CultureInfo.InvariantCulture).ToString();
                    /// StswWindow
                    Themes.Default.cTitleBarBackground = "#EEE";
                    Themes.Default.cTitleBarBorder = "#CCC";
                    Themes.Default.cTitleBarForeground1 = "#000";
                    Themes.Default.cTitleBarForeground2 = "#555";
                    Themes.Default.cWindowBackground = "#FFF";
                    Themes.Default.cWindowForeground = "#000";
                    Themes.Default.cDisabledForeground = "#888";
                    /// ExtCheckBox
                    Themes.Default.cOptionMarkStaticBackground = "#FFF";
                    Themes.Default.cOptionMarkStaticBorder = "#707070";
                    Themes.Default.cOptionMarkStaticGlyph = "#212121";
                    Themes.Default.cOptionMarkMouseOverBackground = "#F3F9FF";
                    Themes.Default.cOptionMarkMouseOverBorder = "#5593FF";
                    Themes.Default.cOptionMarkMouseOverGlyph = "#212121";
                    Themes.Default.cOptionMarkPressedBackground = "#D9ECFF";
                    Themes.Default.cOptionMarkPressedBorder = "#3C77DD";
                    Themes.Default.cOptionMarkPressedGlyph = "#212121";
                    Themes.Default.cOptionMarkDisabledBackground = "#E6E6E6";
                    Themes.Default.cOptionMarkDisabledBorder = "#BCBCBC";
                    Themes.Default.cOptionMarkDisabledGlyph = "#707070";
                    /// ExtComboBox
                    Themes.Default.cComboBoxStaticBackground1 = "#F0F0F0";
                    Themes.Default.cComboBoxStaticBackground2 = "#E5E5E5";
                    Themes.Default.cComboBoxStaticBorder = "#ACACAC";
                    Themes.Default.cComboBoxStaticGlyph = "#606060";
                    Themes.Default.cComboBoxStaticEditableBackground = "#FFF";
                    Themes.Default.cComboBoxStaticEditableBorder = "#ABADB3";
                    Themes.Default.cComboBoxMouseOverBackground1 = "#ECF4FC";
                    Themes.Default.cComboBoxMouseOverBackground2 = "#DCECFC";
                    Themes.Default.cComboBoxMouseOverBorder = "#7EB4EA";
                    Themes.Default.cComboBoxMouseOverGlyph = "#000";
                    Themes.Default.cComboBoxMouseOverEditableBackground = "#FFF";
                    Themes.Default.cComboBoxMouseOverEditableBorder = "#7EB4EA";
                    Themes.Default.cComboBoxMouseOverEditableButtonBackground1 = "#EBF4FC";
                    Themes.Default.cComboBoxMouseOverEditableButtonBackground2 = "#DCECFC";
                    Themes.Default.cComboBoxMouseOverEditableButtonBorder = "#7EB4EA";
                    Themes.Default.cComboBoxPressedBackground1 = "#DAECFC";
                    Themes.Default.cComboBoxPressedBackground2 = "#C4E0FC";
                    Themes.Default.cComboBoxPressedBorder = "#569DE5";
                    Themes.Default.cComboBoxPressedGlyph = "#000";
                    Themes.Default.cComboBoxPressedEditableBackground = "#FFF";
                    Themes.Default.cComboBoxPressedEditableBorder = "#569DE5";
                    Themes.Default.cComboBoxPressedEditableButtonBackground1 = "#DAEBFC";
                    Themes.Default.cComboBoxPressedEditableButtonBackground2 = "#C4E0FC";
                    Themes.Default.cComboBoxPressedEditableButtonBorder = "#569DE5";
                    Themes.Default.cComboBoxDisabledBackground = "#F0F0F0";
                    Themes.Default.cComboBoxDisabledBorder = "#D9D9D9";
                    Themes.Default.cComboBoxDisabledGlyph = "#BFBFBF";
                    Themes.Default.cComboBoxDisabledEditableBackground = "#FFF";
                    Themes.Default.cComboBoxDisabledEditableBorder = "#BFBFBF";
                    Themes.Default.cComboBoxItemItemsviewHoverBackground = "#1F26A0DA";
                    Themes.Default.cComboBoxItemItemsviewHoverBorder = "#A826A0DA";
                    Themes.Default.cComboBoxItemItemsviewSelectedBackground = "#3D26A0DA";
                    Themes.Default.cComboBoxItemItemsviewSelectedBorder = "#26A0DA";
                    Themes.Default.cComboBoxItemItemsviewSelectedHoverBackground = "#2E0080FF";
                    Themes.Default.cComboBoxItemItemsviewSelectedHoverBorder = "#99006CD9";
                    Themes.Default.cComboBoxItemItemsviewSelectedNoFocusBackground = "#3DDADADA";
                    Themes.Default.cComboBoxItemItemsviewSelectedNoFocusBorder = "#DADADA";
                    Themes.Default.cComboBoxItemItemsviewFocusBorder = "#26A0DA";
                    Themes.Default.cComboBoxItemItemsviewHoverFocusBackground = "#5426A0DA";
                    Themes.Default.cComboBoxItemItemsviewHoverFocusBorder = "#26A0DA";
                    /// ExtDataGrid
                    Themes.Default.cDataGridStaticGridLines = "#2222";
                    Themes.Default.cDataGridStaticBackground = "#FFF";
                    Themes.Default.cDataGridErrorForeground = "#F00";
                    /// ExtDatePicker
                    Themes.Default.cDatePickerTextBoxMouseOverWatermark = "#99C1E2";
                    Themes.Default.cDatePickerTextBoxStaticBorderWatermark = "#FFF";
                    Themes.Default.cDatePickerTextBoxStaticBorderHost = "#45D6FA";
                    /// ExtMenuItem
                    Themes.Default.cMenuStaticBackground = "#F0F0F0";
                    Themes.Default.cMenuStaticBorder = "#999";
                    Themes.Default.cMenuStaticForeground = "#212121";
                    Themes.Default.cMenuStaticSeparator = "#D7D7D7";
                    Themes.Default.cMenuDisabledForeground = "#707070";
                    Themes.Default.cMenuItemSelectedBackground = "#3D26A0DA";
                    Themes.Default.cMenuItemSelectedBorder = "#26A0DA";
                    Themes.Default.cMenuItemHighlightBackground = "#3D26A0DA";
                    Themes.Default.cMenuItemHighlightBorder = "#26A0DA";
                    Themes.Default.cMenuItemHighlightDisabledBackground = "#0A000000";
                    Themes.Default.cMenuItemHighlightDisabledBorder = "#21000000";
                    /// ExtTabControl
                    Themes.Default.cTabItemSelectedBackground = "#FFF";
                    Themes.Default.cTabItemSelectedBorder = "#ACACAC";
                    Themes.Default.cTabItemStaticBackground1 = "#F0F0F0";
                    Themes.Default.cTabItemStaticBackground2 = "#E5E5E5";
                    Themes.Default.cTabItemStaticBorder = "#ACACAC";
                    Themes.Default.cTabItemMouseOverBackground1 = "#ECF4FC";
                    Themes.Default.cTabItemMouseOverBackground2 = "#DCECFC";
                    Themes.Default.cTabItemMouseOverBorder = "#7EB4EA";
                    Themes.Default.cTabItemDisabledBackground = "#F0F0F0";
                    Themes.Default.cTabItemDisabledBorder = "#D9D9D9";
                    /// IconButton
                    Themes.Default.cButtonStaticBackground = "#DDD";
                    Themes.Default.cButtonStaticBorder = "#707070";
                    Themes.Default.cButtonMouseOverBackground = "#BEE6FD";
                    Themes.Default.cButtonMouseOverBorder = "#3C7FB1";
                    Themes.Default.cButtonPressedBackground = "#C4E5F6";
                    Themes.Default.cButtonPressedBorder = "#2C628B";
                    Themes.Default.cButtonDisabledBackground = "#F4F4F4";
                    Themes.Default.cButtonDisabledBorder = "#ADB2B5";
                    Themes.Default.cButtonDisabledForeground = "#838383";
                    /// LoadingCircle
                    break;
                /// DARK THEME
                case 1:
                    Themes.Default.cMain = new conv_Color().Convert(new ColorConverter().ConvertToString(Fn.GetWindowsThemeColor()), typeof(Color), "-0.6", CultureInfo.InvariantCulture).ToString();
                    /// StswWindow
                    Themes.Default.cTitleBarBackground = "#0A0A0A";
                    Themes.Default.cTitleBarBorder = "#333";
                    Themes.Default.cTitleBarForeground1 = "#FFF";
                    Themes.Default.cTitleBarForeground2 = "#AAA";
                    Themes.Default.cWindowBackground = "#111";
                    Themes.Default.cWindowForeground = "#FFF";
                    Themes.Default.cDisabledForeground = "#888";
                    /// ExtCheckBox
                    Themes.Default.cOptionMarkStaticBackground = "#111";
                    Themes.Default.cOptionMarkStaticBorder = "#8F8F8F";
                    Themes.Default.cOptionMarkStaticGlyph = "#DEDEDE";
                    Themes.Default.cOptionMarkMouseOverBackground = "#13191F";
                    Themes.Default.cOptionMarkMouseOverBorder = "#2553AA";
                    Themes.Default.cOptionMarkMouseOverGlyph = "#DEDEDE";
                    Themes.Default.cOptionMarkPressedBackground = "#393C3F";
                    Themes.Default.cOptionMarkPressedBorder = "#1C479D";
                    Themes.Default.cOptionMarkPressedGlyph = "#DEDEDE";
                    Themes.Default.cOptionMarkDisabledBackground = "#292929";
                    Themes.Default.cOptionMarkDisabledBorder = "#545454";
                    Themes.Default.cOptionMarkDisabledGlyph = "#8F8F8F";
                    /// ExtComboBox
                    Themes.Default.cComboBoxStaticBackground1 = "#1F1F1F";
                    Themes.Default.cComboBoxStaticBackground2 = "#2A2A2A";
                    Themes.Default.cComboBoxStaticBorder = "#646464";
                    Themes.Default.cComboBoxStaticGlyph = "#9F9F9F";
                    Themes.Default.cComboBoxStaticEditableBackground = "#000";
                    Themes.Default.cComboBoxStaticEditableBorder = "#5B5D63";
                    Themes.Default.cComboBoxMouseOverBackground1 = "#9CA4AC";
                    Themes.Default.cComboBoxMouseOverBackground2 = "#8C9CAC";
                    Themes.Default.cComboBoxMouseOverBorder = "#4E749A";
                    Themes.Default.cComboBoxMouseOverGlyph = "#FFF";
                    Themes.Default.cComboBoxMouseOverEditableBackground = "#000";
                    Themes.Default.cComboBoxMouseOverEditableBorder = "#4E749A";
                    Themes.Default.cComboBoxMouseOverEditableButtonBackground1 = "#9BA4AC";
                    Themes.Default.cComboBoxMouseOverEditableButtonBackground2 = "#8C9CAC";
                    Themes.Default.cComboBoxMouseOverEditableButtonBorder = "#4E749A";
                    Themes.Default.cComboBoxPressedBackground1 = "#8A9CAC";
                    Themes.Default.cComboBoxPressedBackground2 = "#7490AC";
                    Themes.Default.cComboBoxPressedBorder = "#265D95";
                    Themes.Default.cComboBoxPressedGlyph = "#FFF";
                    Themes.Default.cComboBoxPressedEditableBackground = "#000";
                    Themes.Default.cComboBoxPressedEditableBorder = "#265D95";
                    Themes.Default.cComboBoxPressedEditableButtonBackground1 = "#8A9BAC";
                    Themes.Default.cComboBoxPressedEditableButtonBackground2 = "#7490AC";
                    Themes.Default.cComboBoxPressedEditableButtonBorder = "#265D95";
                    Themes.Default.cComboBoxDisabledBackground = "#1F1F1F";
                    Themes.Default.cComboBoxDisabledBorder = "#363636";
                    Themes.Default.cComboBoxDisabledGlyph = "#505050";
                    Themes.Default.cComboBoxDisabledEditableBackground = "#000";
                    Themes.Default.cComboBoxDisabledEditableBorder = "#505050";
                    Themes.Default.cComboBoxItemItemsviewHoverBackground = "#1F06709A";
                    Themes.Default.cComboBoxItemItemsviewHoverBorder = "#A806709A";
                    Themes.Default.cComboBoxItemItemsviewSelectedBackground = "#3D06709A";
                    Themes.Default.cComboBoxItemItemsviewSelectedBorder = "#06709A";
                    Themes.Default.cComboBoxItemItemsviewSelectedHoverBackground = "#2E004077";
                    Themes.Default.cComboBoxItemItemsviewSelectedHoverBorder = "#99003C69";
                    Themes.Default.cComboBoxItemItemsviewSelectedNoFocusBackground = "#3D353535";
                    Themes.Default.cComboBoxItemItemsviewSelectedNoFocusBorder = "#353535";
                    Themes.Default.cComboBoxItemItemsviewFocusBorder = "#06709A";
                    Themes.Default.cComboBoxItemItemsviewHoverFocusBackground = "#5406709A";
                    Themes.Default.cComboBoxItemItemsviewHoverFocusBorder = "#06709A";
                    /// ExtDataGrid
                    Themes.Default.cDataGridStaticGridLines = "#2DDD";
                    Themes.Default.cDataGridStaticBackground = "#111";
                    Themes.Default.cDataGridErrorForeground = "#A11";
                    /// ExtDatePicker
                    Themes.Default.cDatePickerTextBoxMouseOverWatermark = "#698192";
                    Themes.Default.cDatePickerTextBoxStaticBorderWatermark = "#000";
                    Themes.Default.cDatePickerTextBoxStaticBorderHost = "#1596AA";
                    /// ExtMenuItem
                    Themes.Default.cMenuStaticBackground = "#1F1F1F";
                    Themes.Default.cMenuStaticBorder = "#777";
                    Themes.Default.cMenuStaticForeground = "#DEDEDE";
                    Themes.Default.cMenuStaticSeparator = "#383838";
                    Themes.Default.cMenuDisabledForeground = "#8F8F8F";
                    Themes.Default.cMenuItemSelectedBackground = "#3D06709A";
                    Themes.Default.cMenuItemSelectedBorder = "#06709A";
                    Themes.Default.cMenuItemHighlightBackground = "#3D06709A";
                    Themes.Default.cMenuItemHighlightBorder = "#06709A";
                    Themes.Default.cMenuItemHighlightDisabledBackground = "#0AFFFFFF";
                    Themes.Default.cMenuItemHighlightDisabledBorder = "#21FFFFFF";
                    /// ExtTabControl
                    Themes.Default.cTabItemSelectedBackground = "#000";
                    Themes.Default.cTabItemSelectedBorder = "#646464";
                    Themes.Default.cTabItemStaticBackground1 = "#1F1F1F";
                    Themes.Default.cTabItemStaticBackground2 = "#2A2A2A";
                    Themes.Default.cTabItemStaticBorder = "#646464";
                    Themes.Default.cTabItemMouseOverBackground1 = "#9CA4AC";
                    Themes.Default.cTabItemMouseOverBackground2 = "#8C9CAC";
                    Themes.Default.cTabItemMouseOverBorder = "#4E749A";
                    Themes.Default.cTabItemDisabledBackground = "#1F1F1F";
                    Themes.Default.cTabItemDisabledBorder = "#363636";
                    /// IconButton
                    Themes.Default.cButtonStaticBackground = "#333";
                    Themes.Default.cButtonStaticBorder = "#9F9F9F";
                    Themes.Default.cButtonMouseOverBackground = "#6E96AD";
                    Themes.Default.cButtonMouseOverBorder = "#0C3F61";
                    Themes.Default.cButtonPressedBackground = "#7495A6";
                    Themes.Default.cButtonPressedBorder = "#1C425B";
                    Themes.Default.cButtonDisabledBackground = "#1B1B1B";
                    Themes.Default.cButtonDisabledBorder = "#5D6265";
                    Themes.Default.cButtonDisabledForeground = "#7C7C7C";
                    /// LoadingCircle
                    break;
            }

            Themes.Default.Save();
        }
        #endregion
    }
}
