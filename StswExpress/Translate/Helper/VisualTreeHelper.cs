using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace StswExpress.Translate
{
    public static class LogicalOrVisualTreeHelper
    {
        public static string GetContextByName(this DependencyObject dependencyObject)
        {
            string result = string.Empty;

            if (dependencyObject != null)
            {
                if (dependencyObject is UserControl || dependencyObject is Window)
                    result = dependencyObject.FormatForTextId(true);
                else
                {
                    DependencyObject parent = dependencyObject is Visual || dependencyObject is Visual3D ? VisualTreeHelper.GetParent(dependencyObject) : LogicalTreeHelper.GetParent(dependencyObject);
                    result = GetContextByName(parent);
                }
            }

            return string.IsNullOrEmpty(result) ? dependencyObject?.FormatForTextId(true) ?? string.Empty : result;
        }

        public static string FormatForTextId(this DependencyObject dependencyObject, bool typeFullName = false)
            => $"{(dependencyObject as FrameworkElement)?.Name ?? string.Empty}[{(typeFullName ? dependencyObject.GetType().FullName : dependencyObject.GetType().Name)}]";
    }
}
