using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StswExpress.Generators;
internal class StswFn
{

    /// <summary>
    /// Retrieves the field symbol from the syntax context.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <returns>The field symbol if found; otherwise, <see langword="null"/>.</returns>
    internal static IFieldSymbol? GetFieldSymbol(GeneratorSyntaxContext context)
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
            if (arg.Key == name)
            {
                if (arg.Value.Value is T value)
                    return value;
            }
        }
        return default;
    }

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
    /// <param name="field">The field symbol to check.</param>
    /// <param name="attributeData">The attribute data if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the attribute was found; otherwise, <see langword="false"/>.</returns>
    internal static bool TryGetInvokingAttributeData(IFieldSymbol field, string invokingAttribute, out AttributeData? attributeData)
    {
        foreach (var attr in field.GetAttributes())
        {
            if (attr.AttributeClass?.ToDisplayString() == invokingAttribute)
            {
                attributeData = attr;
                return true;
            }
        }

        attributeData = null;
        return false;
    }
}
