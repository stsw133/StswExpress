using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StswExpress;
/// <summary>
/// Utility class providing various helper functions for general use.
/// </summary>
public static class StswFn
{
    #region Assembly functions
    /// <summary>
    /// Gets the name of the currently executing application.
    /// </summary>
    /// <returns>The name of the currently executing application, or null if it cannot be determined.</returns>
    public static string? AppName() => Assembly.GetEntryAssembly()?.GetName().Name;

    /// <summary>
    /// Gets the version number of the currently executing application.
    /// </summary>
    /// <returns>The version number of the currently executing application as a string, or null if it cannot be determined.</returns>
    public static string? AppVersion()
    {
        if (Assembly.GetEntryAssembly()?.GetName().Version is Version version)
        {
            int?[] versionParts = [version.Major, version.Minor, version.Build, version.Revision];
            for (int i = versionParts.Length - 1; i >= 0; i--)
            {
                if (versionParts[i] == 0)
                    versionParts[i] = null;
                else
                    break;
            }

            return string.Join(".", versionParts.Where(part => part != null));
        }

        return null;
    }

    /// <summary>
    /// Gets the name and version number of the currently executing application.
    /// </summary>
    /// <returns>A string containing the name and version number of the currently executing application.</returns>
    public static string AppNameAndVersion => $"{AppName()} {(AppVersion() != "1" ? AppVersion() : string.Empty)}";

    /// <summary>
    /// Gets the copyright information for the currently executing application.
    /// </summary>
    /// <returns>The copyright information, or null if it cannot be determined.</returns>
    public static string? AppCopyright => Assembly.GetEntryAssembly()?.Location is string location ? FileVersionInfo.GetVersionInfo(location).LegalCopyright : null;

    /// <summary>
    /// Determines whether the entry assembly was built in debug mode.
    /// </summary>
    /// <param name="assembly">The assembly to check.</param>
    /// <returns><see langword="true"/> if the assembly was built in debug mode; otherwise, <see langword="false"/>.</returns>
    public static bool IsInDebug() => Assembly.GetEntryAssembly()?.GetCustomAttributes<DebuggableAttribute>().FirstOrDefault()?.IsJITTrackingEnabled == true;
    #endregion

    #region Color functions
    /// <summary>
    /// Creates a <see cref="Color"/> from the specified alpha, hue, saturation, and lightness (HSL) values.
    /// </summary>
    /// <param name="alpha">The alpha component (0-255).</param>
    /// <param name="hue">The hue component (0-360).</param>
    /// <param name="saturation">The saturation component (0-1).</param>
    /// <param name="lightness">The lightness component (0-1).</param>
    /// <returns>A <see cref="Color"/> object representing the specified HSL values.</returns>
    public static Color ColorFromHsl(byte alpha, double hue, double saturation, double lightness)
    {
        var h = hue / 360.0;
        var c = (1 - Math.Abs(2 * lightness - 1)) * saturation;
        var x = c * (1 - Math.Abs((h * 6) % 2 - 1));
        var m = lightness - c / 2;

        double r = 0, g = 0, b = 0;
        if (h < 1.0 / 6)
        {
            r = c; g = x;
        }
        else if (h < 2.0 / 6)
        {
            r = x; g = c;
        }
        else if (h < 3.0 / 6)
        {
            g = c; b = x;
        }
        else if (h < 4.0 / 6)
        {
            g = x; b = c;
        }
        else if (h < 5.0 / 6)
        {
            r = x; b = c;
        }
        else
        {
            r = c; b = x;
        }

        r = Math.Round((r + m) * 255);
        g = Math.Round((g + m) * 255);
        b = Math.Round((b + m) * 255);

        return Color.FromArgb(alpha, (byte)r, (byte)g, (byte)b);
    }

    /// <summary>
    /// Creates a <see cref="Color"/> from the specified hue, saturation, and lightness (HSL) values with full opacity.
    /// </summary>
    /// <param name="hue">The hue component (0-360).</param>
    /// <param name="saturation">The saturation component (0-1).</param>
    /// <param name="lightness">The lightness component (0-1).</param>
    /// <returns>A <see cref="Color"/> object representing the specified HSL values with full opacity.</returns>
    public static Color ColorFromHsl(double hue, double saturation, double lightness) => ColorFromHsl(255, hue, saturation, lightness);

    /// <summary>
    /// Converts a <see cref="Color"/> to its hue, saturation, and lightness (HSL) components.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <param name="hue">The hue component (0-360).</param>
    /// <param name="saturation">The saturation component (0-1).</param>
    /// <param name="lightness">The lightness component (0-1).</param>
    public static void ColorToHsl(Color color, out double hue, out double saturation, out double lightness)
    {
        var drawingColor = color.ToDrawingColor();

        hue = drawingColor.GetHue();
        saturation = drawingColor.GetSaturation();
        lightness = drawingColor.GetBrightness();
    }

