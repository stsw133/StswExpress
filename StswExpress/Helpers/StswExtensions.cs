using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StswExpress;

public static class StswExtensions
{
    /// Converts byte[] to BitmapImage
    public static BitmapImage? AsImage(this byte[] imageData)
    {
        if (!imageData.Any())
            return null;

        var result = new BitmapImage();
        using (var mem = new MemoryStream(imageData))
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

    /// Converts DataTable to list of T objects
    public static List<T> AsList<T>(this DataTable table) where T : class, new()
    {
        var result = new List<T>();

        foreach (var row in table.AsEnumerable())
        {
            var obj = new T();

            foreach (var prop in obj.GetType().GetProperties().Where(p => p.Name.ToLower().In(table.Columns.Cast<DataColumn>().Select(x => x.ColumnName.ToLower()))))
            {
                try
                {
                    var propertyInfo = obj.GetType().GetProperty(prop.Name);

                    if (propertyInfo?.PropertyType != typeof(object))
                        propertyInfo?.SetValue(obj, row[prop.Name].ConvertTo(propertyInfo.PropertyType), null);
                    else
                        propertyInfo?.SetValue(obj, row[prop.Name], null);
                }
                catch
                {
                    continue;
                }
            }

            result.Add(obj);
        }

        return result;
    }

    /// Returns true if start <= item <= end
    public static bool Between<T>(this T item, T start, T end) => Comparer<T>.Default.Compare(item, start) >= 0 && Comparer<T>.Default.Compare(item, end) <= 0;

    /// Sets first letter to upper case and rest to lower case
    public static string Capitalize(this string value) => char.ToUpper(value.First()) + value[1..].ToLower();

    /// Changes object to different type
    public static object? ConvertTo(this object o, Type t)
    {
        if (o == null || o == DBNull.Value)
            return Nullable.GetUnderlyingType(t) == null ? default : Convert.ChangeType(null, t);
        else
        {
            var underlyingType = Nullable.GetUnderlyingType(t);
            return underlyingType == null
                ? Convert.ChangeType(o, t, CultureInfo.InvariantCulture)
                : Convert.ChangeType(o, underlyingType, CultureInfo.InvariantCulture);
        }
    }

    /// Returns true if parameter list or array contains given value
    public static bool In<T>(this T value, IEnumerable<T> input) => input.Any(n => Equals(n, value));
    public static bool In<T>(this T value, params T[] input) => input.Any(n => Equals(n, value));

    /// Converts IEnumerable to ObservableCollection or ExtCollection
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> value) => new ObservableCollection<T>(value);
    public static ExtCollection<T> ToExtCollection<T>(this IEnumerable<T> value) => new ExtCollection<T>(value);

    /// Converts IDictionary to ExtDictionary
    public static ExtDictionary<T1, T2> ToExtDictionary<T1, T2>(this IDictionary<T1, T2> value) => new ExtDictionary<T1, T2>(value);

    /// Converts string to Nullable
    public static T? ToNullable<T>(this string s) where T : struct
    {
        T? result = new();
        //try
        //{
        if (!string.IsNullOrEmpty(s) && s.Trim().Length > 0)
        {
            var conv = TypeDescriptor.GetConverter(typeof(T));
            result = (T?)conv.ConvertFrom(s);
        }
        //}
        //catch { }
        return result;
    }

    /// Trim start or end of string
    public static string TrimEnd(this string source, string value) => !source.EndsWith(value) ? source : source.Remove(source.LastIndexOf(value));
    public static string TrimStart(this string source, string value) => !source.StartsWith(value) ? source : source[value.Length..];

    /// Tries to do action or func a few times in case a single time could not work
    public static void TryMultipleTimes(this Action action, int maxTries = 5, int msInterval = 200)
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
                if (--maxTries <= 0)
                    throw;

