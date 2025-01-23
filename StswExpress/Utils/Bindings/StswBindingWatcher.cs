using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class StswBindingWatcher
{
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
