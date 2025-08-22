using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows;
using System.Threading;

namespace StswExpress;

/// <summary>
/// A XAML markup extension that dynamically binds to a resource with support for value conversion and formatting.
/// </summary>
/// <remarks>
/// This extension allows binding to dynamic resources in XAML, enabling automatic updates when the resource changes.
/// It supports converters, formatting, and fallback values.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;TextBlock Text="Dynamic Color" Foreground="{Binding Source={se:StswDynamicResource MyBrushResource}, Converter={StaticResource BrushToColorConverter}}"/&gt;
/// &lt;Border BorderBrush="{se:StswDynamicResource MyBorderBrush}" BorderThickness="2"/&gt;
/// &lt;TextBlock Text="{se:StswDynamicResource MyNumberResource, StringFormat='Value: {0:F2}'}"/&gt;
/// &lt;TextBlock Text="{se:StswDynamicResource NonExistingResource, TargetNullValue='Default Text'}" /&gt;
/// </code>
/// </example>
[StswInfo("0.2.0")]
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
    public object ResourceKey { get; } = resourceKey ?? throw new ArgumentNullException(nameof(resourceKey));

    /// <summary>
    /// Gets or sets the string format to use when converting the resource value to a string.
    /// </summary>
    public string? StringFormat { get; set; }

    /// <summary>
    /// Gets or sets the value to use when the resource value is null.
    /// </summary>
    public object? TargetNullValue { get; set; }

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var dynamicResource = new DynamicResourceExtension(ResourceKey);
        _bindingProxy = new StswBindingProxy { Proxy = dynamicResource.ProvideValue(null) };

        var dynamicResourceBinding = new Binding()
        {
            Source = _bindingProxy,
            Path = new PropertyPath(StswBindingProxy.ProxyProperty),
            Mode = BindingMode.OneWay,
            Converter = Converter,
            ConverterParameter = ConverterParameter,
            ConverterCulture = ConverterCulture,
            StringFormat = StringFormat,
            TargetNullValue = TargetNullValue
        };

        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget { TargetObject: DependencyObject dependencyObject })
        {
            if (dependencyObject is FrameworkElement targetFrameworkElement)
                targetFrameworkElement.Resources[_bindingProxy] = _bindingProxy;

            return dynamicResourceBinding.ProvideValue(serviceProvider);
        }

        var findTargetBinding = new Binding { RelativeSource = new RelativeSource(RelativeSourceMode.Self) };
        _bindingTrigger = new StswBindingTrigger();

        var wrapperBinding = new MultiBinding()
        {
            Bindings = { dynamicResourceBinding, findTargetBinding, _bindingTrigger.Binding },
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
