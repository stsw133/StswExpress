using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows;
using System.Threading;

namespace StswExpress;
/// <summary>
/// Provides a way to dynamically bind to resources in XAML with support for value conversion and formatting.
/// </summary>
public class StswDynamicResourceExtension(object resourceKey) : MarkupExtension
{
    private StswBindingProxy? _bindingProxy;
    private StswBindingTrigger? _bindingTrigger;

    /// <summary>
    /// Gets or sets the converter used to modify the data as it is passed between the source and target.
    /// </summary>
    public IValueConverter? Converter { get; set; }

    /// <summary>
    /// Gets or sets the culture to use for the converter.
    /// </summary>
    public CultureInfo? ConverterCulture { get; set; }

    /// <summary>
    /// Gets or sets the parameter to pass to the converter.
    /// </summary>
    public object? ConverterParameter { get; set; }

    /// <summary>
    /// Gets or sets the key of the resource to bind to.
    /// </summary>
    public object? ResourceKey { get; set; } = resourceKey ?? throw new ArgumentNullException(nameof(resourceKey));

    /// <summary>
    /// Gets or sets the string format to use when converting the resource value to a string.
    /// </summary>
    public string? StringFormat { get; set; }

    /// <summary>
    /// Gets or sets the value to use when the resource value is null.
    /// </summary>
    public object? TargetNullValue { get; set; }

    /// <summary>
    /// Provides the value for the target property.
    /// </summary>
    /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
    /// <returns>The object to set on the target property.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var dynamicResource = new DynamicResourceExtension(ResourceKey);
        _bindingProxy = new StswBindingProxy() { Proxy = dynamicResource.ProvideValue(null) };

        var dynamicResourceBinding = new Binding()
        {
            Source = _bindingProxy,
            Path = new PropertyPath(StswBindingProxy.ProxyProperty),
            Mode = BindingMode.OneWay
        };

        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget { TargetObject: DependencyObject dependencyObject })
        {
            dynamicResourceBinding.Converter = Converter;
            dynamicResourceBinding.ConverterParameter = ConverterParameter;
            dynamicResourceBinding.ConverterCulture = ConverterCulture;
            dynamicResourceBinding.StringFormat = StringFormat;
            dynamicResourceBinding.TargetNullValue = TargetNullValue;

            if (dependencyObject is FrameworkElement targetFrameworkElement)
                targetFrameworkElement.Resources[_bindingProxy] = _bindingProxy;

            return dynamicResourceBinding.ProvideValue(serviceProvider);
        }

        var findTargetBinding = new Binding()
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.Self)
        };

        _bindingTrigger = new StswBindingTrigger();

        var wrapperBinding = new MultiBinding()
        {
            Bindings = {
                dynamicResourceBinding,
                findTargetBinding,
                _bindingTrigger.Binding
            },
            Converter = new StswInlineMultiConverter(WrapperConvert)
        };

        return wrapperBinding.ProvideValue(serviceProvider);
    }

    /// <summary>
    /// Converts the values from multiple bindings to a single value.
    /// </summary>
    /// <param name="values">The array of values produced by the source bindings in the <see cref="MultiBinding"/>.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>The converted value.</returns>
    private object WrapperConvert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var dynamicResourceBindingResult = values[0];
        var bindingTargetObject = values[1];

        if (Converter != null)
            dynamicResourceBindingResult = Converter.Convert(dynamicResourceBindingResult, targetType, ConverterParameter, ConverterCulture);

        if (dynamicResourceBindingResult == null)
            dynamicResourceBindingResult = TargetNullValue;
        else if (targetType == typeof(string) && StringFormat != null)
            dynamicResourceBindingResult = string.Format(StringFormat, dynamicResourceBindingResult);

        if (bindingTargetObject is FrameworkElement targetFrameworkElement && !targetFrameworkElement.Resources.Contains(_bindingProxy))
        {
            targetFrameworkElement.Resources[_bindingProxy] = _bindingProxy;
            SynchronizationContext.Current?.Post((state) => _bindingTrigger?.Refresh(), null);
        }

        return dynamicResourceBindingResult!;
    }
}
