using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A custom MarkupExtension that allows creating a list of values from a comma-separated string for XAML.
/// </summary>
/// <remarks>
/// This extension can be used in XAML to create lists of a specific type by providing a comma-separated string.
/// </remarks>
/// <param name="values">The comma-separated string representing the list of values.</param>
/// <exception cref="ArgumentNullException">Thrown when the values parameter is null.</exception>
public class StswMakeListExtension(string values) : MarkupExtension
{
    private readonly string _values = values ?? throw new ArgumentNullException(nameof(values));

    /// <summary>
    /// Provides the value for the XAML markup extension.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>A newly created instance of the specified list type with the provided values.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the target property cannot be determined, the target type is not a list,
    /// or the values cannot be converted to the list type.
    /// </exception>
    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget targetProvider)
            throw new InvalidOperationException("Cannot obtain target object information.");

        var targetType = targetProvider.TargetProperty?.GetType() ?? throw new InvalidOperationException("Cannot determine target type.");

        if (targetType == typeof(DependencyProperty))
            targetType = ((DependencyProperty)targetProvider.TargetProperty).PropertyType;

        if (!targetType.IsListType(out var listType) || listType == null)
            throw new InvalidOperationException("The target property must be of a list type.");

        if (string.IsNullOrEmpty(_values))
            throw new InvalidOperationException("The Values property must be set.");

        var converter = TypeDescriptor.GetConverter(listType);
        if (converter == null || !converter.CanConvertFrom(typeof(string)))
            throw new InvalidOperationException($"Cannot convert '{listType.Name}' from string.");

        var result = (IList?)Activator.CreateInstance(targetType);
        foreach (var valueString in _values.Split(',', StringSplitOptions.TrimEntries))
            result?.Add(converter.ConvertFromString(valueString));

        return result;
    }
}
