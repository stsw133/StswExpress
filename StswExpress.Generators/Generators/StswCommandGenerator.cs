using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Composition;
using System.Text;

namespace StswExpress.Generators;

/// <summary>
/// Generates command properties for methods marked with StswCommandAttribute or StswAsyncCommandAttribute.
/// </summary>
[Generator]
public class StswCommandGenerator : IIncrementalGenerator
{
    private static readonly string[] TargetAttributes =
    [
        "StswExpress.StswCommandAttribute",
        "StswExpress.StswAsyncCommandAttribute"
    ];

    /// <summary>
    /// Initializes the generator by registering a syntax provider to collect declarations.
    /// </summary>
    /// <param name="context">The generator initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var symbolDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is MethodDeclarationSyntax m && m.AttributeLists.Count > 0,
                transform: static (ctx, _) => Helpers.GetMethodSymbol(ctx, TargetAttributes))
            .Where(static symbol => symbol is not null)
            .Collect();

        context.RegisterSourceOutput(symbolDeclarations, (spc, symbols) =>
        {
            var grouped = symbols!
                .OfType<IMethodSymbol>()
                .Where(m => m.ContainingType is { } ct && Helpers.IsValidPartialClass(ct))
                .GroupBy(m => m.ContainingType, SymbolEqualityComparer.Default);

            foreach (var group in grouped)
            {
                if (group.Key is not INamedTypeSymbol classSymbol)
                    continue;

                var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
                var className = classSymbol.Name;

                var existingProperties = classSymbol
                    .GetMembers()
                    .OfType<IPropertySymbol>()
                    .Select(p => p.Name)
                    .ToImmutableHashSet();

                var commands = group
                    .Select(method =>
                    {
                        var methodName = method.Name;
                        var isAsync = method.ReturnType.ToDisplayString() == "System.Threading.Tasks.Task";
                        var hasParameter = method.Parameters.Length == 1;
                        var paramType = hasParameter
                            ? method.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
                            : null;

                        var commandBaseType = isAsync ? "StswAsyncCommand" : "StswCommand";
                        var fullCommandType = hasParameter
                            ? $"{commandBaseType}<{paramType}>"
                            : commandBaseType;

                        var propertyName = methodName + "Command";

                        return new
                        {
                            MethodName = methodName,
                            PropertyName = propertyName,
                            FullCommandType = fullCommandType
                        };
                    })
                    .Where(cmd => !existingProperties.Contains(cmd.PropertyName))
                    .GroupBy(cmd => cmd.PropertyName)
                    .Select(g => g.First())
                    .ToList();

                if (commands.Count == 0)
                    continue;

                var classDecl = classSymbol.DeclaringSyntaxReferences
                    .Select(r => r.GetSyntax())
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault();

                var ctor = classDecl?.Members
                    .OfType<ConstructorDeclarationSyntax>()
                    .FirstOrDefault(x => x.ParameterList.Parameters.Count == 0);

                var sb = new StringBuilder();
                sb.AppendLine($"namespace {namespaceName}");
                sb.AppendLine($"{{");
                sb.AppendLine($"    public partial class {className}");
                sb.AppendLine($"    {{");
                commands.ForEach(cmd => sb.AppendLine($"        public {cmd.FullCommandType} {cmd.PropertyName} {{ get; private set; }}"));
                sb.AppendLine();

                if (ctor is null)
                {
                    sb.AppendLine($"        public {className}()");
                    sb.AppendLine($"        {{");
                    foreach (var cmd in commands)
                        sb.AppendLine($"            {cmd.PropertyName} = new({cmd.MethodName});");
                    sb.AppendLine($"        }}");
                }
                else
                {
                    sb.AppendLine($"        partial void InitializeGeneratedCommands()");
                    sb.AppendLine($"        {{");
                    foreach (var cmd in commands)
                        sb.AppendLine($"            {cmd.PropertyName} = new({cmd.MethodName});");
                    sb.AppendLine($"        }}");
                }

                sb.AppendLine($"    }}");
                sb.AppendLine($"}}");

                spc.AddSource($"{className}_StswCommands.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
            }
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MissingInitializeGeneratedCommandsAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "STSW001";
    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Missing call to InitializeGeneratedCommands()",
        "Constructor should call InitializeGeneratedCommands()",
        "Usage",
        DiagnosticSeverity.Warning,
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
