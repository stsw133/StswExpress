using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace StswExpress.Generators;

/// <summary>
/// Generates observable properties for fields marked with the StswObservablePropertyAttribute attribute.
/// </summary>
[Generator]
public class StswObservablePropertyGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the generator and registers the syntax provider for field declarations with the StswObservablePropertyAttribute attribute.
    /// </summary>
    /// <param name="context">The generator initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var fieldDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsCandidate(node),
                transform: static (ctx, _) => GetFieldSymbol(ctx))
            .Where(static field => field is not null);

        context.RegisterSourceOutput(fieldDeclarations, (spc, fieldSymbol) =>
        {
            if (fieldSymbol is not IFieldSymbol field)
                return;

            if (!HasStswObservablePropertyAttribute(field))
                return;

            var classSymbol = field.ContainingType;
            var propertyName = ToPascalCase(field.Name.TrimStart('_'));
            var propertyType = field.Type.ToDisplayString();
            var fieldName = field.Name;

            var source = $@"
namespace {classSymbol.ContainingNamespace}
{{
    partial class {classSymbol.Name}
    {{
        public {propertyType} {propertyName}
        {{
            get => {fieldName};
            set => SetProperty(ref {fieldName}, value);
        }}
    }}
}}";
            spc.AddSource($"{classSymbol.Name}_{propertyName}_ObservableProperty.g.cs", SourceText.From(source, Encoding.UTF8));
        });
    }

    /// <summary>
    /// Checks if the syntax node is a candidate for processing.
    /// </summary>
    /// <param name="node">The syntax node to check.</param>
    /// <returns><see langword="true"/> if the node is a field declaration with attributes; otherwise, <see langword="false"/>.</returns>
    private static bool IsCandidate(SyntaxNode node) => node is FieldDeclarationSyntax f && f.AttributeLists.Count > 0;

    /// <summary>
    /// Retrieves the field symbol from the syntax context.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <returns>The field symbol if found; otherwise, <see langword="null"/>.</returns>
    private static IFieldSymbol? GetFieldSymbol(GeneratorSyntaxContext context)
    {
        if (context.Node is not FieldDeclarationSyntax fieldDecl)
            return null;

        foreach (var variable in fieldDecl.Declaration.Variables)
        {
            var symbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
            if (symbol is not null)
                return symbol;
        }

        return null;
    }

    /// <summary>
    /// Checks if the field has the StswObservablePropertyAttribute attribute.
    /// </summary>
    /// <param name="field">The field symbol to check.</param>
    /// <returns><see langword="true"/> if the field has the attribute; otherwise, <see langword="false"/>.</returns>
    private static bool HasStswObservablePropertyAttribute(IFieldSymbol field)
    {
        foreach (var attribute in field.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == "StswExpress.Commons.StswObservablePropertyAttribute"
             || attribute.AttributeClass?.Name == "StswObservablePropertyAttribute")
                return true;
        }
        return false;
    }

    /// <summary>
    /// Converts a string to PascalCase.
    /// </summary>
    /// <param name="name">The string to convert.</param>
    /// <returns>The PascalCase version of the string.</returns>
    private static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        if (name.Length == 1) return name.ToUpper();
        return char.ToUpper(name[0]) + name.Substring(1);
    }
}