using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A XAML markup extension that allows using bindable parameters in multi-bindings.
/// </summary>
/// <remarks>
/// This extension enables multi-bindings with a dynamically bound converter parameter, 
/// making it useful for complex data binding scenarios in XAML.
/// </remarks>
[ContentProperty(nameof(Binding))]
[StswInfo("0.9.0")]
public class StswBindableParameterExtension : MarkupExtension
{
    /// <summary>
    /// Gets or sets the primary binding that provides the main value.
    /// </summary>
    public Binding? Binding { get; set; }

    /// <summary>
    /// Gets or sets the binding mode for the main binding.
    /// </summary>
    public BindingMode Mode { get; set; }

    /// <summary>
    /// Gets or sets the value converter that will be applied to the binding.
    /// </summary>
    public IValueConverter? Converter { get; set; }

    /// <summary>
    /// Gets or sets the binding that provides the parameter for the value converter.
    /// </summary>
    public Binding? ConverterParameter { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StswBindableParameterExtension"/> class.
    /// </summary>
    public StswBindableParameterExtension()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StswBindableParameterExtension"/> class with the specified binding path.
    /// </summary>
    /// <param name="path">The path to the property to bind to.</param>
    public StswBindableParameterExtension(string path)
    {
        Binding = new Binding(path);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StswBindableParameterExtension"/> class with the specified binding.
    /// </summary>
    /// <param name="binding">The binding instance to use.</param>
    public StswBindableParameterExtension(Binding binding)
    {
        Binding = binding;
    }

    /// <inheritdoc/>
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
    /// A helper class that adapts a single value converter to work in a multi-binding scenario.
    /// </summary>
    [ContentProperty(nameof(Converter))]
    private class MultiValueConverterAdapter : IMultiValueConverter
    {
        /// <summary>
        /// Gets or sets the single value converter to be adapted for multi-binding.
        /// </summary>
        public IValueConverter? Converter { get; set; }

        private object? _lastParameter;

        /// <summary>
        /// Converts multiple source values into a single value for the binding target.
        /// </summary>
        /// <param name="values">The values provided by the source bindings.</param>
        /// <param name="targetType">The expected type of the target property.</param>
        /// <param name="parameter">The converter parameter (unused in this implementation).</param>
        /// <param name="culture">The culture information for the conversion.</param>
        /// <returns>The converted value.</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (Converter == null)
                return values[0];

            if (values.Length > 1)
                _lastParameter = values[1];

            return Converter.Convert(values[0], targetType, _lastParameter, culture);
        }

        /// <summary>
        /// Converts a value from the target back to multiple source values.
        /// </summary>
        /// <param name="value">The value to be converted back.</param>
        /// <param name="targetTypes">The expected types of the source values.</param>
        /// <param name="parameter">The converter parameter (unused in this implementation).</param>
        /// <param name="culture">The culture information for the conversion.</param>
        /// <returns>An array of converted values.</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if (Converter == null)
                return [value];

            return [Converter.ConvertBack(value, targetTypes[0], _lastParameter, culture)];
        }
    }
}

/* usage:

<TextBlock Text="{Binding Value, Converter={StaticResource ExampleConverter}, ConverterParameter={se:StswBindableParameter SomeOtherValue}}"/>

<TextBlock>
    <TextBlock.Text>
        <MultiBinding Converter="{StaticResource ExampleMultiConverter}">
            <Binding Path="MainValue"/>
            <se:StswBindableParameter Binding="{Binding AdditionalValue}"/>
        </MultiBinding>
    </TextBlock.Text>
</TextBlock>

<TextBlock Text="{Binding Value, Converter={StaticResource ExampleConverter}, ConverterParameter={se:StswBindableParameter FormatString}}"/>

*/
