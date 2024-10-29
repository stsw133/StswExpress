using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class StswDataGridPathColumn : DataGridTextColumn
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
    private static readonly Style StswEditingElementStyle = new(typeof(StswPathPicker), (Style)Application.Current.FindResource(typeof(StswPathPicker)))
    {
        Setters =
        {
            new Setter(StswPathPicker.BorderThicknessProperty, new Thickness(0)),
            new Setter(StswPathPicker.CornerClippingProperty, false),
            new Setter(StswPathPicker.CornerRadiusProperty, new CornerRadius(0)),
            new Setter(StswPathPicker.PaddingProperty, new Thickness(0)),
            new Setter(StswPathPicker.SeparatorThicknessProperty, 0d),

            new Setter(StswPathPicker.HorizontalAlignmentProperty, HorizontalAlignment.Stretch),
            new Setter(StswPathPicker.HorizontalContentAlignmentProperty, HorizontalAlignment.Left),
            new Setter(StswPathPicker.VerticalAlignmentProperty, VerticalAlignment.Stretch),
            new Setter(StswPathPicker.VerticalContentAlignmentProperty, VerticalAlignment.Top)
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
        var editingElement = new StswPathPicker();
        
        /// bindings
        if (Binding != null)
            BindingOperations.SetBinding(editingElement, StswPathPicker.SelectedPathProperty, Binding);

        /// assign style
        editingElement.Style = StswEditingElementStyle;

        return editingElement;
    }
}