using System;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A XAML markup extension that retrieves the name of a specified property or field as a string.
/// </summary>
/// <remarks>
/// This extension allows for obtaining the name of a property or field in XAML, typically used for debugging purposes or when working with reflection.
/// </remarks>
public class StswNameOfExtension(string? member) : MarkupExtension
{
    public Type? Type { get; set; }
    public string? Member { get; set; } = member;

    /// <summary>
    /// Provides the name of the specified property or field as a string.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The name of the specified property or field as a string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the type is not specified, the member name is empty or contains '.', or no property or field is found with the specified name in the type.</exception>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));

        if (Type == null || string.IsNullOrEmpty(Member) || Member.Contains('.'))
            throw new ArgumentException("Syntax for x:NameOf is Type={x:Type [className]} Member=[propertyName]");

        var pinfo = Type.GetRuntimeProperties().FirstOrDefault(pi => pi.Name == Member);
        var finfo = Type.GetRuntimeFields().FirstOrDefault(fi => fi.Name == Member);
        if (pinfo == null && finfo == null)
            throw new ArgumentException($"No property or field found for {Member} in {Type}");

        return Member;
    }
}
