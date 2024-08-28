using System.Windows;
using System.Windows.Controls;

namespace StswExpress;
/// <summary>
/// Represents a control that functions as a label container.
/// </summary>
public class StswLabelContainer : StswLabel, IStswCornerControl
{
    static StswLabelContainer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswLabelContainer), new FrameworkPropertyMetadata(typeof(StswLabelContainer)));
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets the content of the header.
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
            typeof(StswLabelContainer)
        );

    /// <summary>
    /// Gets or sets the string format for the header content.
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
            typeof(StswLabelContainer)
        );

    /// <summary>
    /// Gets or sets the data template for the header content.
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
            typeof(StswLabelContainer)
        );

    /// <summary>
    /// Gets or sets the data template selector for the header content.
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
            typeof(StswLabelContainer)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the horizontal alignment of the header.
    /// </summary>
    public static readonly DependencyProperty HeaderAlignmentProperty
        = DependencyProperty.RegisterAttached(
            nameof(HeaderAlignmentProperty)[..^8],
            typeof(HorizontalAlignment?),
            typeof(StswLabelContainer),
            new PropertyMetadata(default, OnHeaderAlignmentChanged)
        );
    public static void SetHeaderAlignment(DependencyObject obj, HorizontalAlignment value) => obj.SetValue(HeaderAlignmentProperty, value);
    private static void OnHeaderAlignmentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is FrameworkElement element && e.NewValue is HorizontalAlignment alignment)
        {
            void Update()
            {
                if (element is StswLabelContainer)
                {
                    element.ApplyTemplate();
                    if (StswFn.FindVisualChild<StswHeader>(element) is StswHeader header)
                        header.HorizontalContentAlignment = alignment;
                }
                else
                {
                    foreach (var container in StswFn.FindVisualChildren<StswLabelContainer>(element))
                    {
                        container.ApplyTemplate();
                        if (StswFn.FindVisualChild<StswHeader>(container) is StswHeader header)
                            header.HorizontalContentAlignment = alignment;
                    }
                }
            }

            if (element.IsLoaded)
            {
                Update();
            }
            else
            {
                void loadedHandler(object s, RoutedEventArgs args)
                {
                    Update();
                    element.Loaded -= loadedHandler;
                }
                element.Loaded += loadedHandler;
            }
        }
    }

    /// <summary>
    /// Gets or sets the font weight of the header.
    /// </summary>
    public static readonly DependencyProperty HeaderFontWeightProperty
        = DependencyProperty.RegisterAttached(
            nameof(HeaderFontWeightProperty)[..^8],
            typeof(FontWeight?),
            typeof(StswLabelContainer),
            new PropertyMetadata(default, OnHeaderFontWeightChanged)
        );
    public static void SetHeaderFontWeight(DependencyObject obj, FontWeight value) => obj.SetValue(HeaderFontWeightProperty, value);
    private static void OnHeaderFontWeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is FrameworkElement element && e.NewValue is FontWeight weight)
        {
            void Update()
            {
                if (element is StswLabelContainer)
                {
                    element.ApplyTemplate();
                    if (StswFn.FindVisualChild<StswHeader>(element) is StswHeader header)
                        header.FontWeight = weight;
                }
                else
                {
                    foreach (var container in StswFn.FindVisualChildren<StswLabelContainer>(element))
                    {
                        container.ApplyTemplate();
                        if (StswFn.FindVisualChild<StswHeader>(container) is StswHeader header)
                            header.FontWeight = weight;
                    }
                }
            }

            if (element.IsLoaded)
            {
                Update();
            }
            else
            {
                void loadedHandler(object s, RoutedEventArgs args)
                {
                    Update();
                    element.Loaded -= loadedHandler;
                }
                element.Loaded += loadedHandler;
            }
        }
    }

    /// <summary>
    /// Gets or sets the orientation of the header.
    /// </summary>
    public static readonly DependencyProperty HeaderOrientationProperty
        = DependencyProperty.RegisterAttached(
            nameof(HeaderOrientationProperty)[..^8],
            typeof(Orientation?),
            typeof(StswLabelContainer),
            new PropertyMetadata(default, OnHeaderOrientationChanged)
        );
    public static void SetHeaderOrientation(DependencyObject obj, Orientation value) => obj.SetValue(HeaderOrientationProperty, value);
    private static void OnHeaderOrientationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is FrameworkElement element && e.NewValue is Orientation orientation)
        {
            void Update()
            {
                if (element is StswLabelContainer container)
                {
                    element.ApplyTemplate();
                    container.Orientation = orientation;
                }
                else
                {
                    foreach (var headerContainer in StswFn.FindVisualChildren<StswLabelContainer>(element))
                    {
                        headerContainer.ApplyTemplate();
                        headerContainer.Orientation = orientation;
                    }
                }
            }

            if (element.IsLoaded)
            {
                Update();
            }
            else
            {
                void loadedHandler(object s, RoutedEventArgs args)
                {
                    Update();
                    element.Loaded -= loadedHandler;
                }
                element.Loaded += loadedHandler;
            }
        }
    }

    /// <summary>
    /// Gets or sets the width of the header.
    /// </summary>
    public static readonly DependencyProperty HeaderWidthProperty
        = DependencyProperty.RegisterAttached(
            nameof(HeaderWidthProperty)[..^8],
            typeof(double?),
            typeof(StswLabelContainer),
            new PropertyMetadata(default, OnHeaderWidthChanged)
        );
    public static void SetHeaderWidth(DependencyObject obj, double value) => obj.SetValue(HeaderWidthProperty, value);
    private static void OnHeaderWidthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is FrameworkElement element && e.NewValue is double width)
        {
            void Update()
            {
                if (element is StswLabelContainer)
                {
                    element.ApplyTemplate();
                    if (StswFn.FindVisualChild<StswHeader>(element) is StswHeader header)
                        header.Width = width;
                }
                else
                {
                    foreach (var container in StswFn.FindVisualChildren<StswLabelContainer>(element))
                    {
                        container.ApplyTemplate();
                        if (StswFn.FindVisualChild<StswHeader>(container) is StswHeader header)
                            header.Width = width;
                    }
                }
            }

            if (element.IsLoaded)
            {
                Update();
            }
            else
            {
                void loadedHandler(object s, RoutedEventArgs args)
                {
                    Update();
                    element.Loaded -= loadedHandler;
                }
                element.Loaded += loadedHandler;
            }
        }
    }
    #endregion
}
