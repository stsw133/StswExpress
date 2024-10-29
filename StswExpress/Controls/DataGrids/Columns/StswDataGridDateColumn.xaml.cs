using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class StswDataGridDateColumn : DataGridTextColumn
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
    private static readonly Style StswEditingElementStyle = new(typeof(StswDatePicker), (Style)Application.Current.FindResource(typeof(StswDatePicker)))
    {
        Setters =
        {
            new Setter(StswDatePicker.BorderThicknessProperty, new Thickness(0)),
            new Setter(StswDatePicker.CornerClippingProperty, false),
            new Setter(StswDatePicker.CornerRadiusProperty, new CornerRadius(0)),
            new Setter(StswDatePicker.PaddingProperty, new Thickness(0)),
            new Setter(StswDatePicker.SeparatorThicknessProperty, 0d),

            new Setter(StswDatePicker.HorizontalAlignmentProperty, HorizontalAlignment.Stretch),
            new Setter(StswDatePicker.HorizontalContentAlignmentProperty, HorizontalAlignment.Left),
            new Setter(StswDatePicker.VerticalAlignmentProperty, VerticalAlignment.Stretch),
            new Setter(StswDatePicker.VerticalContentAlignmentProperty, VerticalAlignment.Top)
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
        var editingElement = new StswDatePicker();

        /// bindings
        if (Binding != null)
            BindingOperations.SetBinding(editingElement, StswDatePicker.SelectedDateProperty, Binding);

        /// assign style
        editingElement.Style = StswEditingElementStyle;

        return editingElement;
    }
}