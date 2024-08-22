using System;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;
/// <summary>
/// Represents a control functioning as label.
/// </summary>
[Obsolete("Experimental control!")]
public class StswHeaderContainer : StswHeader, IStswCornerControl
{
    static StswHeaderContainer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswHeaderContainer), new FrameworkPropertyMetadata(typeof(StswHeaderContainer)));
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets the header of the control.
    /// </summary>
    public object? Header
    {
        get => (object?)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    public static readonly DependencyProperty HeaderProperty
        = DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(StswHeaderContainer)
        );

    /// <summary>
    /// 
    /// </summary>
    public string? HeaderStringFormat
    {
        get => (string?)GetValue(HeaderStringFormatProperty);
        set => SetValue(HeaderStringFormatProperty, value);
    }
    public static readonly DependencyProperty HeaderStringFormatProperty
        = DependencyProperty.Register(
            nameof(HeaderStringFormat),
            typeof(string),
            typeof(StswHeaderContainer)
        );

    /// <summary>
    /// 
    /// </summary>
    public DataTemplate HeaderTemplate
    {
        get => (DataTemplate)GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }
    public static readonly DependencyProperty HeaderTemplateProperty
        = DependencyProperty.Register(
            nameof(HeaderTemplate),
            typeof(DataTemplate),
            typeof(StswHeaderContainer)
        );

    /// <summary>
    /// 
    /// </summary>
    public DataTemplateSelector HeaderTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(HeaderTemplateSelectorProperty);
        set => SetValue(HeaderTemplateSelectorProperty, value);
    }
    public static readonly DependencyProperty HeaderTemplateSelectorProperty
        = DependencyProperty.Register(
            nameof(HeaderTemplateSelector),
            typeof(DataTemplateSelector),
            typeof(StswHeaderContainer)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// 
    /// </summary>
    public FontWeight HeaderFontWeight
    {
        get => (FontWeight)GetValue(HeaderFontWeightProperty);
        set => SetValue(HeaderFontWeightProperty, value);
    }
    public static readonly DependencyProperty HeaderFontWeightProperty
        = DependencyProperty.Register(
            nameof(HeaderFontWeight),
            typeof(FontWeight),
            typeof(StswHeaderContainer)
        );
    #endregion
}
