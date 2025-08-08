using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace StswExpress.Analyzers;

/// <summary>
/// Generates command properties for methods marked with the StswCommandAttribute.
/// </summary>
[Generator]
public class StswCommandGenerator : IIncrementalGenerator
{
    private const string AttributeFullName = "StswExpress.StswCommandAttribute";

    /// <summary>
    /// Initializes the generator by registering a syntax provider to collect declarations.
    /// </summary>
    /// <param name="context">The generator initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methodSymbols = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is MethodDeclarationSyntax m && m.AttributeLists.Count > 0,
                transform: static (ctx, _) => Helpers.GetMethodSymbol(ctx))
            .Where(static symbol => symbol is not null)
            .Collect();

        context.RegisterSourceOutput(methodSymbols, (spc, symbols) =>
        {
            var grouped = symbols!
                .OfType<IMethodSymbol>()
                .Select(method => new
                {
                    Method = method,
                    Attribute = Helpers.GetAttribute(method, AttributeFullName)
                })
                .Where(m => m.Attribute is not null)
                .GroupBy(m => m.Method.ContainingType, SymbolEqualityComparer.Default);

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
                    var attrData = item.Method.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == AttributeFullName);
                    if (attrData is null)
                        continue;

                    var methodName = item.Method.Name;
                    var propertyName = methodName + "Command";
                    var fieldName = "_" + char.ToLower(propertyName[0]) + propertyName.Substring(1);

                    var isAsync = item.Method.ReturnType.ToDisplayString() == "System.Threading.Tasks.Task";
                    var parameters = item.Method.Parameters;
                    var hasToken = parameters.Length > 0 && parameters.Last().Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.Threading.CancellationToken";

                    string commandType;
                    string? parameterType = null;

                    if (isAsync && hasToken)
                    {
                        commandType = "StswCancellableCommand";
                        if (parameters.Length > 1)
                            parameterType = parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    }
                    else if (isAsync)
                    {
                        commandType = "StswAsyncCommand";
                        if (parameters.Length == 1)
                            parameterType = parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    }
                    else
                    {
                        commandType = "StswCommand";
                        if (parameters.Length == 1)
                            parameterType = parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    }

                    var fullCommandType = parameterType is not null ? $"{commandType}<{parameterType}>" : commandType;
                    var conditionMethod = Helpers.GetNamedArgument<string>(attrData, "ConditionMethodName");
                    var conditionArg = !string.IsNullOrWhiteSpace(conditionMethod) ? conditionMethod : "null";
                    var isReusable = isAsync && Helpers.GetNamedArgument<bool>(attrData, "IsReusable");

                    sb.AppendLine($"        private {fullCommandType}? {fieldName};");
                    sb.AppendLine($"        public {fullCommandType} {propertyName} => {fieldName} ??= new {fullCommandType}({methodName}, {conditionArg})");
                    sb.AppendLine($"        {{");
                    if (isReusable)
                        sb.AppendLine($"            IsReusable = true");
                    sb.AppendLine($"        }};");
                    sb.AppendLine();
                }

                sb.AppendLine($"    }}");
                sb.AppendLine($"}}");

                spc.AddSource($"{classCtx.ClassName}_StswCommands.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
            }
        });
    }
}
/*
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MissingInitializeGeneratedCommandsAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "STSW004";
    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Missing call to InitializeGeneratedCommands()",
        messageFormat: "Constructor should call InitializeGeneratedCommands()",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;

        // only partial classes
        if (!classDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
            return;

        // must contain methods with [StswCommand] or [StswAsyncCommand]
        var hasTargetMethod = classDecl.Members
            .OfType<MethodDeclarationSyntax>()
            .Any(m => m.AttributeLists
                .SelectMany(l => l.Attributes)
                .Any(a =>
                {
                    var symbolInfo = context.SemanticModel.GetSymbolInfo(a).Symbol as IMethodSymbol;
                    return symbolInfo?.ContainingType.ToDisplayString() is
                        "StswExpress.StswCommandAttribute" or "StswExpress.StswAsyncCommandAttribute";
                }));

        if (!hasTargetMethod)
            return;

        var ctors = classDecl.Members
            .OfType<ConstructorDeclarationSyntax>()
            .Where(c => c.Body != null);

        foreach (var ctor in ctors)
        {
            var hasInitCall = ctor.Body!.Statements
                .OfType<ExpressionStatementSyntax>()
                .Any(s =>
                    s.Expression is InvocationExpressionSyntax invocation &&
                    invocation.Expression is IdentifierNameSyntax id &&
                    id.Identifier.Text == "InitializeGeneratedCommands");

            if (!hasInitCall)
            {
                var diagnostic = Diagnostic.Create(Rule, ctor.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddInitializeGeneratedCommandsFixProvider)), Shared]
public class AddInitializeGeneratedCommandsFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MissingInitializeGeneratedCommandsAnalyzer.DiagnosticId);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);
        if (root is null) return;

        var diagnostic = context.Diagnostics[0];
        var ctorNode = root.FindNode(diagnostic.Location.SourceSpan) as ConstructorDeclarationSyntax;
        if (ctorNode is null) return;

        context.RegisterCodeFix(
            Microsoft.CodeAnalysis.CodeActions.CodeAction.Create(
                "Add InitializeGeneratedCommands();",
                ct => AddInitializeCallAsync(context.Document, ctorNode, ct),
                equivalenceKey: "AddInitializeGeneratedCommands"),
            diagnostic);
    }

    private static async Task<Document> AddInitializeCallAsync(Document document, ConstructorDeclarationSyntax ctor, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null) return document;

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        var newStatement = SyntaxFactory.ParseStatement("        InitializeGeneratedCommands();\n");

        var newCtor = ctor.WithBody(ctor.Body!.AddStatements(newStatement));
        var newRoot = root.ReplaceNode(ctor, newCtor);

        return document.WithSyntaxRoot(newRoot);
    }
}
*/
