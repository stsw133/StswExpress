using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StswExpress.Analyzers;
internal class Helpers
{
    /// <summary>
    /// Retrieves the field symbol from the syntax context.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <returns>The field symbol if found; otherwise, <see langword="null"/>.</returns>
    internal static IFieldSymbol? GetFieldSymbol(GeneratorSyntaxContext context)
    {
        if (context.Node is not FieldDeclarationSyntax field)
            return null;

        foreach (var variable in field.Declaration.Variables)
        {
            var symbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
            if (symbol is not null)
                return symbol;
        }

        return null;
    }

    /// <summary>
    /// Retrieves the method symbol from the syntax context based on the specified attribute names.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="attributeNames">An array of attribute names to check against.</param>
    /// <returns>The method symbol if found; otherwise, <see langword="null"/>.</returns>
    public static IMethodSymbol? GetMethodSymbol(GeneratorSyntaxContext context, params string[] attributeNames)
    {
        if (context.Node is not MethodDeclarationSyntax methodDecl)
            return null;

        foreach (var attrList in methodDecl.AttributeLists)
        {
            foreach (var attr in attrList.Attributes)
            {
                var symbolInfo = context.SemanticModel.GetSymbolInfo(attr);
                if (symbolInfo.Symbol is not IMethodSymbol attributeSymbol)
                    continue;

                var fullName = attributeSymbol.ContainingType.ToDisplayString();
                if (attributeNames.Contains(fullName))
                    return context.SemanticModel.GetDeclaredSymbol(methodDecl) as IMethodSymbol;
            }
        }

        return null;
    }

    /// <summary>
    /// Retrieves a named argument from the attribute data.
    /// </summary>
    /// <typeparam name="T">The type of the argument value to retrieve.</typeparam>
    /// <param name="attributeData">The attribute data containing the named arguments.</param>
    /// <param name="name">The name of the argument to retrieve.</param>
    /// <returns>The value of the named argument if found; otherwise, <see langword="null"/>.</returns>
    internal static T? GetNamedArgument<T>(AttributeData attributeData, string name)
    {
        foreach (var arg in attributeData.NamedArguments)
        {
            if (arg.Key == name && arg.Value.Value is T value)
                return value;
        }
        return default;
    }

    /// <summary>
    /// Checks if the given symbol is a valid partial class.
    /// </summary>
    /// <param name="symbol">The symbol to check.</param>
    /// <returns><see langword="true"/> if the symbol is a valid partial class; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidPartialClass(INamedTypeSymbol symbol)
        => symbol.TypeKind == TypeKind.Class &&
               !symbol.IsStatic &&
               symbol.DeclaringSyntaxReferences
                   .Select(r => r.GetSyntax())
                   .OfType<ClassDeclarationSyntax>()
                   .Any(cds => cds.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));

    /// <summary>
    /// Converts a string to PascalCase.
    /// </summary>
    /// <param name="name">The string to convert.</param>
    /// <returns>The PascalCase version of the string.</returns>
    internal static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        if (name.Length == 1) return name.ToUpper();
        return char.ToUpper(name[0]) + name.Substring(1);
    }

    /// <summary>
    /// Tries to get the attribute data for a specific invoking attribute from the field symbol.
    /// </summary>
    /// <param name="symbol">The field symbol to check.</param>
    /// <param name="attributeFullName"> The full name of the attribute to look for.</param>
    /// <param name="attributeData">The attribute data if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the attribute was found; otherwise, <see langword="false"/>.</returns>
    internal static bool TryGetInvokingAttributeData(ISymbol symbol, string attributeFullName, out AttributeData? attributeData)
    {
        foreach (var attr in symbol.GetAttributes())
        {
            if (attr.AttributeClass?.ToDisplayString() == attributeFullName)
            {
                attributeData = attr;
                return true;
            }
        }

        attributeData = null;
        return false;
    }
}
