using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace StswExpress.Globals
{
    internal static class Extensions
    {
        /// <summary>
        /// Is value in list of given values
        /// </summary>
        public static bool In<T>(this T value, params T[] input)
        {
            return input.Any(n => Equals(n, value));
        }

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
                            propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null);
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
    }
}
