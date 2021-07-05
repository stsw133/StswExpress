using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace StswExpress
{
    public static class Extensions
    {
        /// <summary>
        /// Is value between 2 given values
        /// </summary>
        public static bool Between<T>(this T item, T start, T end) => Comparer<T>.Default.Compare(item, start) >= 0 && Comparer<T>.Default.Compare(item, end) <= 0;

        /// <summary>
        /// Set first letter to upper and rest to lower
        /// </summary>
        public static string Capitalize(this string value) => char.ToUpper(value.First()) + value[1..].ToLower();

        /// <summary>
        /// Is value in list of given values
        /// </summary>
        public static bool In<T>(this T value, params T[] input) => input.Any(n => Equals(n, value));

        /// <summary>
        /// Convert DataTable to list of any model class
        /// </summary>
        public static IList<T> ToList<T>(this DataTable table, string ignoreString = null) where T : class, new()
        {
            try
            {
                IList<T> list = new List<T>();

                /// ignore string
                if (ignoreString?.Length > 0)
                    for (int i = 0; i < table.Columns.Count; i++)
                        table.Columns[i].ColumnName = table.Columns[i].ColumnName.Replace(ignoreString, string.Empty);

                /// assign values to properties
                foreach (var row in table.AsEnumerable())
                {
                    var obj = new T();

                    foreach (var prop in obj.GetType().GetProperties())
                    {
                        try
                        {
                            var propertyInfo = obj.GetType().GetProperty(prop.Name);

                            if (propertyInfo.PropertyType != typeof(object))
                                propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null);
                            else
                                propertyInfo.SetValue(obj, row[prop.Name], null);
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
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get visual child of control
        /// </summary>
        public static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default;

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

        /// <summary>
        /// Find visual children of specified type in control
        /// </summary>
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
