using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
public class StswMakeListExtension : MarkupExtension
{
    private readonly string _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="StswMakeListExtension"/> class.
    /// </summary>
    /// <param name="type">The type of the class to instantiate.</param>
    public StswMakeListExtension(string values) => _values = values ?? throw new ArgumentNullException(nameof(values));

    /// <summary>
    /// Provides the value for the XAML markup extension.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The newly created instance of the specified class type.</returns>
    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget targetProvider)
            throw new InvalidOperationException("Cannot obtain target object information.");

        var targetType = (targetProvider.TargetProperty?.GetType()) ?? throw new InvalidOperationException("Cannot determine target type.");
        if (targetType == typeof(DependencyProperty))
            targetType = ((DependencyProperty)targetProvider.TargetProperty).PropertyType;

        if (!targetType.IsListType(out var listType))
            throw new InvalidOperationException("The Values property must be list type.");

        if (string.IsNullOrEmpty(_values))
            throw new InvalidOperationException("The Values property must be set.");

        var converter = TypeDescriptor.GetConverter(listType);
        if (converter == null || !converter.CanConvertFrom(typeof(string)))
            throw new InvalidOperationException($"Cannot convert '{listType.Name}' from string.");

        var result = (IList?)Activator.CreateInstance(targetType); //new List<object?>();
        foreach (string valueString in _values.Split(',', StringSplitOptions.TrimEntries))
            result?.Add(converter.ConvertFromString(valueString));

        return result;
    }
}
