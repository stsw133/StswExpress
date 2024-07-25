using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A XAML markup extension that supports bindable parameters in multi-bindings.
/// </summary>
/// <remarks>
/// This extension allows for creating multi-bindings with a bindable converter parameter, 
/// which can be useful for complex binding scenarios in XAML.
/// </remarks>
[ContentProperty(nameof(Binding))]
public class StswBindableParameterExtension : MarkupExtension
{
    /// <summary>
    /// Gets or sets the main binding.
    /// </summary>
    public Binding? Binding { get; set; }

    /// <summary>
    /// Gets or sets the binding mode.
    /// </summary>
    public BindingMode Mode { get; set; }

    /// <summary>
    /// Gets or sets the value converter.
    /// </summary>
    public IValueConverter? Converter { get; set; }

    /// <summary>
    /// Gets or sets the converter parameter binding.
    /// </summary>
    public Binding? ConverterParameter { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StswBindableParameterExtension"/> class.
    /// </summary>
    public StswBindableParameterExtension()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StswBindableParameterExtension"/> class with the specified path.
    /// </summary>
    /// <param name="path">The binding path.</param>
    public StswBindableParameterExtension(string path)
    {
        Binding = new Binding(path);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StswBindableParameterExtension"/> class with the specified binding.
    /// </summary>
    /// <param name="binding">The binding instance.</param>
    public StswBindableParameterExtension(Binding binding)
    {
        Binding = binding;
    }

    /// <summary>
    /// Provides the value for the XAML markup extension.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>A MultiBinding instance with the configured bindings and converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var multiBinding = new MultiBinding();
        Binding ??= new();
        Binding.Mode = Mode;
        multiBinding.Bindings.Add(Binding);

        if (ConverterParameter != null)
        {
            ConverterParameter.Mode = BindingMode.OneWay;
            multiBinding.Bindings.Add(ConverterParameter);
        }

        var adapter = new MultiValueConverterAdapter
        {
            Converter = Converter
        };
        multiBinding.Converter = adapter;

        return multiBinding.ProvideValue(serviceProvider);
    }

    /// <summary>
    /// A class that adapts a single value converter to a multi-value converter.
    /// </summary>
    [ContentProperty(nameof(Converter))]
    private class MultiValueConverterAdapter : IMultiValueConverter
    {
        /// <summary>
        /// Gets or sets the single value converter.
        /// </summary>
        public IValueConverter? Converter { get; set; }

        private object? _lastParameter;

        /// <summary>
        /// Converts source values to a value for the binding target.
        /// </summary>
        /// <param name="values">The array of values that the source bindings in the MultiBinding produces.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value.</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (Converter == null)
                return values[0];

            if (values.Length > 1)
                _lastParameter = values[1];

            return Converter.Convert(values[0], targetType, _lastParameter, culture);
        }

        /// <summary>
        /// Converts a binding target value to the source binding values.
        /// </summary>
        /// <param name="value">The value that the binding target produces.</param>
        /// <param name="targetTypes">The array of types to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>An array of values that have been converted from the target value back to the source values.</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if (Converter == null)
                return [value];

            return [Converter.ConvertBack(value, targetTypes[0], _lastParameter, culture)];
        }
    }
}
