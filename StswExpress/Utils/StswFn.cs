using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Utility class providing various helper functions for general use.
/// </summary>
public static partial class StswFn
{
    #region Assembly functions
    /// <summary>
    /// Returns the name of the currently executing application.
    /// </summary>
    public static string? AppName() => Assembly.GetEntryAssembly()?.GetName().Name;

    /// <summary>
    /// Returns the version number of the currently executing application.
    /// </summary>
    public static string? AppVersion() => Assembly.GetEntryAssembly()?.GetName().Version?.ToString()?.TrimEnd('0', '.');

    /// <summary>
    /// Returns the name and version number of the currently executing application.
    /// </summary>
    public static string AppNameAndVersion => $"{AppName()} {(AppVersion() != "1" ? AppVersion() : string.Empty)}";

    /// <summary>
    /// Returns the copyright information for the currently executing application.
    /// </summary>
    public static string? AppCopyright => Assembly.GetEntryAssembly()?.Location is string location ? FileVersionInfo.GetVersionInfo(location).LegalCopyright : null;
    #endregion

    #region Bool functions
    /// <summary>
    /// Checks if element is part of <see cref="Popup"/> control.
    /// </summary>
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
    #endregion

    #region Color functions
    /// <summary>
    /// Generate new color based on passed value and the provided seed as parameter.
    /// </summary>
    /// <param name="text">Text to change into color.</param>
    /// <param name="seed">Brightness threshold.</param>
    /// <returns>Generated color.</returns>
    public static Color GenerateColor(string text, int seed)
    {
        var color = Colors.Transparent;

        /// generate new color
        if (!string.IsNullOrEmpty(text))
        {
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
            int r = hashBytes[0];
            int g = hashBytes[1];
            int b = hashBytes[2];

            if (seed >= 0)
            {
                if (r > seed)
                    r -= (r - seed) / 2;
                else if (r < seed)
                    r += (seed - r) / 2;

                if (g > seed)
                    g -= (g - seed) / 2;
                else if (g < seed)
                    g += (seed - g) / 2;

                if (b > seed)
                    b -= (b - seed) / 2;
                else if (b < seed)
                    b += (seed - b) / 2;
            }

            color = Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
        }

        return color;
    }
    #endregion

