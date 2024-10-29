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

    private static readonly Style StswDisplayElementStyle = new(typeof(StswText), (Style)Application.Current.FindResource(typeof(StswText)))
    {
        Setters =
        {
            new Setter(StswText.HorizontalAlignmentProperty, HorizontalAlignment.Left),
            new Setter(StswText.VerticalAlignmentProperty, VerticalAlignment.Top)
        }
    };
    private static readonly Style StswEditingElementStyle = new(typeof(StswTextBox), (Style)Application.Current.FindResource(typeof(StswTextBox)))
    {
        Setters =
        {
            new Setter(StswTextBox.BorderThicknessProperty, new Thickness(0)),
            new Setter(StswTextBox.CornerClippingProperty, false),
            new Setter(StswTextBox.CornerRadiusProperty, new CornerRadius(0)),
            new Setter(StswTextBox.PaddingProperty, new Thickness(0)),

            new Setter(StswTextBox.HorizontalAlignmentProperty, HorizontalAlignment.Stretch),
            new Setter(StswTextBox.HorizontalContentAlignmentProperty, HorizontalAlignment.Left),
            new Setter(StswTextBox.VerticalAlignmentProperty, VerticalAlignment.Stretch),
            new Setter(StswTextBox.VerticalContentAlignmentProperty, VerticalAlignment.Top)
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="dataItem"></param>
    /// <returns></returns>
    protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
    {
        var editingElement = new StswTextBox();

        /// bindings
        if (Binding != null)
            BindingOperations.SetBinding(editingElement, StswTextBox.TextProperty, Binding);

        /// assign style
        editingElement.Style = StswEditingElementStyle;

        return editingElement;
    }
}