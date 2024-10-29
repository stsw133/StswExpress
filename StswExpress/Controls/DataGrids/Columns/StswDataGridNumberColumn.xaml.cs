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

    private static readonly Style StswDisplayElementStyle = new(typeof(StswText), (Style)Application.Current.FindResource(typeof(StswText)))
    {
        Setters =
        {
            new Setter(StswText.HorizontalAlignmentProperty, HorizontalAlignment.Right),
            new Setter(StswText.VerticalAlignmentProperty, VerticalAlignment.Top)
        }
    };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="dataItem"></param>
    /// <returns></returns>
    protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
    {
        var displayElement = new StswText();

        /// bindings
        if (Binding != null)
            BindingOperations.SetBinding(displayElement, StswText.TextProperty, Binding);

        /// assign style
        displayElement.Style = StswDisplayElementStyle;

        return displayElement;
    }
}

/// <summary>
/// 
/// </summary>
public class StswDataGridDecimalColumn : StswDataGridNumberColumn<decimal>
{
    private static readonly Style StswEditingElementStyle = new(typeof(StswDecimalBox), (Style)Application.Current.FindResource(typeof(StswDecimalBox)))
    {
        Setters =
        {
            new Setter(StswDecimalBox.BorderThicknessProperty, new Thickness(0)),
            new Setter(StswDecimalBox.CornerClippingProperty, false),
            new Setter(StswDecimalBox.CornerRadiusProperty, new CornerRadius(0)),
            new Setter(StswDecimalBox.PaddingProperty, new Thickness(0)),
            new Setter(StswDecimalBox.SeparatorThicknessProperty, 0d),

            new Setter(StswDecimalBox.HorizontalAlignmentProperty, HorizontalAlignment.Stretch),
            new Setter(StswDecimalBox.HorizontalContentAlignmentProperty, HorizontalAlignment.Right),
            new Setter(StswDecimalBox.VerticalAlignmentProperty, VerticalAlignment.Stretch),
            new Setter(StswDecimalBox.VerticalContentAlignmentProperty, VerticalAlignment.Top)
        }
    };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="dataItem"></param>
    /// <returns></returns>
    protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
    {
        var editingElement = new StswDecimalBox();

        /// bindings
        if (Binding != null)
            BindingOperations.SetBinding(editingElement, StswDecimalBox.ValueProperty, Binding);

        /// assign style
        editingElement.Style = StswEditingElementStyle;

        return editingElement;
    }
}
