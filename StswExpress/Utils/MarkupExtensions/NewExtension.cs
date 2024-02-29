using System;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A XAML markup extension that allows for the inline instantiation of a specified class type.
/// </summary>
/// <remarks>
/// This extension simplifies the creation of instances of classes within XAML, providing a concise one-line syntax.
/// </remarks>
public class NewExtension : MarkupExtension
{
    private readonly Type _type;

    /// <summary>
    /// Initializes a new instance of the <see cref="NewExtension"/> class.
    /// </summary>
    /// <param name="type">The type of the class to instantiate.</param>
    public NewExtension(Type type) => _type = type;

    /// <summary>
    /// Provides the value for the XAML markup extension.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The newly created instance of the specified class type.</returns>
    public override object? ProvideValue(IServiceProvider serviceProvider) => Activator.CreateInstance(_type);
}
