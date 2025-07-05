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
    private const string InvokingAttribute = "StswExpress.Commons.StswObservablePropertyAttribute";

    /// <summary>
    /// Initializes the generator by registering a syntax provider to collect declarations.
    /// </summary>
    /// <param name="context">The generator initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var symbolDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is FieldDeclarationSyntax f && f.AttributeLists.Count > 0,
                transform: static (ctx, _) => Helpers.GetFieldSymbol(ctx))
            .Where(static symbol => symbol is not null)
            .Collect();

        context.RegisterSourceOutput(symbolDeclarations, (spc, symbols) =>
        {
            var grouped = symbols!
                .OfType<IFieldSymbol>()
                .Select(field =>
                    Helpers.TryGetInvokingAttributeData(field, InvokingAttribute, out var attrData)
                        ? (Field: field, AttrData: attrData)
                        : (Field: null, AttrData: null))
                .Where(t => t.Field is not null && t.AttrData is not null)
                .GroupBy(t => t.Field!.ContainingType, SymbolEqualityComparer.Default);

            foreach (var group in grouped)
            {
                var classSymbol = group.Key!;
                var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
                var className = classSymbol.Name;

                var sb = new StringBuilder();
                sb.AppendLine($"#nullable enable");
                sb.AppendLine($"namespace {namespaceName}");
                sb.AppendLine($"{{");
                sb.AppendLine($"    public partial class {className}");
                sb.AppendLine($"    {{");

                foreach (var (field, attrData) in group)
                {
                    var fieldName = field!.Name;
                    var propertyName = Helpers.ToPascalCase(field.Name.TrimStart('_'));
                    var propertyType = field.Type.ToDisplayString();

                    var docComment = field.GetDocumentationCommentXml();
                    var documentation = string.IsNullOrWhiteSpace(docComment)
                        ? ""
                        : $"        /// {docComment!.Trim().Replace("\n", "\n        /// ")}\n";

                    sb.AppendLine();
                    sb.Append(documentation);
                    sb.AppendLine($"        public {propertyType} {propertyName}");
                    sb.AppendLine($"        {{");
                    sb.AppendLine($"            get => {fieldName};");
                    sb.AppendLine($"            set");
                    sb.AppendLine($"            {{");
                    sb.AppendLine($"                var oldValue = {fieldName};");
                    sb.AppendLine($"                if (SetProperty(ref {fieldName}, value))");
                    sb.AppendLine($"                    On{propertyName}Changed(oldValue, value);");
                    sb.AppendLine($"            }}");
                    sb.AppendLine($"        }}");
                    sb.AppendLine($"        partial void On{propertyName}Changed({propertyType} oldValue, {propertyType} newValue);");
                }

                sb.AppendLine($"    }}");
                sb.AppendLine($"}}");

                spc.AddSource($"{className}_StswObservableProperties.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
            }
        });
    }
}
