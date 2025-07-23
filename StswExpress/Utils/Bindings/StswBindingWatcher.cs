using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// Provides a mechanism to observe changes in the binding assignment of a dependency property
/// and execute a callback when a binding is applied.
/// </summary>
[StswInfo("0.14.0")]
public class StswBindingWatcher
{
    /// <summary>
    /// Monitors a specified dependency property on a <see cref="FrameworkElement"/> 
    /// and invokes a callback when a binding is assigned to it.
    /// </summary>
    /// <param name="target">The target framework element whose property is being monitored.</param>
    /// <param name="property">The dependency property to observe for binding assignments.</param>
    /// <param name="callback">The action to invoke when a binding is assigned to the property.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="target"/>, <paramref name="property"/>, or <paramref name="callback"/> is null.
    /// </exception>
    public static void WatchBindingAssignment(FrameworkElement target, DependencyProperty property, Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        ArgumentNullException.ThrowIfNull(property);
        ArgumentNullException.ThrowIfNull(target);

        var descriptor = DependencyPropertyDescriptor.FromProperty(property, target.GetType());
        descriptor?.AddValueChanged(target, (sender, args) =>
            {
                if ((BindingExpression?)BindingOperations.GetBindingExpression(target, property) != null)
                    callback();
            });
    }
}
