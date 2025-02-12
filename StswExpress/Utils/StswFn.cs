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
using System.Text.RegularExpressions;
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
    /// <returns>The name of the currently executing application, or <see langword="null"/> if it cannot be determined.</returns>
    public static string? AppName() => Assembly.GetEntryAssembly()?.GetName().Name;

    /// <summary>
    /// Gets the version number of the currently executing application.
    /// </summary>
    /// <returns>The version number of the currently executing application as a string, or <see langword="null"/> if it cannot be determined.</returns>
    public static string? AppVersion()
    {
        if (Assembly.GetEntryAssembly()?.GetName().Version is Version version)
        {
            int?[] versionParts = [version.Major, version.Minor, version.Build, version.Revision];
            for (var i = versionParts.Length - 1; i >= 0; i--)
            {
                if (versionParts[i] == 0)
                    versionParts[i] = null;
                else
                    break;
            }

            return string.Join('.', versionParts.Where(part => part != null));
        }

        return null;
    }

    /// <summary>
    /// Gets the name and version number of the currently executing application.
    /// </summary>
    /// <returns>A string containing the name and version number of the currently executing application.</returns>
    public static string? AppNameAndVersion
    {
        get
        {
            var appName = AppName();
            var appVersion = AppVersion();

            return appVersion != "1" ? $"{appName} {appVersion}" : appName;
        }
    }

    /// <summary>
    /// Gets the copyright information for the currently executing application.
    /// </summary>
    /// <returns>The copyright information, or <see langword="null"/> if it cannot be determined.</returns>
    public static string? AppCopyright => Assembly.GetEntryAssembly()?.Location is string location ? FileVersionInfo.GetVersionInfo(location).LegalCopyright : null;

    /// <summary>
    /// Retrieves the content of an embedded resource file from a specified assembly.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly containing the resource.</param>
    /// <param name="resourcePath">The path of the resource file within the assembly.</param>
    /// <returns>The content of the resource file as a string.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the specified resource is not found.</exception>
    public static string? GetResourceText(string assemblyName, string resourcePath)
    {
        var resourceUri = new Uri($"pack://application:,,,/{assemblyName};component/{resourcePath}", UriKind.Absolute);

        var resource = Application.GetResourceStream(resourceUri);
        if (resource == null)
            return null;

        using var stream = resource.Stream;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Determines whether the currently executing assembly was built in debug mode.
    /// This method checks for the presence of the <see cref="DebuggableAttribute"/> 
    /// with JIT tracking enabled.
    /// </summary>
    /// <returns><see langword="true"/> if the assembly was compiled in debug mode; otherwise, <see langword="false"/>.</returns>
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

        if (h < 1.0 / 6) { r = c; g = x; }
        else if (h < 2.0 / 6) { r = x; g = c; }
        else if (h < 3.0 / 6) { g = c; b = x; }
        else if (h < 4.0 / 6) { g = x; b = c; }
        else if (h < 5.0 / 6) { r = x; b = c; }
        else { r = c; b = x; }

        return Color.FromArgb(
            alpha,
            (byte)Math.Round((r + m) * 255),
            (byte)Math.Round((g + m) * 255),
            (byte)Math.Round((b + m) * 255)
        );
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
        var r = color.R / 255.0;
        var g = color.G / 255.0;
        var b = color.B / 255.0;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));

        lightness = (max + min) / 2.0;

        if (max == min)
        {
            hue = 0;
            saturation = 0;
        }
        else
        {
            var delta = max - min;

            saturation = lightness > 0.5
                ? delta / (2.0 - max - min)
                : delta / (max + min);

            if (max == r)
                hue = (g - b) / delta + (g < b ? 6 : 0);
            else if (max == g)
                hue = (b - r) / delta + 2;
            else
                hue = (r - g) / delta + 4;

            hue *= 60;
        }
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
        hue = hue % 360;
        var h = (int)(hue / 60) % 6;
        var f = hue / 60 - Math.Floor(hue / 60);

        var v = (byte)(value * 255);
        var p = (byte)(v * (1 - saturation));
        var q = (byte)(v * (1 - f * saturation));
        var t = (byte)(v * (1 - (1 - f) * saturation));

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
        var r = color.R / 255.0;
        var g = color.G / 255.0;
        var b = color.B / 255.0;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;

        if (delta == 0)
            hue = 0;
        else if (max == r)
            hue = 60 * (((g - b) / delta) % 6);
        else if (max == g)
            hue = 60 * (((b - r) / delta) + 2);
        else
            hue = 60 * (((r - g) / delta) + 4);

        if (hue < 0)
            hue += 360;

        saturation = (max == 0) ? 0 : delta / max;

        value = max;
    }

    /// <summary>
    /// Generates a deterministic color based on the SHA-256 hash of the input text.
    /// The seed value adjusts brightness to ensure contrast variations.
    /// </summary>
    /// <param name="text">The text to convert into a color.</param>
    /// <param name="seed">The seed value used to adjust the brightness of the generated color.</param>
    /// <returns>A <see cref="Color"/> object generated from the text and adjusted by the seed value.</returns>
    public static Color GenerateColor(string text, int seed)
    {
        static int AdjustBrightness(int component, int seed) => component + (seed - component) / 2;

        if (string.IsNullOrEmpty(text))
            return Colors.Transparent;

        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        int r = hashBytes[0];
        int g = hashBytes[1];
        int b = hashBytes[2];

        if (seed >= 0 && seed <= 255)
        {
            r = AdjustBrightness(r, seed);
            g = AdjustBrightness(g, seed);
            b = AdjustBrightness(b, seed);
        }

        return Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
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
        using var mem = new MemoryStream(value);
        result.BeginInit();
        result.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
        result.CacheOption = BitmapCacheOption.OnLoad;
        result.StreamSource = mem;
        result.EndInit();
        result.Freeze();

        return result;
    }

    /// <summary>
    /// Merges properties of multiple objects into a single dynamic object. 
    /// In case of property name conflicts, properties from later objects will overwrite those from earlier ones.
    /// </summary>
    /// <param name="parameters">An array of objects to be merged.</param>
    /// <returns>A dynamic object containing all properties from the provided objects.</returns>
    public static dynamic MergeObjects(params object[] parameters)
    {
        var expando = new ExpandoObject() as IDictionary<string, object?>;

        foreach (var parameter in parameters)
        {
            if (parameter == null)
                continue;

            foreach (var property in parameter.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.GetIndexParameters().Length > 0)
                    continue;

                var propertyName = property.Name;
                var propertyValue = property.GetValue(parameter);
                expando[propertyName] = propertyValue;
            }
        }

        return (ExpandoObject)expando;
    }
    #endregion

    #region DateTime functions
    /// <summary>
    /// Generates a list of unique year and month tuples within a specified date range.
    /// </summary>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <returns>A list of tuples, where each tuple contains the year and month within the specified date range.</returns>
    public static List<(int Year, int Month)> GetUniqueMonthsFromRange(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
            return [];

        var monthsDifference = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month + 1;
        var months = new List<(int Year, int Month)>(monthsDifference);

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
    /// <summary>
    /// Executes a specified verb action on the given file, such as opening or printing the file.
    /// </summary>
    /// <param name="path">The path to the file to be opened or printed.</param>
    /// <param name="verb">The action to perform on the file (e.g., "open", "print").</param>
    private static void ExecuteFileAction(string path, string verb)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"File '{path}' not found.", path);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                Verb = verb
            }
        };
        process.Start();
    }

    /// <summary>
    /// Extracts the associated icon of the specified file or directory path.
    /// If the path points to a directory, attempts to retrieve the default folder icon.
    /// </summary>
    /// <param name="path">The file or directory path to extract the icon from.</param>
    /// <returns>The associated icon as an <see cref="ImageSource"/> if found; otherwise, <see langword="null"/>.</returns>
    public static System.Drawing.Icon? ExtractAssociatedIcon(string? path)
    {
        const uint SHGFI_ICON = 0x100;
        const uint SHGFI_LARGEICON = 0x0;

        if (!Path.Exists(path))
            return null;

        if ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
        {
            var shinfo = new SHFILEINFO();
            if (SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_LARGEICON) != IntPtr.Zero)
                return System.Drawing.Icon.FromHandle(shinfo.hIcon);
        }
        else
        {
            return System.Drawing.Icon.ExtractAssociatedIcon(path);
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
        if (!File.Exists(path))
            return false;

        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Moves a file to the Windows recycle bin instead of deleting it permanently.
    /// Uses the <see cref="FileSystem.DeleteFile"/> method 
    /// with <see cref="RecycleOption.SendToRecycleBin"/>.
    /// </summary>
    /// <param name="path">The path to the file to be moved to the recycle bin.</param>
    public static void MoveToRecycleBin(string path) => FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

    /// <summary>
    /// Opens a file in its associated application.
    /// </summary>
    /// <param name="path">The path to the file to be opened.</param>
    public static void OpenFile(string path) => ExecuteFileAction(path, "open");

    /// <summary>
    /// Prints a file in its associated application.
    /// </summary>
    /// <param name="path">The path to the file to be printed.</param>
    public static void PrintFile(string path) => ExecuteFileAction(path, "print");
    #endregion

    #region Finding functions
    /// <summary>
    /// Finds the first logical child of a specific type for the given control.
    /// </summary>
    /// <typeparam name="T">The type of the child to find.</typeparam>
    /// <param name="obj">The control for which to find the logical child.</param>
    /// <returns>The first logical child of the specified type, or <see langword="null"/> if no such child exists.</returns>
    public static T? FindLogicalChild<T>(DependencyObject obj) where T : class
    {
        foreach (var child in LogicalTreeHelper.GetChildren(obj))
        {
            if (child is T result)
                return result;

            if (child is DependencyObject depChild)
            {
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
    /// <returns>The first visual ancestor of the specified type, or <see langword="null"/> if no such ancestor exists.</returns>
    public static T? FindVisualAncestor<T>(DependencyObject obj) where T : class
    {
        var parent = obj;

        while (parent != null)
        {
            parent = GetParent(parent);
            if (parent is T ancestor)
                return ancestor;
        }

        return null;
    }

    /// <summary>
    /// Finds the first visual child of a specific type for the given control.
    /// </summary>
    /// <typeparam name="T">The type of the child to find.</typeparam>
    /// <param name="obj">The control for which to find the visual child.</param>
    /// <returns>The first visual child of the specified type, or <see langword="null"/> if no such child exists.</returns>
    public static T? FindVisualChild<T>(DependencyObject obj) where T : class
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
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
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        {
            var child = VisualTreeHelper.GetChild(obj, i);
            if (child is T t)
                yield return t;

            foreach (var childOfChild in FindVisualChildren<T>(child))
                yield return childOfChild;
        }
    }

    /// <summary>
    /// Gets the parent of the given control.
    /// </summary>
    /// <param name="obj">The control for which to find the parent.</param>
    /// <returns>The parent of the control, or <see langword="null"/> if no parent exists.</returns>
    public static DependencyObject? GetParent(DependencyObject obj)
    {
        if (obj == null)
            return null;

        if (obj is ContentElement contentElement)
        {
            var parent = ContentOperations.GetParent(contentElement);
            if (parent != null)
                return parent;

            if (contentElement is FrameworkContentElement frameworkContentElement)
                return frameworkContentElement.Parent;
        }

        return VisualTreeHelper.GetParent(obj);
    }

    /// <summary>
    /// Gets the parent popup of the given control.
    /// </summary>
    /// <param name="obj">The control for which to find the parent popup.</param>
    /// <returns>The parent <see cref="Popup"/> control, or <see langword="null"/> if no parent popup exists.</returns>
    public static Popup? GetParentPopup(DependencyObject obj)
    {
        var parent = VisualTreeHelper.GetParent(obj);

        while (parent != null)
        {
            if (parent is Popup popup)
                return popup;

            var logicalParent = LogicalTreeHelper.GetParent(parent);
            if (logicalParent is Popup logicalPopup)
                return logicalPopup;

            parent = VisualTreeHelper.GetParent(parent);
        }

        return null;
    }
    #endregion

    #region List functions
    /// <summary>
    /// Generates a collection of random items of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of items to generate.</typeparam>
    /// <param name="count">The number of random items to generate.</param>
    /// <returns>An enumerable collection of randomly generated items.</returns>
    public static IEnumerable<T> CreateRandomItems<T>(int count) => StswRandomGenerator.CreateRandomItems<T>(count);
    #endregion

    #region Logical functions
    public static readonly bool True = true;
    public static readonly bool False = false;
    #endregion

    #region Numeric functions
    /// <summary>
    /// Evaluates a mathematical expression represented as a string.
    /// Supports basic arithmetic operations such as +, -, *, /, and parentheses.
    /// </summary>
    /// <param name="expression">The mathematical expression to evaluate.</param>
    /// <returns>The result of the evaluated expression as a <see cref="double"/>.</returns>
    /// <exception cref="FormatException">Thrown if the expression contains invalid syntax.</exception>
    public static double Compute(string expression)
    {
        try
        {
            return StswCalculator.EvaluatePostfix(StswCalculator.ConvertToPostfix(expression));
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Invalid mathematical expression: {expression}", ex);
        }
    }

    /// <summary>
    /// Attempts to evaluate a mathematical expression written in infix notation.
    /// Supports basic arithmetic operations such as +, -, *, /, and parentheses.
    /// </summary>
    /// <param name="expression">The mathematical expression to evaluate.</param>
    /// <param name="result">The result of the evaluated expression.</param>
    /// <returns><see langword="true"/> if the expression was successfully evaluated; otherwise, <see langword="false"/>.</returns>
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
    /// Replaces diacritical marks in a string with their ASCII equivalents.
    /// Uses Unicode normalization to decompose characters and remove 
    /// non-spacing marks.
    /// Useful for text search and standardization.
    /// </summary>
    /// <param name="text">The input string containing diacritics.</param>
    /// <returns>The normalized string with diacritics replaced by ASCII characters.</returns>
    public static string NormalizeDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(text.Length);

        foreach (var c in normalizedString)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(c);
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Removes consecutive occurrences of a specified string from the original text.
    /// </summary>
    /// <param name="originalText">The original text to process.</param>
    /// <param name="textToRemove">The string to remove when consecutive occurrences are found.</param>
    /// <returns>The processed string with consecutive occurrences removed.</returns>
    public static string RemoveConsecutiveText(string originalText, string textToRemove)
    {
        if (string.IsNullOrEmpty(textToRemove))
            return originalText;

        var pattern = $"({Regex.Escape(textToRemove)})+";
        return Regex.Replace(originalText, pattern, textToRemove);
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

        for (int i = 0; i < lines.Length; i += linesPerPart)
            yield return string.Join("\n", lines, i, Math.Min(linesPerPart, lines.Length - i));
    }
    #endregion

    #region UI functions
    /// <summary>
    /// Determines whether the specified element is part of the specified <see cref="Popup"/> control.
    /// </summary>
    /// <param name="popup">The <see cref="Popup"/> control to check against.</param>
    /// <param name="element">The element to check.</param>
    /// <returns><see langword="true"/> if the element is part of the <see cref="Popup"/> control; otherwise, <see langword="false"/>.</returns>
    public static bool IsChildOfPopup(Popup popup, DependencyObject element)
    {
        var parent = VisualTreeHelper.GetParent(element);
        while (parent != null)
        {
            if (parent == popup)
                return true;
            parent = VisualTreeHelper.GetParent(parent);
        }

        parent = LogicalTreeHelper.GetParent(element);
        while (parent != null)
        {
            if (parent == popup)
                return true;
            parent = LogicalTreeHelper.GetParent(parent);
        }

        return false;
    }

    /// <summary>
    /// Determines the current Windows theme (Light or Dark) by reading system registry settings.
    /// </summary>
    /// <returns>The current Windows theme as a <see cref="StswTheme"/> enumeration.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if access to the registry is denied.</exception>
    public static string? GetWindowsTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var registryValueObject = key?.GetValue("AppsUseLightTheme");
            if (registryValueObject is int registryValue)
                return registryValue > 0 ? "Light" : "Dark";
        }
        catch (Exception)
        {
            // Optionally log the exception here if needed
        }

        return "Light";
    }

    /// <summary>
    /// Removes a <see cref="FrameworkElement"/> from its parent container.
    /// Supports <see cref="ContentControl"/>, <see cref="ContentPresenter"/>, <see cref="Decorator"/>, <see cref="ItemsControl"/>, and <see cref="Panel"/>.
    /// </summary>
    /// <param name="element">The element to remove from its parent.</param>
    public static void RemoveFromParent(FrameworkElement element)
    {
        var parent = element.Parent;
        if (parent == null)
            return;

        switch (parent)
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

    #region Universal functions
    /// <summary>
    /// Attempts to execute the specified action multiple times until it succeeds or reaches the maximum number of attempts.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="maxTries">The maximum number of attempts before failing. Defaults to 5.</param>
    /// <param name="msInterval">The delay in milliseconds between attempts. Defaults to 200ms.</param>
    /// <remarks>
    /// If the action throws an exception, it will be retried up to <paramref name="maxTries"/> times.
    /// </remarks>
    public static void TryMultipleTimes(Action action, int maxTries = 5, int msInterval = 200)
    {
        for (var attempt = 1; attempt <= maxTries; attempt++)
        {
            try
            {
                action.Invoke();
                break;
            }
            catch (Exception) when (attempt < maxTries)
            {
                Thread.Sleep(msInterval);
            }
            catch
            {
                throw;
            }
        }
    }
    #endregion

    #region Validation functions
    /// <summary>
    /// Validates whether the specified string contains only valid email addresses.
    /// Multiple email addresses can be separated by a specified character.
    /// </summary>
    /// <param name="emails">A string containing one or more email addresses to validate.</param>
    /// <param name="separator">The character used to separate multiple email addresses. Defaults to ','.</param>
    /// <returns><see langword="true"/> if all email addresses are valid; otherwise, <see langword="false"/>.</returns>
    public static bool AreValidEmails(string emails, char separator = ',')
    {
        if (string.IsNullOrWhiteSpace(emails))
            return false;

        var emailList = emails.Split(separator, StringSplitOptions.TrimEntries);
        var emailValidator = new EmailAddressAttribute();
        return emailList.All(emailValidator.IsValid);
    }

    /// <summary>
    /// Validates whether the specified email address is in a valid format.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns><see langword="true"/> if the email address is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidEmail(string email) => new EmailAddressAttribute().IsValid(email);

    /// <summary>
    /// Validates whether a phone number matches the expected format for a specified country.
    /// </summary>
    /// <param name="number">The phone number to validate.</param>
    /// <param name="countryCode">The country code (e.g., "PL" for Poland, "US" for United States).</param>
    /// <returns><see langword="true"/> if the phone number is valid; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Supports PL (Poland), UK (United Kingdom), US (United States), and generic validation for other countries.
    /// </remarks>
    public static bool IsValidPhoneNumber(string number, string countryCode)
    {
        var digits = new string(number.Where(char.IsDigit).ToArray());

        switch (countryCode.ToUpper())
        {
            case "PL":
                if (digits.Length == 9)
                    return true;

                if (digits.Length == 11 && digits.StartsWith("48"))
                    return true;

                return false;
            case "UK":
                return digits.Length == 10 || digits.Length == 11;
            case "US":
                return digits.Length == 10;
            default:
                return digits.Length >= 7 && digits.Length <= 15;
        }
    }

    /// <summary>
    /// Validates whether the specified URL is in a valid format and uses HTTP or HTTPS.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns><see langword="true"/> if the URL is valid, uses HTTP or HTTPS, and has a valid domain; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var result)
         && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps))
        {
            var host = result.Host;
            return !string.IsNullOrWhiteSpace(host) && host.Contains('.') && !host.Any(char.IsWhiteSpace);
        }
        return false;
    }
    #endregion

    private struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    [DllImport("shell32.dll")]
    static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
}
