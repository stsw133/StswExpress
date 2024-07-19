using System;
using System.Globalization;
using System.Linq;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A XAML markup extension that allows for the inline instantiation of a specified class type.
/// </summary>
/// <remarks>
/// This extension simplifies the creation of instances of classes within XAML, providing a concise one-line syntax.
/// </remarks>
/// <param name="type">The type of the class to instantiate.</param>
/// <param name="args">A comma-separated string of arguments to pass to the constructor of the class.</param>
/// <exception cref="ArgumentNullException">Thrown when the type parameter is null.</exception>
public class StswCreateInstanceExtension(Type type, string? args) : MarkupExtension
{
    /// <summary>
    /// Gets or sets the type of the class to instantiate.
    /// </summary>
    public Type Type { get; set; } = type ?? throw new ArgumentNullException(nameof(type));

    /// <summary>
    /// Gets or sets the comma-separated string of arguments to pass to the constructor of the class.
    /// </summary>
    public string Args { get; set; } = args ?? string.Empty;

    /// <summary>
    /// Provides the value for the XAML markup extension.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The newly created instance of the specified class type.</returns>
    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        var args = Args.Split(',')
                       .Select(arg => ConvertArgument(arg.Trim()))
                       .ToArray();
        return Activator.CreateInstance(Type, args);
    }

    /// <summary>
    /// Converts a string argument to the appropriate type.
    /// </summary>
    /// <param name="arg">The string argument to convert.</param>
    /// <returns>The converted argument.</returns>
    private static object? ConvertArgument(string arg)
    {
        if (int.TryParse(arg, out int intValue))
            return intValue;

        if (double.TryParse(arg, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleValue))
            return doubleValue;

        if (bool.TryParse(arg, out bool boolValue))
            return boolValue;

        return arg;
    }
}
