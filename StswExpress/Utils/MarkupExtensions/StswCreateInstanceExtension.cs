using System;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A XAML markup extension that allows for the inline instantiation of a specified class type.
/// </summary>
/// <remarks>
/// This extension simplifies the creation of instances of classes within XAML, providing a concise one-line syntax.
/// </remarks>
[Obsolete("This is not working and I don't know why yet. Please do not use this.", true)]
public class StswCreateInstanceExtension(Type type, params object?[]? args) : MarkupExtension
{
    public Type Type { get; set; } = type ?? throw new ArgumentNullException(nameof(type));
    public object?[]? Args { get; set; } = args;

    /// <summary>
    /// Provides the value for the XAML markup extension.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The newly created instance of the specified class type.</returns>
    public override object? ProvideValue(IServiceProvider serviceProvider) => Activator.CreateInstance(Type, Args);
}
