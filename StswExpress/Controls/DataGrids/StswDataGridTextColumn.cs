using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class StswDataGridTextColumn : DataGridTextColumn
{
    public string? Format { get; set; }

    public Style? StswDisplayElementStyle { get; set; } = Application.Current.TryFindResource("StswDataGridTextColumnDisplayStyle") as Style;
    public Style? StswEditingElementStyle { get; set; } = Application.Current.TryFindResource("StswDataGridTextColumnEditingStyle") as Style;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="dataItem"></param>
    /// <returns></returns>
    protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
    {
        var displayElement = new StswText();

        if (Binding != null)
            BindingOperations.SetBinding(displayElement, StswText.TextProperty, Binding);

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
        var editingElement = new StswTextBox();

        if (Binding != null)
            BindingOperations.SetBinding(editingElement, StswTextBox.TextProperty, Binding);

        if (StswEditingElementStyle != null)
            editingElement.Style = StswEditingElementStyle;

        return editingElement;
    }
}