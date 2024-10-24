using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class StswDataGridCheckColumn : DataGridCheckBoxColumn
{
    public string? Format { get; set; }

    public Style? StswDisplayElementStyle { get; set; } = Application.Current.TryFindResource("StswDataGridCheckColumnDisplayStyle") as Style;
    public Style? StswEditingElementStyle { get; set; } = Application.Current.TryFindResource("StswDataGridCheckColumnEditingStyle") as Style;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="dataItem"></param>
    /// <returns></returns>
    protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
    {
        var displayElement = new StswCheckBox();

        if (Binding != null)
            BindingOperations.SetBinding(displayElement, StswCheckBox.IsCheckedProperty, Binding);

        if (StswDisplayElementStyle != null)
            displayElement.Style = StswDisplayElementStyle;

        return displayElement;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="dataItem"></param>
    /// <returns></returns>
    protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
    {
        var editingElement = new StswCheckBox();

        if (Binding != null)
            BindingOperations.SetBinding(editingElement, StswCheckBox.IsCheckedProperty, Binding);

        if (StswEditingElementStyle != null)
            editingElement.Style = StswEditingElementStyle;

        return editingElement;
    }
}