﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StswExpress.Analyzers;
internal static class Helpers
{
    /// <summary>
    /// Formats the documentation comment for a given symbol.
    /// </summary>
    /// <param name="symbol">The symbol to format the documentation for.</param>
    /// <returns>The formatted documentation comment, or <see langword="null"/> if no documentation is available.</returns>
    public static string? FormatDocComment(ISymbol symbol)
    {
        var doc = symbol.GetDocumentationCommentXml();
        if (string.IsNullOrWhiteSpace(doc)) return null;
        return "        /// " + doc!.Trim().Replace("\n", "\n        /// ");
    }

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
    /// Retrieves the method symbol from the syntax context.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <returns>The method symbol if found; otherwise, <see langword="null"/>.</returns>
    public static IMethodSymbol? GetMethodSymbol(GeneratorSyntaxContext context)
    {
        if (context.Node is not MethodDeclarationSyntax methodDecl)
            return null;

        return context.SemanticModel.GetDeclaredSymbol(methodDecl);
    }

    /// <summary>
    /// Retrieves the attribute data for a specific attribute from the given symbol.
    /// </summary>
    /// <param name="symbol">The symbol to check for attributes.</param>
    /// <param name="attributeFullName">The full name of the attribute to look for.</param>
    /// <returns>The attribute data if found; otherwise, <see langword="null"/>.</returns>
    internal static AttributeData? GetAttribute(ISymbol symbol, string attributeFullName)
        => symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == attributeFullName);

    /// <summary>
    /// Creates a new instance of <see cref="PartialClassContext"/> for the specified class symbol.
    /// </summary>
    /// <param name="classSymbol">The class symbol to create the context for.</param>
    /// <returns>A new instance of <see cref="PartialClassContext"/> containing the class symbol.</returns>
    public static PartialClassContext GetClassContext(INamedTypeSymbol classSymbol)
        => new PartialClassContext { ClassSymbol = classSymbol };

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
    /// Tries to get the symbol and attribute data for a specific attribute from the syntax context.
    /// </summary>
    /// <typeparam name="TSymbol">The type of the symbol to retrieve (e.g., <see cref="IFieldSymbol"/> or <see cref="IMethodSymbol"/>).</typeparam>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="attributeFullName">The full name of the attribute to look for.</param>
    /// <param name="symbol">The symbol if found; otherwise, <see langword="null"/>.</param>
    /// <param name="attributeData">The attribute data if found; otherwise, <see langword="null"/>.</param>
    /// <returns></returns>
    public static bool TryGetAttributedSymbol<TSymbol>(
        GeneratorSyntaxContext context,
        string attributeFullName,
        out TSymbol? symbol,
        out AttributeData? attributeData)
        where TSymbol : class, ISymbol
    {
        symbol = null;
        attributeData = null;

        if (context.Node is not MemberDeclarationSyntax memberSyntax)
            return false;

        var declaredSymbols = memberSyntax switch
        {
            FieldDeclarationSyntax fieldDecl => fieldDecl.Declaration.Variables
                .Select(v => context.SemanticModel.GetDeclaredSymbol(v)).OfType<TSymbol>(),

            MethodDeclarationSyntax methodDecl => new[] { context.SemanticModel.GetDeclaredSymbol(methodDecl) }
                .OfType<TSymbol>(),

            _ => []
        };

        foreach (var s in declaredSymbols)
        {
            var attr = GetAttribute(s, attributeFullName);
            if (attr is not null)
            {
                symbol = s;
                attributeData = attr;
                return true;
            }
        }

        return false;
    }
}

/// <summary>
/// Represents the context for a partial class, containing its symbol and related information.
/// </summary>
internal class PartialClassContext
{
    public INamedTypeSymbol ClassSymbol { get; set; } = default!;
    public string Namespace => ClassSymbol.ContainingNamespace.ToDisplayString();
    public string ClassName => ClassSymbol.Name;
    public string FullName => $"{Namespace}.{ClassName}";
}
