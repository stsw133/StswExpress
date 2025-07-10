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
[StswInfo("0.6.0", Changes = StswPlannedChanges.Refactor)]
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

    /// <inheritdoc/>
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
