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
    private static readonly string[] AttributeFullNames =
    [
        "StswExpress.Avalonia.StswCommandAttribute",
        "StswExpress.Wpf.StswCommandAttribute",
    ];

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
                    Attribute = Helpers.GetAttribute(method, AttributeFullNames)
                })
                .Where(m => m.Attribute is not null)
                .GroupBy(m => m.Method.ContainingType, SymbolEqualityComparer.Default);

            foreach (var group in grouped)
            {
                if (group.Key is not INamedTypeSymbol namedTypeSymbol)
                    continue;

                var classCtx = Helpers.GetClassContext(namedTypeSymbol);

                var sb = new StringBuilder();
                sb.AppendLine("#nullable enable");
                sb.AppendLine($"namespace {classCtx.Namespace}");
                sb.AppendLine("{");
                sb.AppendLine($"    public partial class {classCtx.ClassName}");
                sb.AppendLine("    {");

                foreach (var item in group)
                {
                    var attrData = item.Attribute;
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

                    string? conditionFromCtor = null;
                    if (attrData.ConstructorArguments.Length >= 1)
                    {
                        var arg = attrData.ConstructorArguments[0];
                        if (!arg.IsNull && arg.Value is string s)
                            conditionFromCtor = s;
                    }

                    string? conditionFromProp = null;
                    foreach (var kv in attrData.NamedArguments)
                    {
                        if (kv.Key == "ConditionMethodName" && kv.Value.Value is string s)
                        {
                            conditionFromProp = s;
                            break;
                        }
                    }

                    var conditionMethod = conditionFromCtor ?? conditionFromProp;
                    var conditionArg = !string.IsNullOrWhiteSpace(conditionMethod) ? conditionMethod : "null";
                    var isReusable = isAsync && Helpers.GetNamedArgument<bool>(attrData, "IsReusable");
                    var tryCatchTargets = GetTryCatchTargets(attrData);

                    sb.AppendLine($"        private {fullCommandType}? {fieldName};");

                    if (tryCatchTargets == 0)
                    {
                        sb.AppendLine($"        public {fullCommandType} {propertyName} => {fieldName} ??= new {fullCommandType}({methodName}, {conditionArg})");
                    }
                    else
                    {
                        sb.AppendLine($"        public {fullCommandType} {propertyName} => {fieldName} ??= new {fullCommandType}(");
                        AppendTryCatchDelegate(sb, item.Method, methodName, isAsync, hasToken, tryCatchTargets);
                        sb.AppendLine($"            {conditionArg})");
                    }

                    sb.AppendLine("        {");
                    if (isReusable)
                        sb.AppendLine("            IsReusable = true");
                    sb.AppendLine("        };");
                    sb.AppendLine();
                }

                sb.AppendLine("    }");
                sb.AppendLine("}");

                spc.AddSource($"{classCtx.ClassName}_StswCommands.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
            }
        });
    }

    private static long GetTryCatchTargets(AttributeData attributeData)
    {
        foreach (var argument in attributeData.NamedArguments)
        {
            if (argument.Key != "TryCatch" || argument.Value.Value is null)
                continue;

            return Convert.ToInt64(argument.Value.Value);
        }

        return 0;
    }

    private static void AppendTryCatchDelegate(StringBuilder sb, IMethodSymbol method, string methodName, bool isAsync, bool hasToken, long tryCatchTargets)
    {
        var parameters = method.Parameters;
        var lambdaParameters = hasToken
            ? parameters.Length > 1 ? "(parameter, cancellationToken)" : "cancellationToken"
            : parameters.Length == 1 ? "parameter" : "()";

        var invocationArguments = hasToken
            ? parameters.Length > 1 ? "parameter, cancellationToken" : "cancellationToken"
            : parameters.Length == 1 ? "parameter" : string.Empty;

        sb.AppendLine($"            {(isAsync ? "async " : string.Empty)}{lambdaParameters} =>");
        sb.AppendLine("            {");
        sb.AppendLine("                try");
        sb.AppendLine("                {");
        sb.AppendLine($"                    {(isAsync ? "await " : string.Empty)}{methodName}({invocationArguments});");
        sb.AppendLine("                }");
        sb.AppendLine("                catch (global::System.Exception ex)");
        sb.AppendLine("                {");
        sb.AppendLine("                    global::StswExpress.Commons.StswLog.WriteException(");
        sb.AppendLine("                        ex,");
        sb.AppendLine("                        global::StswExpress.Commons.StswInfoType.Error,");
        sb.AppendLine($"                        nameof({methodName}),");
        sb.AppendLine($"                        (global::StswExpress.Commons.StswLogTarget){tryCatchTargets});");
        sb.AppendLine("                }");
        sb.AppendLine("            },");
    }
}
