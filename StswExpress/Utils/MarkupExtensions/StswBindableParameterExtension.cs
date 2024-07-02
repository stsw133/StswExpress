using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
[ContentProperty(nameof(Binding))]
public class StswBindableParameterExtension : MarkupExtension
{
    public Binding? Binding { get; set; }
    public BindingMode Mode { get; set; }
    public IValueConverter? Converter { get; set; }
    public Binding? ConverterParameter { get; set; }

    public StswBindableParameterExtension()
    {
    }
    public StswBindableParameterExtension(string path)
    {
        Binding = new Binding(path);
    }
    public StswBindableParameterExtension(Binding binding)
    {
        Binding = binding;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var multiBinding = new MultiBinding();
        (Binding ?? new Binding()).Mode = Mode;
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

    [ContentProperty(nameof(Converter))]
    private class MultiValueConverterAdapter : IMultiValueConverter
    {
        public IValueConverter? Converter { get; set; }

        private object? lastParameter;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (Converter == null)
                return values[0];

            if (values.Length > 1)
                lastParameter = values[1];

            return Converter.Convert(values[0], targetType, lastParameter, culture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if (Converter == null)
                return [value];

            return [Converter.ConvertBack(value, targetTypes[0], lastParameter, culture)];
        }
    }
}
