using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// Helper methods for creating bindings to properties of <see cref="DataGridColumn"/> instances.
/// </summary>
internal static class StswDataGridColumnBindingHelper
{
    /// <summary>
    /// Creates a one-way binding to a property of the provided <paramref name="column"/>.
    /// </summary>
    /// <param name="column">The column that owns the property.</param>
    /// <param name="propertyName">The name of the property to bind to.</param>
    /// <returns>A one-way binding pointing to the requested column property.</returns>
    public static Binding CreateColumnBinding(this DataGridColumn column, string propertyName) => new(propertyName)
    {
        Source = column,
        Mode = BindingMode.OneWay
    };
}
