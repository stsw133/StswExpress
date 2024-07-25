using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A markup extension that converts an enum type into a list of selection items.
/// </summary>
/// <remarks>
/// This extension can be used in XAML to bind an enumeration to a UI element, providing a list of selection items
/// where each item contains the display name and value of the enumeration.
/// </remarks>
public class StswEnumToListExtension : MarkupExtension
{
    private readonly Type _type;

    /// <summary>
    /// Initializes a new instance of the <see cref="StswEnumToListExtension"/> class.
    /// </summary>
    /// <param name="type">The enum type to convert to a list of selection items.</param>
    /// <exception cref="ArgumentNullException">Thrown when the type parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the type parameter is not an enum.</exception>
    public StswEnumToListExtension(Type type)
    {
        _type = type ?? throw new ArgumentNullException(nameof(type));

        if (!_type.IsEnum)
            throw new ArgumentException("Type must be an enum.", nameof(type));
    }

    /// <summary>
    /// Provides the value for the XAML markup extension.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>A list of selection items created from the enum type.</returns>
    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        return Enum.GetNames(_type)
                   .Select(name => new StswSelectionItem
                   {
                       Display = _type.GetField(name)?
                                      .GetCustomAttributes(typeof(DescriptionAttribute), false)
                                      .Cast<DescriptionAttribute>()
                                      .FirstOrDefault()?.Description ?? name,
                       Value = Enum.Parse(_type, name)
                   })
                   .ToList();
    }
}