    /// <summary>
    /// Creates a <see cref="Color"/> from the specified alpha, hue, saturation, and value (HSV) values.
    /// </summary>
    /// <param name="alpha">The alpha component (0-255).</param>
    /// <param name="hue">The hue component (0-360).</param>
    /// <param name="saturation">The saturation component (0-1).</param>
    /// <param name="value">The value component (0-1).</param>
    /// <returns>A <see cref="Color"/> object representing the specified HSV values.</returns>
    public static Color ColorFromHsv(byte alpha, double hue, double saturation, double value)
    {
        var h = (int)Math.Floor(hue / 60) % 6;
        var f = hue / 60 - Math.Floor(hue / 60);

        value *= 255;
        var v = (byte)value;
        var p = (byte)(value * (1 - saturation));
        var q = (byte)(value * (1 - f * saturation));
        var t = (byte)(value * (1 - (1 - f) * saturation));

        return h switch
        {
            0 => Color.FromArgb(alpha, v, t, p),
            1 => Color.FromArgb(alpha, q, v, p),
            2 => Color.FromArgb(alpha, p, v, t),
            3 => Color.FromArgb(alpha, p, q, v),
            4 => Color.FromArgb(alpha, t, p, v),
            _ => Color.FromArgb(alpha, v, p, q)
        };
    }

    /// <summary>
    /// Creates a <see cref="Color"/> from the specified hue, saturation, and value (HSV) values with full opacity.
    /// </summary>
    /// <param name="hue">The hue component (0-360).</param>
    /// <param name="saturation">The saturation component (0-1).</param>
    /// <param name="value">The value component (0-1).</param>
    /// <returns>A <see cref="Color"/> object representing the specified HSV values with full opacity.</returns>
    public static Color ColorFromHsv(double hue, double saturation, double value) => ColorFromHsv(255, hue, saturation, value);

    /// <summary>
    /// Converts a <see cref="Color"/> to its hue, saturation, and value (HSV) components.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <param name="hue">The hue component (0-360).</param>
    /// <param name="saturation">The saturation component (0-1).</param>
    /// <param name="value">The value component (0-1).</param>
    public static void ColorToHsv(Color color, out double hue, out double saturation, out double value)
    {
        int max = Math.Max(color.R, Math.Max(color.G, color.B));
        int min = Math.Min(color.R, Math.Min(color.G, color.B));

        hue = color.ToDrawingColor().GetHue();
        saturation = (max == 0) ? 0 : 1d - (1d * min / max);
        value = max / 255d;
    }

    /// <summary>
    /// Generates a new <see cref="Color"/> based on the specified text and seed for brightness adjustment.
    /// </summary>
    /// <param name="text">The text to convert into a color.</param>
    /// <param name="seed">The seed value used to adjust the brightness of the generated color.</param>
    /// <returns>A <see cref="Color"/> object generated from the text and adjusted by the seed value.</returns>
    public static Color GenerateColor(string text, int seed)
    {
        static int AdjustBrightness(int colorComponent, int seed)
        {
            if (colorComponent > seed)
                return colorComponent - (colorComponent - seed) / 2;
            else if (colorComponent < seed)
                return colorComponent + (seed - colorComponent) / 2;
            return colorComponent;
        }

        var color = Colors.Transparent;

        if (!string.IsNullOrEmpty(text))
        {
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
            int r = hashBytes[0];
            int g = hashBytes[1];
            int b = hashBytes[2];

            if (seed >= 0)
            {
                r = AdjustBrightness(r, seed);
                g = AdjustBrightness(g, seed);
                b = AdjustBrightness(b, seed);
            }

            color = Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
        }

        return color;
    }
    #endregion

    #region Convert functions
    /// <summary>
    /// Converts a byte array to a <see cref="BitmapImage"/>.
    /// </summary>
    /// <param name="value">The byte array to convert.</param>
    /// <returns>The converted <see cref="BitmapImage"/>, or null if the byte array is empty.</returns>
    public static BitmapImage? BytesToBitmapImage(byte[]? value)
    {
        if (value == null || value.Length == 0)
            return null;

        var result = new BitmapImage();
        using (var mem = new MemoryStream(value))
        {
            mem.Position = 0;
            result.BeginInit();
            result.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.UriSource = null;
            result.StreamSource = mem;
            result.EndInit();
        }
        result.Freeze();

        return result;
    }

