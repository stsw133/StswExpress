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

                    bool? reusableFromCtor = null;
                    if (attrData.ConstructorArguments.Length >= 2)
                    {
                        var arg = attrData.ConstructorArguments[1];
                        if (!arg.IsNull && arg.Value is bool b)
                            reusableFromCtor = b;
                    }

                    bool? reusableFromProp = null;
                    foreach (var kv in attrData.NamedArguments)
                    {
                        if (kv.Key == "IsReusable" && kv.Value.Value is bool b)
                        {
                            reusableFromProp = b;
                            break;
                        }
                    }

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
