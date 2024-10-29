using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class StswDataGridColorColumn : DataGridTextColumn
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
    private static readonly Style StswEditingElementStyle = new(typeof(StswColorBox), (Style)Application.Current.FindResource(typeof(StswColorBox)))
    {
        Setters =
        {
            new Setter(StswColorBox.BorderThicknessProperty, new Thickness(0)),
            new Setter(StswColorBox.CornerClippingProperty, false),
            new Setter(StswColorBox.CornerRadiusProperty, new CornerRadius(0)),
            new Setter(StswColorBox.PaddingProperty, new Thickness(0)),
            new Setter(StswColorBox.SeparatorThicknessProperty, 0d),

            new Setter(StswColorBox.HorizontalAlignmentProperty, HorizontalAlignment.Stretch),
            new Setter(StswColorBox.HorizontalContentAlignmentProperty, HorizontalAlignment.Left),
            new Setter(StswColorBox.VerticalAlignmentProperty, VerticalAlignment.Stretch),
            new Setter(StswColorBox.VerticalContentAlignmentProperty, VerticalAlignment.Top)
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
        var editingElement = new StswColorBox();

        /// bindings
        if (Binding != null)
            BindingOperations.SetBinding(editingElement, StswColorBox.SelectedColorProperty, Binding);

        /// assign style
        editingElement.Style = StswEditingElementStyle;

        return editingElement;
    }
}