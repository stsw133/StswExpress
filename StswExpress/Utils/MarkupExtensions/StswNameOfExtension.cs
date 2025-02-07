using System;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// A XAML markup extension that retrieves the name of a specified property or field as a string.
/// </summary>
/// <remarks>
/// This extension is useful for debugging, reflection, and binding scenarios where the name of a property
/// or field needs to be obtained dynamically in XAML.
/// </remarks>
/// <param name="member">The name of the property or field.</param>
public class StswNameOfExtension(string? member) : MarkupExtension
{
    /// <summary>
    /// Gets or sets the type that contains the member.
    /// </summary>
    public Type? Type { get; set; }

    /// <summary>
    /// Gets or sets the name of the property or field.
    /// </summary>
    public string? Member { get; set; } = member;

    /// <summary>
    /// Provides the name of the specified property or field as a string.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The name of the specified property or field as a string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the type is not specified, the member name is empty or contains '.', 
    /// or no property or field is found with the specified name in the type.
    /// </exception>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        if (Type == null || string.IsNullOrWhiteSpace(Member) || Member.Contains('.'))
            throw new ArgumentException("Syntax for StswNameOfExtension is Type={x:Type [ClassName]} Member=[PropertyName]");

        if (!Type.GetRuntimeProperties().Any(pi => pi.Name == Member) &&
            !Type.GetRuntimeFields().Any(fi => fi.Name == Member))
        {
            throw new ArgumentException($"No property or field found for '{Member}' in type '{Type.FullName}'");
        }

        return Member;
    }
}

/* usage:

<TextBlock Text="{se:StswNameOf Type={x:Type local:MyClass}, Member=MyProperty}"/>

<TextBlock Text="{se:StswNameOf Type={x:Type local:Constants}, Member=StaticField}"/>

<TextBlock Text="{Binding Path={se:StswNameOf Type={x:Type local:UserModel}, Member=UserName}}"/>

*/
