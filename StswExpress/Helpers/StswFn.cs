using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Utility class providing various helper functions for general use.
/// </summary>
public static class StswFn
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

    #region Enum functions
    /// <summary>
    /// Returns the next value in an enumeration, with wraparound if the end of the enumeration is reached.
    /// </summary>
    public static T GetNextEnumValue<T>(T value, int count = 1) where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        var index = Array.IndexOf(values, value);
        var nextIndex = (index + count) % values.Length;
        return values[nextIndex];
    }
    #endregion

    #region File functions
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
                if (VisualTreeHelper.GetChild(obj, i) is DependencyObject child and not null)
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
    #endregion

    #region Numeric functions
    /// <summary>
    /// Attempts to calculate a result from the provided mathematical expression using the order of operations and returns a bool indicating success or failure.
    /// The result of the calculation is stored in the result out parameter if successful.
    /// </summary>
    public static bool TryCalculateString(string expression, out double result, CultureInfo? culture = null)
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
            double value;
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
                var iSign = findFirstAndLastIndex(new char[] { '^' });
                var number1 = Convert.ToDouble(expression[i1..iSign], culture);
                var number2 = Convert.ToDouble(expression[(iSign + 1)..i2], culture);
                value = Math.Pow(number1, number2);
                expressionReplace();
            }
            /// next * /
            while (expression[1..].Any(x => x.In('*', '/')))
            {
                var iSign = findFirstAndLastIndex(new char[] { '*', '/' });
                var number1 = Convert.ToDouble(expression[i1..iSign], culture);
                var number2 = Convert.ToDouble(expression[(iSign + 1)..i2], culture);
                value = expression[iSign] == '*' ? number1 * number2 : number1 / number2;
                expressionReplace();
            }
            /// next + -
            while (expression[1..].Any(x => x.In('+', '-')))
            {
                var iSign = findFirstAndLastIndex(new char[] { '+', '-' });
                var number1 = Convert.ToDouble(expression[i1..iSign], culture);
                var number2 = Convert.ToDouble(expression[(iSign + 1)..i2], culture);
                value = expression[iSign] == '+' ? number1 + number2 : number1 - number2;
                expressionReplace();
            }

            result = Convert.ToDouble(expression, CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            result = 0;
            return false;
        }
    }
    #endregion

    #region Special functions
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
    /// Removes consecutive occurrences of a specified string in another string.
    /// </summary>
    public static string RemoveConsecutiveText(string originalText, string textToRemove)
    {
        var result = originalText;

        while (result.Contains(textToRemove + textToRemove))
            result = result.Replace(textToRemove + textToRemove, textToRemove);

        return result;
    }
    #endregion

    #region Universal functions
    /// <summary>
    /// Opens the context menu of a FrameworkElement at its current position.
    /// </summary>
    public static void OpenContextMenu(object sender)
    {
        if (sender is FrameworkElement f)
        {
            f.ContextMenu.PlacementTarget = f;
            f.ContextMenu.IsOpen = true;
        }
    }
    #endregion
}
