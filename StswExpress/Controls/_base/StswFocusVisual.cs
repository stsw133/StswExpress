using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace StswExpress;

/// <summary>
/// Provides a custom focus visual style for WPF controls, enhancing the default focus rectangle with custom properties.
/// </summary>
public static class StswFocusVisual
{
    /// <summary>
    /// Enables or disables the automatic assignment of a custom <see cref="Control.FocusVisualStyle"/> to the control.
    /// </summary>
    public static readonly DependencyProperty AssignProperty
        = DependencyProperty.RegisterAttached(
            nameof(AssignProperty)[..^8],
            typeof(bool),
            typeof(StswFocusVisual),
            new PropertyMetadata(false, OnAssignChanged)
        );
    public static void SetAssign(DependencyObject element, bool value) => element.SetValue(AssignProperty, value);
    public static bool GetAssign(DependencyObject element) => (bool)element.GetValue(AssignProperty);
    private static void OnAssignChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not Control control)
            return;

        if ((bool)e.NewValue)
            control.FocusVisualStyle = CreateFocusVisualStyle(control);
        else
            control.ClearValue(FrameworkElement.FocusVisualStyleProperty);
    }

    /// <summary>
    /// Creates a custom <see cref="Style"/> for <see cref="Control.FocusVisualStyle"/>, dynamically generating a visual template
    /// with a <see cref="Rectangle"/> that reflects the control's visual properties such as <see cref="Control.BorderThickness"/>
    /// and (optionally) <c>CornerRadius</c> if present.
    /// </summary>
    /// <param name="owner">The control for which the focus visual style is being created. Used as binding source for properties.</param>
    /// <returns>A <see cref="Style"/> instance configured to be used as <see cref="Control.FocusVisualStyle"/>.</returns>
    public static Style CreateFocusVisualStyle(Control owner)
    {
        var style = new Style(typeof(Control));

        var template = new ControlTemplate();
        var rectangleFactory = new FrameworkElementFactory(typeof(Rectangle));

        rectangleFactory.SetValue(Shape.StrokeProperty, Application.Current.Resources["StswFocusVisual.Static.Border"]);
        rectangleFactory.SetValue(Shape.StrokeDashArrayProperty, new DoubleCollection { 2, 1 });

        rectangleFactory.SetBinding(Shape.StrokeThicknessProperty, new Binding(nameof(Control.BorderThickness))
        {
            Source = owner,
            Converter = StswSquashStructToDoubleConverter.Instance
        });
        if (owner is IStswCornerControl)
        {
            var cornerRadiusBinding = new Binding(nameof(IStswCornerControl.CornerRadius))
            {
                Source = owner,
                Converter = StswSquashStructToDoubleConverter.Instance
            };
            rectangleFactory.SetBinding(Rectangle.RadiusXProperty, cornerRadiusBinding);
            rectangleFactory.SetBinding(Rectangle.RadiusYProperty, cornerRadiusBinding);
        }

        template.VisualTree = rectangleFactory;
        style.Setters.Add(new Setter(Control.TemplateProperty, template));

        return style;
    }

    /// <summary>
    /// Converts a <see cref="CornerRadius"/> or <see cref="Thickness"/> structure into a single <see cref="double"/> value by averaging all four component values.
    /// </summary>
    private class StswSquashStructToDoubleConverter : MarkupExtension, IValueConverter
    {
        public static StswSquashStructToDoubleConverter Instance => instance ??= new StswSquashStructToDoubleConverter();
        private static StswSquashStructToDoubleConverter? instance;

        public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                CornerRadius cr => Math.Max((cr.TopLeft + cr.TopRight + cr.BottomLeft + cr.BottomRight) / 4.0, 1.5),
                Thickness t => Math.Max((t.Left + t.Top + t.Right + t.Bottom) / 4.0, 1.5),
                _ => (object)1.0,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
