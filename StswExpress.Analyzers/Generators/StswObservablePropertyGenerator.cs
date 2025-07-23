using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace StswExpress.Analyzers;

/// <summary>
/// Generates properties for fields marked with the StswObservablePropertyAttribute.
/// </summary>
[Generator]
public class StswObservablePropertyGenerator : IIncrementalGenerator
{
    private const string AttributeFullName = "StswExpress.Commons.StswObservablePropertyAttribute";

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
                    var fieldName = item.Field.Name;
                    var propertyName = Helpers.ToPascalCase(item.Field.Name.TrimStart('_'));
                    var propertyType = item.Field.Type.ToDisplayString();

                    sb.AppendLine();
                    sb.Append(documentation);
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
}
