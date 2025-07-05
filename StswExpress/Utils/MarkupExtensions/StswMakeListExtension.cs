using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A XAML markup extension that creates a list of values from a comma-separated string.
/// </summary>
/// <remarks>
/// This extension allows defining a list in XAML using a comma-separated string.
/// The resulting list type is determined by the target property type.
/// </remarks>
/// <param name="values">The comma-separated string representing the list of values.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="values"/> parameter is <see langword="null"/>.</exception>
[Stsw("0.8.0", Changes = StswPlannedChanges.Refactor)]
public class StswMakeListExtension(string values) : MarkupExtension
{
    private readonly string _values = values ?? throw new ArgumentNullException(nameof(values));

    /// <inheritdoc/>
    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget targetProvider)
            throw new InvalidOperationException("Cannot obtain target object information.");

        var targetProperty = targetProvider.TargetProperty as DependencyProperty;
        var targetType = targetProperty?.PropertyType ?? targetProvider.TargetProperty?.GetType()
                         ?? throw new InvalidOperationException("Cannot determine target type.");

        if (!targetType.IsListType(out var listType) || listType == null)
            throw new InvalidOperationException("The target property must be of a list type.");

        if (string.IsNullOrWhiteSpace(_values))
            throw new InvalidOperationException("The values parameter must not be empty.");

        var converter = TypeDescriptor.GetConverter(listType);
        if (converter == null || !converter.CanConvertFrom(typeof(string)))
            throw new InvalidOperationException($"Cannot convert values to '{listType.Name}' type.");

        var result = (IList?)Activator.CreateInstance(targetType);
        foreach (var valueString in _values.Split(',', StringSplitOptions.TrimEntries))
            result?.Add(converter.ConvertFromString(valueString));

        return result;
    }
}

/* usage:

<ListBox ItemsSource="{se:StswMakeList '1, 2, 3, 4, 5'}"/>

<ListBox ItemsSource="{se:StswMakeList 'Red, Green, Blue'}"/>

<ListBox ItemsSource="{se:StswMakeList '1.5, 2.75, 3.14'}"/>

*/