    /// <summary>
    /// Merges properties of multiple objects into a single dictionary object. 
    /// In case of property name conflicts, properties from later objects will overwrite those from earlier ones.
    /// </summary>
    /// <param name="parameters">An array of objects to be merged.</param>
    /// <returns>A dictionary object containing all properties from the provided objects.</returns>
    public static IDictionary<string, object?> MergeObjects(params object[] parameters)
    {
        var expando = new ExpandoObject() as IDictionary<string, object?>;

        foreach (var parameter in parameters)
        {
            if (parameter == null)
                continue;

            foreach (var property in parameter.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propertyName = property.Name;
                var propertyValue = property.GetValue(parameter);
                expando[propertyName] = propertyValue;
            }
        }

        return expando;
    }
    #endregion

    #region DateTime functions
    /// <summary>
    /// Generates a list of unique year and month tuples within a specified date range.
    /// </summary>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <returns>A list of tuples, where each tuple contains the year and month within the specified date range.</returns>
    /// <remarks>
    /// This method iterates from the start date to the end date, month by month,
    /// and collects each unique year and month combination into a list.
    /// The day component of the dates is ignored.
    /// </remarks>
    public static List<(int Year, int Month)> GetUniqueMonthsFromRange(DateTime startDate, DateTime endDate)
    {
        var months = new List<(int Year, int Month)>();
        var current = new DateTime(startDate.Year, startDate.Month, 1);

        while (current <= endDate)
        {
            months.Add((current.Year, current.Month));
            current = current.AddMonths(1);
        }

        return months;
    }
    #endregion

