using System;
using System.Linq;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// A XAML markup extension that converts an enum type into a list of selection items.
/// </summary>
/// <remarks>
/// This extension allows binding an enumeration to a UI element, providing a list of selection items.
/// Each item includes a display name and the corresponding enum value.
/// </remarks>
public class StswEnumToListExtension : MarkupExtension
{
    private readonly Type _enumType;

    /// <summary>
    /// Initializes a new instance of the <see cref="StswEnumToListExtension"/> class.
    /// </summary>
    /// <param name="enumType">The enum type to convert to a list of selection items.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="enumType"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="enumType"/> is not an enum.</exception>
    public StswEnumToListExtension(Type enumType)
    {
        _enumType = enumType ?? throw new ArgumentNullException(nameof(enumType));

        if (!_enumType.IsEnum)
            throw new ArgumentException("Type must be an enum.", nameof(enumType));
    }

    /// <summary>
    /// Provides a list of selection items created from the enum type.
    /// </summary>
    /// <param name="serviceProvider">A service provider for the markup extension (typically not used).</param>
    /// <returns>A list of selection items representing the enum values.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return Enum.GetValues(_enumType)
                   .Cast<Enum>()
                   .Select(value => new StswSelectionItem
                   {
                       Display = value.GetDescription(),
                       Value = value
                   })
                   .ToList();
    }
}

/* usage:

<ComboBox ItemsSource="{se:StswEnumToList local:MyEnum}" DisplayMemberPath="Display" SelectedValuePath="Value"/>

<StackPanel>
    <ItemsControl ItemsSource="{se:StswEnumToList local:MyEnum}">
        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <RadioButton Content="{Binding Display}" IsChecked="{Binding Path=Value, Mode=TwoWay}"/>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</StackPanel>

*/