    #region File functions
    /// <summary>
    /// Checks if file is in use.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
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
    /// Moves a file to recycle bin.
    /// </summary>
    public static void MoveToRecycleBin(string filePath) => FileSystem.DeleteFile(filePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

    /// <summary>
    /// Opens a file in its associated application.
    /// </summary>
    public static void OpenFile(string path)
    {
        var process = new Process();
        process.StartInfo.FileName = path;
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.Verb = "open";
        process.Start();
    }
    #endregion

    #region Finding functions
    /// <summary>
    /// Finds the first visual ancestor of a specific type for the given control.
    /// </summary>
    public static T? FindVisualAncestor<T>(DependencyObject obj) where T : DependencyObject
    {
        if (obj != null)
        {
            var dependObj = obj;
            do
            {
                dependObj = GetParent(dependObj);
                if (dependObj is T)
                    return dependObj as T;
            }
            while (dependObj != null);
        }

        return null;
    }

    /// <summary>
    /// Finds the first visual child of a specific type for the given control.
    /// </summary>
    public static T? FindVisualChild<T>(DependencyObject obj) where T : Visual
    {
        if (obj == null)
            return null;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        {
            var child = VisualTreeHelper.GetChild(obj, i);
            if (child is T result)
                return result;
            else
            {
                var descendent = FindVisualChild<T>(child);
                if (descendent != null)
                    return descendent;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds all visual children of a specific type for the given control.
    /// </summary>
    public static IEnumerable<T> FindVisualChildren<T>(DependencyObject obj) where T : DependencyObject
    {
        if (obj != null)
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                if (VisualTreeHelper.GetChild(obj, i) is DependencyObject child)
                {
                    if (child is T t)
                        yield return t;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
    }

    /// <summary>
    /// Gets the parent of the given control.
    /// </summary>
    public static DependencyObject? GetParent(DependencyObject obj)
    {
        if (obj == null)
            return null;
        if (obj is ContentElement)
        {
            var parent = ContentOperations.GetParent(obj as ContentElement);
            if (parent != null)
                return parent;
            if (obj is FrameworkContentElement)
                return (obj as FrameworkContentElement)?.Parent;
            return null;
        }

        return VisualTreeHelper.GetParent(obj);
    }

    /// <summary>
    /// Gets the parent popup of the given control.
    /// </summary>
    /// <returns></returns>
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
    /// Shifts the selected index by the specified step, considering looping and boundary conditions.
    /// </summary>
    /// <param name="currIndex"></param>
    /// <param name="itemsCount"></param>
    /// <param name="step">The step value for shifting through items</param>
    /// <param name="isLoopingAllowed"></param>
    public static int ShiftIndexBy(int currIndex, int itemsCount, int step, bool isLoopingAllowed)
    {
        if (itemsCount <= 0)
            return -1;
        else if (isLoopingAllowed || !(currIndex + step >= itemsCount || currIndex + step < 0))
            return (currIndex + step % itemsCount + itemsCount) % itemsCount;
        else if (step > 0)
            return itemsCount - 1;
        else
            return 0;
    }

    /// <summary>
    /// Attempts to calculate a result from the provided mathematical expression using the order of operations and returns a bool indicating success or failure.
    /// The result of the calculation is stored in the result out parameter if successful.
    /// </summary>
    public static bool TryCalculateString(string expression, out decimal result, CultureInfo? culture = null)
    {
        try
        {
            culture ??= CultureInfo.CurrentCulture;

            /// remove unnecessary characters and replace NumberDecimalSeparator and NegativeSign
            expression = expression
                .Replace(Environment.NewLine, string.Empty)
                .Replace("\t", string.Empty)
                .Replace(" ", string.Empty)
                .Replace(culture.NumberFormat.NumberDecimalSeparator, ".")
                .Replace(culture.NumberFormat.NegativeSign, "-");

            /// find first and last number
            int i1, i2;
            int findFirstAndLastIndex(char[] signs)
            {
                var iSign = (expression[0] == '-' ? "_" + expression[1..] : expression).IndexOfAny(signs);
                i1 = iSign;
                while (i1 > 0
                        && (char.IsDigit(expression[i1 - 1])
                         || expression[i1 - 1] == '.'
                         || (expression[i1 - 1] == '-' && (i1 < 2 || !char.IsDigit(expression[i1 - 2]))))
                      )
                    i1--;

                i2 = iSign;
                do { i2++; } while (i2 == expression.Length - 1
                                || (i2 < expression.Length && (char.IsDigit(expression[i2]) || expression[i2] == '.' || (!char.IsDigit(expression[i2 - 1]) && expression[i2] == '-'))));

                return iSign;
            }
            /// replace
            decimal value;
            void expressionReplace()
            {
                var addPlusSign = i1 > 0 && char.IsDigit(expression[i1 - 1]);
                expression = expression.Remove(i1, i2 - i1);
                expression = expression.Insert(i1, $"{(value > 0 && addPlusSign ? "+" : string.Empty)}{value}");
            }

            /// do some math:
            /// first ( )
            while (expression.Any(x => x.In('(', ')')))
            {
                /// indexes
                i2 = expression.IndexOf(')') + 1;
                i1 = expression[..i2].LastIndexOf('(');

                /// result
                var part0 = expression[i1..i2];
                TryCalculateString(part0[1..^1], out value);
                expressionReplace();
            }
            /// next ^
            while (expression[1..].Any(x => x.In('^')))
            {
                var iSign = findFirstAndLastIndex(['^']);
                var number1 = Convert.ToDecimal(expression[i1..iSign], culture);
                var number2 = Convert.ToDecimal(expression[(iSign + 1)..i2], culture);
                value = Convert.ToDecimal(Math.Pow(Convert.ToDouble(number1), Convert.ToDouble(number2)));
                expressionReplace();
            }
            /// next * /
            while (expression[1..].Any(x => x.In('*', '/')))
            {
                var iSign = findFirstAndLastIndex(['*', '/']);
                var number1 = Convert.ToDecimal(expression[i1..iSign], culture);
                var number2 = Convert.ToDecimal(expression[(iSign + 1)..i2], culture);
                value = expression[iSign] == '*' ? number1 * number2 : number1 / number2;
                expressionReplace();
            }
            /// next + -
            while (expression[1..].Any(x => x.In('+', '-')))
            {
                var iSign = findFirstAndLastIndex(['+', '-']);
                var number1 = Convert.ToDecimal(expression[i1..iSign], culture);
                var number2 = Convert.ToDecimal(expression[(iSign + 1)..i2], culture);
                value = expression[iSign] == '+' ? number1 + number2 : number1 - number2;
                expressionReplace();
            }

            result = Convert.ToDecimal(expression, CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            result = 0;
            return false;
        }
    }
    #endregion

    #region Serialization functions
    /// <summary>
    /// 
    /// </summary>
    public static string? SerializeToJson<T>(T obj)
    {
        if (obj == null)
            return null;

        var serializer = new DataContractJsonSerializer(obj.GetType());
        using var stream = new MemoryStream();
        serializer.WriteObject(stream, obj);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// 
    /// </summary>
    public static T? DeserializeFromJson<T>(string json)
    {
        var serializer = new DataContractJsonSerializer(typeof(T));
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return (T?)serializer.ReadObject(stream);
    }
    #endregion

    #region Special functions
    /// <summary>
    /// Determines the owner of the process.
    /// </summary>
    /// <returns></returns>
    public static string? GetProcessUser(Process process)
    {
        var processHandle = IntPtr.Zero;
        try
        {
            OpenProcessToken(process.Handle, 8, out processHandle);
            var wi = new WindowsIdentity(processHandle);
            var user = wi.Name;
            return user.Contains('\\') ? user[(user.IndexOf('\\') + 1)..] : user;
        }
        catch
        {
            return null;
        }
        finally
        {
            if (processHandle != IntPtr.Zero)
                CloseHandle(processHandle);
        }
    }

    [LibraryImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(IntPtr hObject);

    /// <summary>
    /// Determines the current Windows theme color (Light or Dark) by checking the "AppsUseLightTheme" registry value.
    /// </summary>
    public static StswTheme GetWindowsTheme()
    {
        var theme = StswTheme.Light;

        try
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                var registryValueObject = key?.GetValue("AppsUseLightTheme");
                if (registryValueObject == null)
                    return 0;

                var registryValue = (int)registryValueObject;

                //if (SystemParameters.HighContrast)
                //    theme = 2;

                theme = (StswTheme)(registryValue > 0 ? 0 : 1);
            }

            return theme;
        }
        catch
        {
            return theme;
        }
    }
    #endregion

    #region Text functions
    /// <summary>
    /// Converts diacritics in string to their ASCII substitutes.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
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
    /// Removes consecutive occurrences of a specified string in another string.
    /// </summary>
    public static string RemoveConsecutiveText(string originalText, string textToRemove)
    {
        var result = originalText;

        while (result.Contains(textToRemove + textToRemove))
            result = result.Replace(textToRemove + textToRemove, textToRemove);

        return result;
    }

    /// <summary>
    /// Returns a new list of strings separated by a certain number of lines.
    /// </summary>
    public static IEnumerable<string> SplitStringByLines(string input, int linesPerPart)
    {
        if (string.IsNullOrEmpty(input) || linesPerPart <= 0)
        {
            yield return input;
            yield break;
        }

        var lines = input.Split(['\n'], StringSplitOptions.None);
        var index = 0;

        while (index < lines.Length)
        {
            yield return string.Join(string.Empty, lines.Skip(index).Take(linesPerPart));
            index += linesPerPart;
        }
    }
    #endregion

    #region Universal functions
    /// <summary>
    /// Opens the context menu of a <see cref="FrameworkElement"/> at its current position.
    /// </summary>
    public static void OpenContextMenu(FrameworkElement f)
    {
        f.ContextMenu.PlacementTarget = f;
        f.ContextMenu.IsOpen = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="f"></param>
    public static void RemoveFromParent(FrameworkElement f)
    {
        if (f.Parent == null)
            return;

        switch (f.Parent)
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
                itemsControl.Items.Remove(f);
                break;
            case Panel panel:
                panel.Children.Remove(f);
                break;
        }
    }
    #endregion

    #region Validation functions
    /// <summary>
    /// 
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public static bool IsValidEmail(string email) => new EmailAddressAttribute().IsValid(email);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsValidPhoneNumber(string number) => new string(number.ToCharArray().Where(char.IsDigit).ToArray()).Length.Between(9, 11);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static bool IsValidUrl(string url) => Uri.TryCreate(url, UriKind.Absolute, out var result) && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    #endregion
}