                Thread.Sleep(msInterval);
            }
        }
    }
    public static T? TryMultipleTimes<T>(this Func<T> func, int maxTries = 5, int msInterval = 200)
    {
        while (maxTries > 0)
        {
            try
            {
                return func();
            }
            catch
            {
                if (--maxTries <= 0)
                    throw;

                Thread.Sleep(msInterval);
            }
        }
        return default;
    }

    #region VisualChildren
    /// Gets visual child of T type inside parent
    public static T? GetVisualChild<T>(DependencyObject parent) where T : Visual
    {
        T? child = default;

        var numVisuals = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < numVisuals; i++)
        {
            var v = (Visual)VisualTreeHelper.GetChild(parent, i);
            child = v as T;

            child ??= GetVisualChild<T>(v);

            if (child != null)
                break;
        }

        return child;
    }

    /// Finds all visual children of specified T type in parameter control
    public static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent != null)
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T t)
                    yield return t;

                foreach (T childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
    }
    #endregion

    #region ColumnFilters
    /// Gets controls of "ColumnFilter" type from DataGrid.
    [Obsolete]
    public static void GetColumnFilters(this DataGrid dg, out string filter, out List<(string name, object val)> parameters)
    {
        var dict = new ExtDictionary<string, StswColumnFilterData>();

        foreach (var col in dg.Columns)
        {
            /// Header is ColumnFilter
            if (col.Header is StswColumnFilter cf1)
                dict.Add(new KeyValuePair<string, StswColumnFilterData>(Guid.NewGuid().ToString(), cf1.Data));
            /// Header's children are ColumnFilter
            else if (col.Header is DependencyObject cf2)
                foreach (var cf in FindVisualChildren<StswColumnFilter>(cf2).Where(x => x.SqlString != null))
                    dict.Add(new KeyValuePair<string, StswColumnFilterData>(Guid.NewGuid().ToString(), cf.Data));
        }

        dict.GetColumnFilters(out filter, out parameters);
    }

    /// Clears values in controls of "ColumnFilter" type in DataGrid.
    [Obsolete]
    public static void ClearColumnFilters(this DataGrid dg)
    {
        var dict = new ExtDictionary<string, StswColumnFilterData>();

        foreach (var col in dg.Columns)
        {
            /// Header is ColumnFilter
            if (col.Header is StswColumnFilter cf1)
                dict.Add(new KeyValuePair<string, StswColumnFilterData>(Guid.NewGuid().ToString(), cf1.Data));
            /// Header's children are ColumnFilter
            else if (col.Header is DependencyObject cf2)
                foreach (var cf in FindVisualChildren<StswColumnFilter>(cf2).Where(x => x.SqlString != null))
                    dict.Add(new KeyValuePair<string, StswColumnFilterData>(Guid.NewGuid().ToString(), cf.Data));
        }

        dict.ClearColumnFilters();
    }

    /// Gets controls of "ColumnFilter" type from ExtDictionary.
    public static void GetColumnFilters(this ExtDictionary<string, StswColumnFilterData> dict, out string filter, out List<(string name, object val)> parameters)
    {
        filter = string.Empty;
        parameters = new List<(string, object)>();

        foreach (var elem in dict)
        {
            /// Header is StswColumnFilterData
            if (elem.Value?.SqlString != null)
            {
                filter += " and " + elem.Value.SqlString;
                if (elem.Value.Value1 != null && elem.Value.SqlParam != null)
                    parameters.Add((elem.Value.SqlParam[..(elem.Value.SqlParam.Length > 120 ? 120 : elem.Value.SqlParam.Length)] + "1", (elem.Value.Value1 is List<object> ? null : elem.Value.Value1) ?? DBNull.Value));
                if (elem.Value.Value2 != null && elem.Value.SqlParam != null)
                    parameters.Add((elem.Value.SqlParam[..(elem.Value.SqlParam.Length > 120 ? 120 : elem.Value.SqlParam.Length)] + "2", (elem.Value.Value2 is List<object> ? null : elem.Value.Value2) ?? DBNull.Value));
            }
        }

        if (filter.StartsWith(" and "))
            filter = filter[5..];
        if (string.IsNullOrWhiteSpace(filter))
            filter = "1=1";
    }

    /// Clears values in controls of "ColumnFilter" type in ExtDictionary.
    public static void ClearColumnFilters(this ExtDictionary<string, StswColumnFilterData> dict)
    {
        foreach (var pair in dict)
            dict[pair.Key]?.Clear();
    }
    #endregion
}
