using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace StswExpress;
/// <summary>
/// Provides helper methods for navigating and working with the logical or visual tree in WPF applications.
/// </summary>
internal static class LogicalOrVisualTreeHelper
{
    /// <summary>
    /// Retrieves the context name of a <see cref="DependencyObject"/> by navigating the logical or visual tree.
    /// </summary>
    /// <param name="dependencyObject">The dependency object to retrieve the context from.</param>
    /// <returns>A string representing the context name of the dependency object.</returns>
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

    /// <summary>
    /// Formats a <see cref="DependencyObject"/> to a string representation suitable for text ID generation.
    /// </summary>
    /// <param name="dependencyObject">The dependency object to format.</param>
    /// <param name="typeFullName">Indicates whether to use the full type name. If false, only the type name is used.</param>
    /// <returns>A formatted string representing the dependency object.</returns>
    internal static string FormatForTextId(this DependencyObject dependencyObject, bool typeFullName = false)
    {
        return $"{(dependencyObject as FrameworkElement)?.Name ?? string.Empty}[{(typeFullName ? dependencyObject.GetType().FullName : dependencyObject.GetType().Name)}]";
    }
}
