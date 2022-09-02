using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace StswExpress
{
    public static class Extensions
    {
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

        /// Returns true if value is inside input
        public static bool In<T>(this T value, params T[] input) => input.Any(n => Equals(n, value));

        /// Converts DataTable to list of T objects
        public static List<T> ToList<T>(this DataTable table) where T : class, new()
        {
            var list = new List<T>();

            foreach (var row in table.AsEnumerable())
            {
                var obj = new T();

                foreach (var prop in obj.GetType().GetProperties())
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

                list.Add(obj);
            }

            return list;
        }

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
    }
}
