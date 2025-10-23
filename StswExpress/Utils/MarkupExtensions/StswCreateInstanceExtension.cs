using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// A XAML markup extension that creates an instance of a specified class type with optional constructor parameters.
/// </summary>
/// <remarks>
/// This extension enables inline instantiation of objects in XAML, allowing the use of constructor parameters.
/// It supports automatic type conversion and correctly handles string arguments containing commas.
/// </remarks>
/// <exception cref="ArgumentNullException">Thrown when the type parameter is null.</exception>
/// <exception cref="MissingMethodException">Thrown if no suitable constructor is found.</exception>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;TextBlock Text="{se:StswCreateInstance local:MyClass}"/&gt;
/// &lt;TextBlock Text="{se:StswCreateInstance local:MyClass, '7, false, \"Test Name\", 2023-12-31'}"/&gt;
/// &lt;TextBlock Text="{Binding Value, Converter={StaticResource ExampleConverter}, ConverterParameter={se:StswCreateInstance local:MyClass, '99'} }"/&gt;
/// &lt;ListBox ItemsSource="{se:StswCreateInstance local:MyCollection, '\"Item 1\", \"Item 2\", \"Item 3\"'}"/&gt;
/// </code>
/// </example>
[StswPlannedChanges(StswPlannedChanges.Rework, "Refactor to improve argument parsing and type conversion.")]
internal partial class StswCreateInstanceExtension(Type type, string? args) : MarkupExtension
{
    [GeneratedRegex("\"(.*?)\"|([^,]+)")]
    private static partial Regex ArgsRegex();

    /// <summary>
    /// Gets or sets the type of the class to instantiate.
    /// </summary>
    public Type Type { get; set; } = type ?? throw new ArgumentNullException(nameof(type));

    /// <summary>
    /// Gets or sets the comma-separated string of arguments to pass to the constructor.
    /// </summary>
    public string Args { get; set; } = args ?? string.Empty;

    /// <inheritdoc/>
    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        var parsedArgs = ParseArguments(Args);
        return CreateInstanceWithBestConstructor(Type, parsedArgs);
    }

    /// <summary>
    /// Parses a comma-separated argument string into individual arguments, handling quoted values correctly.
    /// </summary>
    private static object?[] ParseArguments(string args)
    {
        if (string.IsNullOrWhiteSpace(args))
            return [];

        var matches = ArgsRegex().Matches(args)
                                 .Cast<Match>()
                                 .Select(m => m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value.Trim())
                                 .Select(ConvertArgument)
                                 .ToArray();

        return matches;
    }

    /// <summary>
    /// Attempts to find the best matching constructor and create an instance of the specified type.
    /// </summary>
    private static object CreateInstanceWithBestConstructor(Type type, object?[] args)
    {
        var constructors = type.GetConstructors();

        var bestConstructor = constructors
            .Select(ctor => new { Constructor = ctor, Parameters = ctor.GetParameters() })
            .Where(c => c.Parameters.Length == args.Length)
            .OrderByDescending(c => c.Parameters.Count(p => p.ParameterType.IsInstanceOfType(args[p.Position]) || args[p.Position] == null))
            .FirstOrDefault() ?? throw new MissingMethodException($"No suitable constructor found for {type.FullName} with {args.Length} parameters.");

        return bestConstructor.Constructor.Invoke(args);
    }

    /// <summary>
    /// Converts a string argument into an appropriate data type.
    /// </summary>
    private static object? ConvertArgument(string arg)
    {
        if (string.IsNullOrEmpty(arg)) return null;

        if (int.TryParse(arg, out int intValue))
            return intValue;

        if (double.TryParse(arg, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleValue))
            return doubleValue;

        if (bool.TryParse(arg, out bool boolValue))
            return boolValue;

        if (DateTime.TryParse(arg, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateValue))
            return dateValue;

        return arg;
    }
}
