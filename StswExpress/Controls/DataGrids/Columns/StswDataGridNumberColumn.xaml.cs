using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Numerics;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public abstract class StswDataGridNumberColumnBase<T, TControl> : DataGridTextColumn where T : struct, INumber<T> where TControl : StswNumberBoxBase<T>, new()
{
    //private static readonly Style StswDisplayElementStyle = new(typeof(StswText), (Style)Application.Current.FindResource(typeof(StswText)));
    private static readonly Style StswEditingElementStyle = new(typeof(TControl), (Style)Application.Current.FindResource(typeof(TControl)))
    {
        Setters =
        {
            new Setter(StswNumberBoxBase<T>.BorderThicknessProperty, new Thickness(0)),
            new Setter(StswNumberBoxBase<T>.CornerClippingProperty, false),
            new Setter(StswNumberBoxBase<T>.CornerRadiusProperty, new CornerRadius(0)),
            new Setter(StswNumberBoxBase<T>.FocusVisualStyleProperty, null),
            new Setter(StswNumberBoxBase<T>.SeparatorThicknessProperty, 0.0),
            new Setter(StswNumberBoxBase<T>.HorizontalAlignmentProperty, HorizontalAlignment.Stretch),
            new Setter(StswNumberBoxBase<T>.VerticalAlignmentProperty, VerticalAlignment.Stretch)
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
        if (Binding != null)
            BindingOperations.SetBinding(displayElement, StswText.TextProperty, Binding);

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
        return GenerateEditingElement<TControl>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TControl"></typeparam>
    /// <returns></returns>
    private TControl GenerateEditingElement<TControl>() where TControl : StswNumberBoxBase<T>, new()
    {
        var editingElement = new TControl
        {
            Style = StswEditingElementStyle,
            Format = Format,
            Padding = Padding,
            Placeholder = Placeholder,
            HorizontalContentAlignment = HorizontalContentAlignment,
            VerticalContentAlignment = VerticalContentAlignment
        };

        if (Binding != null)
            BindingOperations.SetBinding(editingElement, StswNumberBoxBase<T>.ValueProperty, Binding);

        return editingElement;
    }


    #region Logic properties
    /// <summary>
    /// 
    /// </summary>
    public string? Format
    {
        get => (string?)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }
    public static readonly DependencyProperty FormatProperty
        = DependencyProperty.Register(
            nameof(FormatProperty),
            typeof(string),
            typeof(StswDataGridNumberColumnBase<T, TControl>)
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
            typeof(StswDataGridNumberColumnBase<T, TControl>)
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
            typeof(StswDataGridNumberColumnBase<T, TControl>)
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
            typeof(StswDataGridNumberColumnBase<T, TControl>)
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
            typeof(StswDataGridNumberColumnBase<T, TControl>)
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
            typeof(StswDataGridNumberColumnBase<T, TControl>)
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
            typeof(StswDataGridNumberColumnBase<T, TControl>),
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
            typeof(StswDataGridNumberColumnBase<T, TControl>),
            new PropertyMetadata(VerticalAlignment.Top)
        );
    #endregion
}

/// <summary>
/// 
/// </summary>
public class StswDataGridDecimalColumn : StswDataGridNumberColumnBase<decimal, StswDecimalBox> { }

/// <summary>
/// 
/// </summary>
public class StswDataGridDoubleColumn : StswDataGridNumberColumnBase<double, StswDoubleBox> { }

/// <summary>
/// 
/// </summary>
public class StswDataGridIntegerColumn : StswDataGridNumberColumnBase<int, StswIntegerBox> { }
