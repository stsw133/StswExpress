using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace StswExpress;
/// <summary>
/// 
/// </summary>
internal static class LogicalOrVisualTreeHelper
{
    internal static string GetContextByName(this DependencyObject dependencyObject)
    {
        var result = string.Empty;

        if (dependencyObject != null)
        {
            if (dependencyObject is UserControl || dependencyObject is Window)
                result = dependencyObject.FormatForTextId(true);
            else
                result = GetContextByName(dependencyObject is Visual || dependencyObject is Visual3D ? VisualTreeHelper.GetParent(dependencyObject) : LogicalTreeHelper.GetParent(dependencyObject));
        }

        return string.IsNullOrEmpty(result) ? dependencyObject?.FormatForTextId(true) ?? string.Empty : result;
    }

    internal static string FormatForTextId(this DependencyObject dependencyObject, bool typeFullName = false)
    {
        return $"{(dependencyObject as FrameworkElement)?.Name ?? string.Empty}[{(typeFullName ? dependencyObject.GetType().FullName : dependencyObject.GetType().Name)}]";
    }
}