    #region File functions
    internal struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    /// <summary>
    /// Extracts the associated icon of the specified file or directory path as an <see cref="ImageSource"/>.
    /// </summary>
    /// <param name="path">The file or directory path to extract the icon from.</param>
    /// <returns>The associated icon as an <see cref="ImageSource"/> if found; otherwise, <see langword="null"/>.</returns>
    public static ImageSource? ExtractAssociatedIcon(string? path)
    {
        const uint SHGFI_ICON = 0x100;
        const uint SHGFI_LARGEICON = 0x0;

        [DllImport("shell32.dll")]
        static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        if (Path.Exists(path))
        {
            if ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var shinfo = new SHFILEINFO();
                if (SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_LARGEICON) != IntPtr.Zero)
                    return System.Drawing.Icon.FromHandle(shinfo.hIcon).ToImageSource();
            }
            else
            {
                if (System.Drawing.Icon.ExtractAssociatedIcon(path) is System.Drawing.Icon icon)
                    return icon.ToImageSource();
            }
        }
        return null;
    }

    /// <summary>
    /// Checks if a file is currently in use.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns><see langword="true"/> if the file is in use; otherwise, <see langword="false"/>.</returns>
    public static bool IsFileInUse(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Moves a file to the recycle bin.
    /// </summary>
    /// <param name="path">The path to the file to be moved to the recycle bin.</param>
    public static void MoveToRecycleBin(string path) => FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

    /// <summary>
    /// Opens a file in its associated application.
    /// </summary>
    /// <param name="path">The path to the file to be opened.</param>
    public static void OpenFile(string path)
    {
        var process = new Process();
        process.StartInfo.FileName = path;
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.Verb = "open";
        process.Start();
    }

    /// <summary>
    /// Prints a file in its associated application.
    /// </summary>
    /// <param name="path">The path to the file to be printed.</param>
    public static void PrintFile(string path)
    {
        var process = new Process();
        process.StartInfo.FileName = path;
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.Verb = "print";
        process.Start();
    }
    #endregion

    #region Finding functions
    /// <summary>
    /// Finds the first logical child of a specific type for the given control.
    /// </summary>
    /// <typeparam name="T">The type of the child to find.</typeparam>
    /// <param name="obj">The control for which to find the logical child.</param>
    /// <returns>The first logical child of the specified type, or null if no such child exists.</returns>
    public static T? FindLogicalChild<T>(DependencyObject obj) where T : class
    {
        if (obj == null)
            return null;

        foreach (var child in LogicalTreeHelper.GetChildren(obj))
        {
            if (child is DependencyObject depChild)
            {
                if (child is T result)
                    return result;

                var descendent = FindLogicalChild<T>(depChild);
                if (descendent != null)
                    return descendent;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the first visual ancestor of a specific type for the given control.
    /// </summary>
    /// <typeparam name="T">The type of the ancestor to find.</typeparam>
    /// <param name="obj">The control for which to find the visual ancestor.</param>
    /// <returns>The first visual ancestor of the specified type, or null if no such ancestor exists.</returns>
    public static T? FindVisualAncestor<T>(DependencyObject obj) where T : class
    {
        var dependObj = obj;
        while (dependObj != null)
        {
            dependObj = GetParent(dependObj);
            if (dependObj is T ancestor)
                return ancestor;
        }
        return null;
    }

    /// <summary>
    /// Finds the first visual child of a specific type for the given control.
    /// </summary>
    /// <typeparam name="T">The type of the child to find.</typeparam>
    /// <param name="obj">The control for which to find the visual child.</param>
    /// <returns>The first visual child of the specified type, or null if no such child exists.</returns>
    public static T? FindVisualChild<T>(DependencyObject obj) where T : class
    {
        if (obj == null)
            return null;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        {
            var child = VisualTreeHelper.GetChild(obj, i);
            if (child is T result)
                return result;

            var descendent = FindVisualChild<T>(child);
            if (descendent != null)
                return descendent;
        }

        return null;
    }

    /// <summary>
    /// Finds all visual children of a specific type for the given control.
    /// </summary>
    /// <typeparam name="T">The type of the children to find.</typeparam>
    /// <param name="obj">The control for which to find the visual children.</param>
    /// <returns>An enumerable collection of visual children of the specified type.</returns>
    public static IEnumerable<T> FindVisualChildren<T>(DependencyObject obj) where T : class
    {
        if (obj == null)
            yield break;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        {
            var child = VisualTreeHelper.GetChild(obj, i);
            if (child is T t)
                yield return t;

            foreach (T childOfChild in FindVisualChildren<T>(child))
                yield return childOfChild;
        }
    }

    /// <summary>
    /// Gets the parent of the given control.
    /// </summary>
    /// <param name="obj">The control for which to find the parent.</param>
    /// <returns>The parent of the control, or null if no parent exists.</returns>
    public static DependencyObject? GetParent(DependencyObject obj)
    {
        if (obj == null)
            return null;

        if (obj is ContentElement contentElement)
        {
            var parent = ContentOperations.GetParent(contentElement);
            if (parent != null)
                return parent;

            if (obj is FrameworkContentElement frameworkContentElement)
                return frameworkContentElement.Parent;
        }
        else return VisualTreeHelper.GetParent(obj);

        return null;
    }

    /// <summary>
    /// Gets the parent popup of the given control.
    /// </summary>
    /// <param name="obj">The control for which to find the parent popup.</param>
    /// <returns>The parent <see cref="Popup"/> control, or null if no parent popup exists.</returns>
    public static Popup? GetParentPopup(DependencyObject obj)
    {
        var popupRootFinder = VisualTreeHelper.GetParent(obj);
        while (popupRootFinder != null)
        {
            var logicalRoot = LogicalTreeHelper.GetParent(popupRootFinder);
            if (logicalRoot is Popup popup)
                return popup;

            popupRootFinder = VisualTreeHelper.GetParent(popupRootFinder);
        }
        return null;
    }
    #endregion

    #region Numeric functions
    /// <summary>
    /// Evaluates a mathematical expression given as a string.
    /// </summary>
    /// <param name="expression">The mathematical expression to evaluate.</param>
    /// <returns>The result of the evaluated expression as a double.</returns>
    public static double Compute(string expression) => StswCalculator.EvaluatePostfix(StswCalculator.ConvertToPostfix(expression));

    /// <summary>
    /// Tries to evaluate a mathematical expression given as a string.
    /// </summary>
    /// <param name="expression">The mathematical expression to evaluate.</param>
    /// <param name="result">The result of the evaluated expression as a double.</param>
    /// <returns><see langword="true"/> on success, or <see langword="false"/> on failure.</returns>
    public static bool TryCompute(string expression, out double result)
    {
        try
        {
            result = StswCalculator.EvaluatePostfix(StswCalculator.ConvertToPostfix(expression));
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }
    #endregion

    #region Text functions
    /// <summary>
    /// Converts diacritics in a string to their ASCII substitutes.
    /// </summary>
    /// <param name="text">The input string containing diacritics.</param>
    /// <returns>The normalized string with diacritics replaced by ASCII substitutes.</returns>
    public static string NormalizeDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(c);

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Removes consecutive occurrences of a specified string from the original text.
    /// </summary>
    /// <param name="originalText">The original text to process.</param>
    /// <param name="textToRemove">The string to remove when consecutive occurrences are found.</param>
    /// <returns>The processed string with consecutive occurrences removed.</returns>
    public static string RemoveConsecutiveText(string originalText, string textToRemove)
    {
        var result = originalText;

        while (result.Contains(textToRemove + textToRemove))
            result = result.Replace(textToRemove + textToRemove, textToRemove);

        return result;
    }

    /// <summary>
    /// Splits a string into multiple parts, each containing a specified number of lines.
    /// </summary>
    /// <param name="input">The input string to split.</param>
    /// <param name="linesPerPart">The number of lines per part.</param>
    /// <returns>An enumerable collection of strings, each containing the specified number of lines.</returns>
    public static IEnumerable<string> SplitStringByLines(string input, int linesPerPart)
    {
        if (string.IsNullOrEmpty(input) || linesPerPart <= 0)
        {
            yield return input;
            yield break;
        }

        var lines = input.Split(new[] { '\n' }, StringSplitOptions.None);
        var index = 0;

        while (index < lines.Length)
        {
            yield return string.Join("\n", lines.Skip(index).Take(linesPerPart));
            index += linesPerPart;
        }
    }
    #endregion

    #region Universal functions
    /// <summary>
    /// Tries to execute an action multiple times with a specified interval between each try, until it succeeds or reaches a maximum number of tries.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="maxTries">The maximum number of tries.</param>
    /// <param name="msInterval">The interval between tries in milliseconds.</param>
    public static void TryMultipleTimes(Action action, int maxTries = 5, int msInterval = 200)
    {
        while (maxTries > 0)
        {
            try
            {
                action.Invoke();
                break;
            }
            catch
            {
                if (--maxTries == 0) throw;
                Thread.Sleep(msInterval);
            }
        }
    }
    #endregion

    #region UI functions
    /// <summary>
    /// Determines whether the specified element is part of the specified <see cref="Popup"/> control.
    /// </summary>
    /// <param name="popup">The Popup control to check against.</param>
    /// <param name="element">The element to check.</param>
    /// <returns>True if the element is part of the Popup control; otherwise, false.</returns>
    public static bool IsChildOfPopup(Popup popup, DependencyObject element)
    {
        var parent = VisualTreeHelper.GetParent(element);
        while (parent != null)
        {
            if (parent == popup)
                return true;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return false;
    }

    /// <summary>
    /// Determines the current Windows theme color (Light or Dark) by checking the "AppsUseLightTheme" registry value.
    /// </summary>
    /// <returns>The current Windows theme as a <see cref="StswTheme"/> enumeration.</returns>
    public static StswTheme GetWindowsTheme()
    {
        var theme = StswTheme.Light;

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var registryValueObject = key?.GetValue("AppsUseLightTheme");
            if (registryValueObject == null)
                return theme;

            var registryValue = (int)registryValueObject;
            theme = registryValue > 0 ? StswTheme.Light : StswTheme.Dark;
        }
        catch
        {
            /// default to light theme in case of exception
        }

        return theme;
    }

    /// <summary>
    /// Removes a <see cref="FrameworkElement"/> from its parent container.
    /// </summary>
    /// <param name="element">The <see cref="FrameworkElement"/> to be removed from its parent.</param>
    public static void RemoveFromParent(FrameworkElement element)
    {
        if (element.Parent == null)
            return;

        switch (element.Parent)
        {
            case ContentControl contentControl:
                contentControl.Content = null;
                break;
            case ContentPresenter contentPresenter:
                contentPresenter.Content = null;
                break;
            case Decorator decorator:
                decorator.Child = null;
                break;
            case ItemsControl itemsControl:
                itemsControl.Items.Remove(element);
                break;
            case Panel panel:
                panel.Children.Remove(element);
                break;
        }
    }
    #endregion

    #region Validation functions
    /// <summary>
    /// Validates whether the specified email address is in a valid format.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns><see langword="true"/> if the email address is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidEmail(string email) => new EmailAddressAttribute().IsValid(email);

    /// <summary>
    /// Validates whether the specified phone number is in a valid format.
    /// </summary>
    /// <param name="number">The phone number to validate.</param>
    /// <returns><see langword="true"/> if the phone number contains between 7 and 15 digits; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidPhoneNumber(string number) => new string(number.Where(char.IsDigit).ToArray()).Length.Between(7, 15);

    /// <summary>
    /// Validates whether the specified URL is in a valid format and uses HTTP or HTTPS.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns><see langword="true"/> if the URL is valid, uses HTTP or HTTPS, and has a valid domain; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var result))
        {
            if (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps)
            {
                try
                {
                    var host = result.Host;
                    if (string.IsNullOrWhiteSpace(host))
                        return false;

                    return host.Contains('.') && !host.Any(char.IsWhiteSpace);
                }
                catch
                {
                    return false;
                }
            }
        }
        return false;
    }
    #endregion
}
