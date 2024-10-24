using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Numerics;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public abstract class StswDataGridNumberColumn<T> : DataGridTextColumn where T : struct, INumber<T>
{
    public string? Format { get; set; }

    public Style? StswDisplayElementStyle { get; set; } = Application.Current.TryFindResource("StswDataGridNumberColumnDisplayStyle") as Style;

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
}

/// <summary>
/// 
/// </summary>
public class StswDataGridDecimalColumn : StswDataGridNumberColumn<decimal>
{
    public Style? StswEditingElementStyle { get; set; } = Application.Current.TryFindResource("StswDataGridDecimalColumnEditingStyle") as Style;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="dataItem"></param>
    /// <returns></returns>
    protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
    {
        var editingElement = new StswDecimalBox();

        if (Binding != null)
            BindingOperations.SetBinding(editingElement, StswDecimalBox.ValueProperty, Binding);

        if (StswEditingElementStyle != null)
            editingElement.Style = StswEditingElementStyle;

        return editingElement;
    }
}
