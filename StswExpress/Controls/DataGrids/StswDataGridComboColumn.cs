using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class StswDataGridComboColumn : DataGridComboBoxColumn
{
    public string? Format { get; set; }

    public Style? StswDisplayElementStyle { get; set; } = Application.Current.TryFindResource("StswDataGridComboColumnDisplayStyle") as Style;
    public Style? StswEditingElementStyle { get; set; } = Application.Current.TryFindResource("StswDataGridComboColumnEditingStyle") as Style;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="dataItem"></param>
    /// <returns></returns>
    protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
    {
        var displayElement = new StswComboBox();

        if (SelectedItemBinding != null)
            BindingOperations.SetBinding(displayElement, StswComboBox.SelectedItemProperty, SelectedItemBinding);
        if (SelectedValueBinding != null)
            BindingOperations.SetBinding(displayElement, StswComboBox.SelectedValueProperty, SelectedValueBinding);

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
        var editingElement = new StswComboBox();

        if (SelectedItemBinding != null)
            BindingOperations.SetBinding(editingElement, StswComboBox.SelectedItemProperty, SelectedItemBinding);
        if (SelectedValueBinding != null)
            BindingOperations.SetBinding(editingElement, StswComboBox.SelectedValueProperty, SelectedValueBinding);

        if (StswEditingElementStyle != null)
            editingElement.Style = StswEditingElementStyle;

        return editingElement;
    }
}