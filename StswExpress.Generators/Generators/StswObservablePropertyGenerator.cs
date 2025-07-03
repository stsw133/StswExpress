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
    private const string InvokingAttribute = "StswExpress.Commons.StswObservablePropertyAttribute";

    /// <summary>
    /// Initializes the generator and registers the syntax provider for field declarations with the StswObservablePropertyAttribute attribute.
    /// </summary>
    /// <param name="context">The generator initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var fieldDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is FieldDeclarationSyntax f && f.AttributeLists.Count > 0,
                transform: static (ctx, _) => StswFn.GetFieldSymbol(ctx))
            .Where(static symbol => symbol is not null);

        context.RegisterSourceOutput(fieldDeclarations, (spc, symbol) =>
        {
            /// validations
            if (symbol is not IFieldSymbol field)
                return;

            if (!StswFn.TryGetInvokingAttributeData(field, InvokingAttribute, out var attrData) || attrData is null)
                return;

            /// get the necessary information from the field symbol
            var classSymbol = field.ContainingType;
            var fieldName = field.Name;
            var propertyName = StswFn.ToPascalCase(field.Name.TrimStart('_'));
            var propertyType = field.Type.ToDisplayString();

            /// get the documentation comment for the field
            var docComment = field.GetDocumentationCommentXml();
            var documentation = string.IsNullOrWhiteSpace(docComment) ? "" : $"/// {docComment!.Trim().Replace("\n", "\n/// ")}\n";

            /// generate the attributes code
            var attributesCode = string.Join("\n", field.GetAttributes()
                .Where(attr => attr.AttributeClass?.ToDisplayString() != InvokingAttribute)
                .Select(attr => attr.ToString()));

            /// get the named arguments from the StswObservablePropertyAttribute
            var callbackMethod = StswFn.GetNamedArgument<string>(attrData, "CallbackMethod");
            var conditionMethod = StswFn.GetNamedArgument<string>(attrData, "ConditionMethod");
            var notifyProps = StswFn.GetNamedArgument<string[]>(attrData, "NotifyProperties");

            var additionalArgs = string.Join(", ",
            [
                notifyProps?.Length > 0
                    ? $"new[] {{ {string.Join(", ", notifyProps.Select(p => $@"""{p}"""))} }}"
                    : "null",
                /*callbackMethod is not null ? callbackMethod :*/ "null",
                conditionMethod is not null ? conditionMethod : "null"
            ]);

            /// generate the source code for the observable property
            var source = $@"
namespace {classSymbol.ContainingNamespace}
{{
    partial class {classSymbol.Name}
    {{
        {documentation}
        {attributesCode}
        public {propertyType} {propertyName}
        {{
            get => {fieldName};
            set
            {{
                var oldValue = {fieldName};
                if (SetProperty(ref {fieldName}, value, {additionalArgs}))
                    On{propertyName}Changed(oldValue, value);
            }}
        }}

        partial void On{propertyName}Changed({propertyType} oldValue, {propertyType} newValue);
    }}
}}";
            spc.AddSource($"{classSymbol.Name}_{propertyName}_ObservableProperty.g.cs", SourceText.From(source, Encoding.UTF8));
        });
    }
}
