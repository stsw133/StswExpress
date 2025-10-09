using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Globalization;
using System.Text;

namespace StswExpress.Analyzers;

/// <summary>
/// Generates properties for fields marked with the StswObservablePropertyAttribute.
/// </summary>
[Generator]
public class StswObservablePropertyGenerator : IIncrementalGenerator
{
    private const string AttributeFullName = "StswExpress.Commons.StswObservablePropertyAttribute";
    private static readonly SymbolDisplayFormat FullyQualified =
        SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Included);

    /// <summary>
    /// Initializes the generator by registering a syntax provider to collect declarations.
    /// </summary>
    /// <param name="context">The generator initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var fieldSymbols = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is FieldDeclarationSyntax f && f.AttributeLists.Count > 0,
                transform: static (ctx, _) => Helpers.GetFieldSymbol(ctx))
            .Where(static symbol => symbol is not null)
            .Collect();

        context.RegisterSourceOutput(fieldSymbols, (spc, symbols) =>
        {
            var grouped = symbols!
                .OfType<IFieldSymbol>()
                .Select(field => new
                {
                    Field = field,
                    Attribute = Helpers.GetAttribute(field, AttributeFullName)
                })
                .Where(t => t.Attribute is not null)
                .GroupBy(t => t.Field.ContainingType, SymbolEqualityComparer.Default);

            foreach (var group in grouped)
            {
                if (group.Key is not INamedTypeSymbol namedTypeSymbol)
                    continue;

                var classCtx = Helpers.GetClassContext(namedTypeSymbol);

                var sb = new StringBuilder();
                sb.AppendLine($"#nullable enable");
                sb.AppendLine($"namespace {classCtx.Namespace}");
                sb.AppendLine($"{{");
                sb.AppendLine($"    public partial class {classCtx.ClassName}");
                sb.AppendLine($"    {{");

                foreach (var item in group)
                {
                    var documentation = Helpers.FormatDocComment(item.Field);
                    var propertyAttributes = GetAttributesForProperty(item.Field);
                    var fieldName = item.Field.Name;
                    var propertyName = Helpers.ToPascalCase(item.Field.Name.TrimStart('_'));
                    var propertyType = item.Field.Type.ToDisplayString();

                    sb.AppendLine();
                    sb.Append(documentation);
                    foreach (var attrLine in propertyAttributes)
                        sb.AppendLine($"        {attrLine}");
                    sb.AppendLine($"        public {propertyType} {propertyName}");
                    sb.AppendLine($"        {{");
                    sb.AppendLine($"            get => {fieldName};");
                    sb.AppendLine($"            set");
                    sb.AppendLine($"            {{");
                    sb.AppendLine($"                var oldValue = {fieldName};");
                    sb.AppendLine($"                var cancel = false;");
                    sb.AppendLine($"                On{propertyName}Changing(oldValue, value, ref cancel);");
                    sb.AppendLine($"                if (cancel)");
                    sb.AppendLine($"                    return;");
                    sb.AppendLine($"                if (SetProperty(ref {fieldName}, value))");
                    sb.AppendLine($"                    On{propertyName}Changed(oldValue, value);");
                    sb.AppendLine($"            }}");
                    sb.AppendLine($"        }}");
                    sb.AppendLine($"        partial void On{propertyName}Changing({propertyType} oldValue, {propertyType} newValue, ref bool cancel);");
                    sb.AppendLine($"        partial void On{propertyName}Changed({propertyType} oldValue, {propertyType} newValue);");
                }

                sb.AppendLine($"    }}");
                sb.AppendLine($"}}");

                spc.AddSource($"{classCtx.ClassName}_StswObservableProperties.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
            }
        });
    }

    /// <summary>
    /// Gets the attributes to be applied to the generated property, filtering out the marker attribute
    /// </summary>
    /// <param name="field">The field symbol.</param>
    /// <returns>An enumerable of attribute strings to be applied to the property.</returns>
    private static IEnumerable<string> GetAttributesForProperty(IFieldSymbol field)
    {
        foreach (var attr in field.GetAttributes())
        {
            // skip the marker attribute itself
            if (SymbolEquals(attr.AttributeClass, AttributeFullName))
                continue;

            // copy only if AttributeUsage allows Property target (or no AttributeUsage -> treat as All)
            if (!IsValidOnProperty(attr.AttributeClass))
                continue;

            yield return RenderAttribute(attr);
        }
    }

    /// <summary>
    /// Compares a named type symbol to a full name string.
    /// </summary>
    /// <param name="type">The named type symbol to compare.</param>
    /// <param name="fullName">The full name to compare against.</param>
    /// <returns><see langword="true"/> if the symbol's full name matches the given string; otherwise, <see langword="false"/>.</returns>
    private static bool SymbolEquals(INamedTypeSymbol? type, string fullName) => type?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) == fullName;

    /// <summary>
    /// Checks if the given attribute type is valid on properties based on its AttributeUsage.
    /// </summary>
    /// <param name="attributeType">The attribute type symbol to check.</param>
    /// <returns><see langword="true"/> if the attribute can be applied to properties; otherwise, <see langword="false"/>.</returns>
    private static bool IsValidOnProperty(INamedTypeSymbol? attributeType)
    {
        if (attributeType is null)
            return false;

        // find [AttributeUsage(...)]
        var usage = attributeType
            .GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString(FullyQualified) == "global::System.AttributeUsageAttribute");

        if (usage is null)
            return true; // default is All

        var validOnArg = usage.ConstructorArguments.FirstOrDefault();
        if (validOnArg.Kind == TypedConstantKind.Enum && validOnArg.Value is not null)
        {
            // Try to resolve named enum constant if possible
            var targetsValue = Convert.ToInt64(validOnArg.Value, CultureInfo.InvariantCulture);
            // Property flag is 0x0100
            const long PropertyFlag = (long)AttributeTargets.Property;
            return (targetsValue & PropertyFlag) == PropertyFlag;
        }

        return true;
    }

    /// <summary>
    /// Renders an attribute to its C# representation, including its arguments.
    /// </summary>
    /// <param name="a">The attribute data to render.</param>
    /// <returns>The C# representation of the attribute.</returns>
    private static string RenderAttribute(AttributeData a)
    {
        var typeName = a.AttributeClass!.ToDisplayString(FullyQualified); // e.g. global::System.ObsoleteAttribute
        // Prefer short form without "Attribute" suffix when emitting inside brackets is optional, but
        // keeping the full name is the safest and unambiguous:
        var args = new List<string>();

        // positional args
        foreach (var ca in a.ConstructorArguments)
            args.Add(RenderTypedConstant(ca));

        // named args
        foreach (var namedArgument in a.NamedArguments)
            args.Add($"{namedArgument.Key} = {RenderTypedConstant(namedArgument.Value)}");

        var joined = string.Join(", ", args);
        return string.IsNullOrEmpty(joined)
            ? $"[{typeName}]"
            : $"[{typeName}({joined})]";
    }

    /// <summary>
    /// Renders a TypedConstant to its C# representation.
    /// </summary>
    /// <param name="c">The TypedConstant to render.</param>
    /// <returns>The C# representation of the TypedConstant.</returns>
    private static string RenderTypedConstant(TypedConstant c)
    {
        if (c.IsNull)
            return "null";

        switch (c.Kind)
        {
            case TypedConstantKind.Primitive:
                return RenderPrimitive(c.Type, c.Value);

            case TypedConstantKind.Enum:
                return RenderEnum(c);

            case TypedConstantKind.Type:
                return $"typeof({(c.Value is ITypeSymbol ts ? ts.ToDisplayString(FullyQualified) : "object")})";

            case TypedConstantKind.Array:
                return RenderArray(c);

            default:
                // Fallback to quoted ToString for safety
                return $"\"{EscapeString(ToCSharpStringSafe(c))}\"";
        }
    }

    /// <summary>
    /// Renders a primitive value to its C# representation.
    /// </summary>
    /// <param name="type">The type symbol of the primitive.</param>
    /// <param name="value">The primitive value.</param>
    /// <returns>The C# representation of the primitive value.</returns>
    private static string RenderPrimitive(ITypeSymbol? type, object? value)
    {
        if (type is null || value is null)
            return "null";

        return type.SpecialType switch
        {
            SpecialType.System_String => $"\"{EscapeString((string)value)}\"",
            SpecialType.System_Char => $"'{EscapeChar((char)value)}'",
            SpecialType.System_Boolean => ((bool)value) ? "true" : "false",
            SpecialType.System_Single => ((float)value).ToString("R", CultureInfo.InvariantCulture) + "F",
            SpecialType.System_Double => ((double)value).ToString("R", CultureInfo.InvariantCulture),
            SpecialType.System_Decimal => ((decimal)value).ToString(CultureInfo.InvariantCulture) + "m",
            SpecialType.System_Byte or
            SpecialType.System_SByte or
            SpecialType.System_Int16 or
            SpecialType.System_UInt16 or
            SpecialType.System_Int32 or
            SpecialType.System_UInt32 or
            SpecialType.System_Int64 or
            SpecialType.System_UInt64 => Convert.ToString(value, CultureInfo.InvariantCulture)!,
            _ => value.ToString() ?? "null"
        };
    }

    /// <summary>
    /// Renders an enum TypedConstant to its C# representation.
    /// </summary>
    /// <param name="c">The enum TypedConstant.</param>
    /// <returns>The C# representation of the enum.</returns>
    private static string RenderEnum(TypedConstant c)
    {
        var enumType = c.Type!;
        var fqEnum = enumType.ToDisplayString(FullyQualified);

        // Try to find the enum member name for the numeric value (also works for combined flags)
        var underlying = Convert.ToInt64(c.Value!, CultureInfo.InvariantCulture);
        // Try exact match
        var fields = enumType.GetMembers().OfType<IFieldSymbol>().Where(f => f.HasConstantValue).ToArray();
        var exact = fields.FirstOrDefault(f => Convert.ToInt64(f.ConstantValue!, CultureInfo.InvariantCulture) == underlying);
        if (exact is not null)
            return $"{fqEnum}.{exact.Name}";

        // For flagged combos, compose by names when possible
        var flagged = fields
            .OrderByDescending(f => Convert.ToInt64(f.ConstantValue!, CultureInfo.InvariantCulture))
            .Where(f => (underlying & Convert.ToInt64(f.ConstantValue!, CultureInfo.InvariantCulture)) != 0)
            .ToList();

        if (flagged.Count > 0 && flagged.Sum(f => Convert.ToInt64(f.ConstantValue!, CultureInfo.InvariantCulture)) == underlying)
            return string.Join(" | ", flagged.Select(f => $"{fqEnum}.{f.Name}"));

        // Fallback to cast with numeric value
        return $"({fqEnum}){underlying}";
    }

    /// <summary>
    /// Renders an array TypedConstant to its C# representation.
    /// </summary>
    /// <param name="c">The array TypedConstant.</param>
    /// <returns>The C# representation of the array.</returns>
    private static string RenderArray(TypedConstant c)
    {
        if (c.Values.IsDefaultOrEmpty)
        {
            // new T[0]
            if (c.Type is IArrayTypeSymbol ats)
                return $"new {ats.ElementType.ToDisplayString(FullyQualified)}[0]";
            return "new object[0]";
        }

        if (c.Type is IArrayTypeSymbol arrType)
        {
            var elements = string.Join(", ", c.Values.Select(RenderTypedConstant));
            return $"new {arrType.ElementType.ToDisplayString(FullyQualified)}[] {{ {elements} }}";
        }

        // generic fallback
        var elems = string.Join(", ", c.Values.Select(RenderTypedConstant));
        return $"new[] {{ {elems} }}";
    }

    /// <summary>
    /// Escapes a string for use in a C# string literal.
    /// </summary>
    /// <param name="s">The string to escape.</param>
    /// <returns>The escaped string.</returns>
    private static string EscapeString(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");

    /// <summary>
    /// Escapes a character for use in a C# character literal.
    /// </summary>
    /// <param name="c">The character to escape.</param>
    /// <returns>The escaped character as a string.</returns>
    private static string EscapeChar(char c) =>
        c switch
        {
            '\\' => "\\\\",
            '\'' => "\\\'",
            '\r' => "\\r",
            '\n' => "\\n",
            '\t' => "\\t",
            _ => c.ToString()
        };

    /// <summary>
    /// Safely converts a TypedConstant to its C# string representation, returning an empty string if the value is null.
    /// </summary>
    /// <param name="c">The TypedConstant to convert.</param>
    /// <returns>The C# string representation of the TypedConstant, or an empty string if the value is null.</returns>
    private static string ToCSharpStringSafe(TypedConstant c) => c.Value?.ToString() ?? string.Empty;
}
