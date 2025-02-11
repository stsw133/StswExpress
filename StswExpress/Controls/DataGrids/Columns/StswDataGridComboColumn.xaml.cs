using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class StswDataGridComboColumn : DataGridComboBoxColumn
{
    //private static readonly Style StswDisplayElementStyle = new(typeof(StswText), (Style)Application.Current.FindResource(typeof(StswText)));
    private static readonly Style StswEditingElementStyle = new(typeof(StswComboBox), (Style)Application.Current.FindResource(typeof(StswComboBox)))
    {
        Setters =
        {
            new Setter(StswComboBox.BorderThicknessProperty, new Thickness(0)),
            new Setter(StswComboBox.CornerClippingProperty, false),
            new Setter(StswComboBox.CornerRadiusProperty, new CornerRadius(0)),
            new Setter(StswComboBox.FocusVisualStyleProperty, null),
            new Setter(StswComboBox.SeparatorThicknessProperty, 0.0),
            new Setter(StswComboBox.IsDropDownOpenProperty, true),
            new Setter(StswComboBox.HorizontalAlignmentProperty, HorizontalAlignment.Stretch),
            new Setter(StswComboBox.VerticalAlignmentProperty, VerticalAlignment.Stretch),
            new Setter(StswPopup.CornerClippingProperty, false),
            new Setter(StswPopup.CornerRadiusProperty, new CornerRadius(0))
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
        cell.PreviewKeyDown += OnPreviewKeyDown;

        var displayElement = new StswText()
        {
            //Style = StswDisplayElementStyle,
            Margin = new Thickness(2, 0, 2, 0),
            Padding = Padding,
            TextAlignment = TextAlignment,
            TextTrimming = TextTrimming,
            TextWrapping = TextWrapping
        };

        /// bindings
        if (SelectedItemBinding is Binding selectedItemBinding && !string.IsNullOrEmpty(DisplayMemberPath))
            BindingOperations.SetBinding(displayElement, StswText.TextProperty, new Binding
            {
                Path = new PropertyPath(selectedItemBinding.Path.Path + "." + DisplayMemberPath),
                Mode = BindingMode.OneWay
            });
        else if (SelectedValueBinding != null)
            BindingOperations.SetBinding(displayElement, StswText.TextProperty, SelectedValueBinding);

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
        var editingElement = new StswComboBox()
        {
            Style = StswEditingElementStyle,
            ItemsSource = ItemsSource,
            Padding = Padding,
            Placeholder = Placeholder,
            HorizontalContentAlignment = HorizontalContentAlignment,
            VerticalContentAlignment = VerticalContentAlignment
        };

        /// bindings
        if (SelectedItemBinding != null)
            BindingOperations.SetBinding(editingElement, StswComboBox.SelectedItemProperty, SelectedItemBinding);
        if (SelectedValueBinding != null)
            BindingOperations.SetBinding(editingElement, StswComboBox.SelectedValueProperty, SelectedValueBinding);

        return editingElement;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
            return;

        if (sender is DataGridCell cell && !cell.IsEditing)
            cell.IsEditing = true;
    }

    #region Logic properties
    /// <summary>
    /// 
    /// </summary>
    public string FilterMemberPath
    {
        get => (string)GetValue(FilterMemberPathProperty);
        set => SetValue(FilterMemberPathProperty, value);
    }
    public static readonly DependencyProperty FilterMemberPathProperty
        = DependencyProperty.Register(
            nameof(FilterMemberPath),
            typeof(string),
            typeof(StswDataGridComboColumn)
        );

    /// <summary>
    /// 
    /// </summary>
    public string FilterText
    {
        get => (string)GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }
    public static readonly DependencyProperty FilterTextProperty
        = DependencyProperty.Register(
            nameof(FilterText),
            typeof(string),
            typeof(StswDataGridComboColumn)
        );

    /// <summary>
    /// 
    /// </summary>
    public bool IsFilterEnabled
    {
        get => (bool)GetValue(IsFilterEnabledProperty);
        set => SetValue(IsFilterEnabledProperty, value);
    }
    public static readonly DependencyProperty IsFilterEnabledProperty
        = DependencyProperty.Register(
            nameof(IsFilterEnabled),
            typeof(bool),
            typeof(StswDataGridComboColumn)
        );

    /// <summary>
    /// 
    /// </summary>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }
    public static readonly DependencyProperty PlaceholderProperty
        = DependencyProperty.Register(
            nameof(PlaceholderProperty),
            typeof(string),
            typeof(StswDataGridComboColumn)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// 
    /// </summary>
    public Thickness Padding
    {
        get => (Thickness)GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }
    public static readonly DependencyProperty PaddingProperty
        = DependencyProperty.Register(
            nameof(PaddingProperty),
            typeof(Thickness),
            typeof(StswDataGridComboColumn)
        );

    /// <summary>
    /// 
    /// </summary>
    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }
    public static readonly DependencyProperty TextAlignmentProperty
        = DependencyProperty.Register(
            nameof(TextAlignmentProperty),
            typeof(TextAlignment),
            typeof(StswDataGridComboColumn)
        );

    /// <summary>
    /// 
    /// </summary>
    public TextTrimming TextTrimming
    {
        get => (TextTrimming)GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }
    public static readonly DependencyProperty TextTrimmingProperty
        = DependencyProperty.Register(
            nameof(TextTrimmingProperty),
            typeof(TextTrimming),
            typeof(StswDataGridComboColumn)
        );

    /// <summary>
    /// 
    /// </summary>
    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }
    public static readonly DependencyProperty TextWrappingProperty
        = DependencyProperty.Register(
            nameof(TextWrappingProperty),
            typeof(TextWrapping),
            typeof(StswDataGridComboColumn)
        );

    /// <summary>
    /// 
    /// </summary>
    public HorizontalAlignment HorizontalContentAlignment
    {
        get => (HorizontalAlignment)GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }
    public static readonly DependencyProperty HorizontalContentAlignmentProperty
        = DependencyProperty.Register(
            nameof(HorizontalContentAlignment),
            typeof(HorizontalAlignment),
            typeof(StswDataGridComboColumn),
            new PropertyMetadata(HorizontalAlignment.Left)
        );

    /// <summary>
    /// 
    /// </summary>
    public VerticalAlignment VerticalContentAlignment
    {
        get => (VerticalAlignment)GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }
    public static readonly DependencyProperty VerticalContentAlignmentProperty
        = DependencyProperty.Register(
            nameof(VerticalContentAlignment),
            typeof(VerticalAlignment),
            typeof(StswDataGridComboColumn),
            new PropertyMetadata(VerticalAlignment.Top)
        );
    #endregion
}