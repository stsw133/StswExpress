using System;
using System.Collections.Generic;
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

public static class Extensions
{
    /// Converts DataTable to list of T objects
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

    /// Returns true if parameter list contains given value
    public static bool In<T>(this T value, IEnumerable<T> input) => input.Any(n => Equals(n, value));

    /// Returns true if parameter array contains given value
    public static bool In<T>(this T value, params T[] input) => input.Any(n => Equals(n, value));

    /// Converts IEnumerable to ExtCollection
    public static ExtCollection<T> ToExtCollection<T>(this IEnumerable<T> value) => new ExtCollection<T>(value);

    /// Tries to do action a few times in case a single time could not work
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

    /// Tries to do func a few times in case a single time could not work
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

            if (child == null)
                child = GetVisualChild<T>(v);

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
                if (child != null && child is T)
                    yield return (T)child;

                foreach (T childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
    }
    #endregion

    #region ColumnFilters
    /// Gets controls of "ColumnFilter" type from DataGrid.
    public static void GetColumnFilters(this DataGrid dg, out string filter, out List<(string name, object val)> parameters)
    {
        filter = string.Empty;
        parameters = new List<(string, object)>();
        
        foreach (var col in dg.Columns)
        {
            /// Header is ColumnFilter
            if (col.Header is ColumnFilter cf1)
            {
                if (cf1.SqlString != null)
                {
                    filter += " and " + cf1.SqlString;
                    if (cf1.Value1 != null)
                        parameters.Add((cf1.SqlParam[..(cf1.SqlParam.Length > 120 ? 120 : cf1.SqlParam.Length)] + "1", (cf1.Value1 is List<object> ? null : cf1.Value1) ?? DBNull.Value));
                    if (cf1.Value2 != null)
                        parameters.Add((cf1.SqlParam[..(cf1.SqlParam.Length > 120 ? 120 : cf1.SqlParam.Length)] + "2", (cf1.Value2 is List<object> ? null : cf1.Value2) ?? DBNull.Value));
                }
            }
            /// Header's children are ColumnFilter
            else if (col.Header is DependencyObject cf2)
            {
                foreach (var cf in FindVisualChildren<ColumnFilter>(cf2).Where(x => x.SqlString != null))
                {
                    filter += " and " + cf.SqlString;
                    if (cf.Value1 != null)
                        parameters.Add((cf.SqlParam[..(cf.SqlParam.Length > 120 ? 120 : cf.SqlParam.Length)] + "1", (cf.Value1 is List<object> ? null : cf.Value1) ?? DBNull.Value));
                    if (cf.Value2 != null)
                        parameters.Add((cf.SqlParam[..(cf.SqlParam.Length > 120 ? 120 : cf.SqlParam.Length)] + "2", (cf.Value2 is List<object> ? null : cf.Value2) ?? DBNull.Value));
                }
            }
        }

        if (filter.StartsWith(" and "))
            filter = filter[5..];
        if (string.IsNullOrWhiteSpace(filter))
            filter = "1=1";
    }

    /// Clears values in controls of "ColumnFilter" type in DataGrid.
    public static void ClearColumnFilters(this DataGrid dg)
    {
        foreach (var col in dg.Columns)
        {
            /// Header is ColumnFilter
            if (col.Header is ColumnFilter cf1)
            {
                cf1.Value1 = cf1.ValueDef;
                cf1.Value2 = cf1.ValueDef;
            }
            /// Header's children are ColumnFilter
            else if (col.Header is DependencyObject cf2)
            {
                foreach (var cf in FindVisualChildren<ColumnFilter>(cf2).Where(x => x.SqlString != null))
                {
                    cf.Value1 = cf.ValueDef;
                    cf.Value2 = cf.ValueDef;
                }
            }
        }
    }
    #endregion
}
