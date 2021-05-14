using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace StswExpress
{
    public static class Extensions
    {
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
        public static ObservableCollection<T> ToList<T>(this DataTable table) where T : class, new()
        {
            try
            {
                ObservableCollection<T> list = new ObservableCollection<T>();

                foreach (var row in table.AsEnumerable())
                {
                    T obj = new T();

                    foreach (var prop in obj.GetType().GetProperties())
                    {
                        try
                        {
                            PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);
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

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;

                if (child == null)
                    child = GetVisualChild<T>(v);

                if (child != null)
                    break;
            }

            return child;
        }

        /// <summary>
        /// Find visual children of control of specified type
        /// </summary>
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
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
