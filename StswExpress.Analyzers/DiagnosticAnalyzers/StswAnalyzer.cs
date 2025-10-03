using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace StswExpress.Analyzers;

/// <summary>
/// Analyzer to check the status of elements marked with StswChangelogAttribute.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StswAnalyzer : DiagnosticAnalyzer
{
    private const string InvokingAttribute = "StswExpress.Commons.StswPlannedChangesAttribute";

    public const string PlannedChangesId = "STSW000";
    public const string MarkedForChangeAccessibilityId = "STSW001";
    public const string MarkedForFinishId = "STSW002";
    public const string MarkedForRemovalId = "STSW003";
    public const string MarkedForReworkId = "STSW004";

    private static readonly DiagnosticDescriptor PlannedChangesRule = new(
        id: PlannedChangesId,
        title: "Planned changes in functionality",
        messageFormat: "Element '{0}' has planned changes: {1}",
        category: "StswExpress",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "This element has planned changes and may change in future versions."
                   + "It is recommended to keep an eye on updates to this element to avoid issues with future versions."
                   + "If you need a stable version, consider using an alternative solution or implementing it yourself."
                   + "Planned changes can include logic changes, visual changes, rework, or other modifications."
                   + "For more details, refer to the documentation or release notes for this element.");

    private static readonly DiagnosticDescriptor MarkedForChangeAccessibilityRule = new(
        id: MarkedForChangeAccessibilityId,
        title: "Functionality marked for accessibility change",
        messageFormat: "Element '{0}' is marked with 'ChangeAccessibility' flag and its accessibility may change in the future",
        category: "StswExpress",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "This element is marked for accessibility change and its accessibility may be changed in future versions."
                   + "Consider reviewing its usage to avoid issues with future updates.");

    private static readonly DiagnosticDescriptor MarkedForFinishRule = new(
        id: MarkedForFinishId,
        title: "Functionality marked for finish",
        messageFormat: "Element '{0}' is marked with 'Finish' flag and may be finalized in the future",
        category: "StswExpress",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "This element is marked for finish and may be finalized in future versions."
                   + "Consider reviewing its usage to avoid issues with future updates.");

    private static readonly DiagnosticDescriptor MarkedForRemovalRule = new(
        id: MarkedForRemovalId,
        title: "Functionality marked for removal",
        messageFormat: "Element '{0}' is marked with 'Remove' flag and may be removed in the future",
        category: "StswExpress",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "This element is marked for removal and may be removed in future versions."
                   + "Consider removing it from your code to avoid issues with future updates."
                   + "If you need this functionality, consider implementing it yourself or using an alternative solution.");

    private static readonly DiagnosticDescriptor MarkedForReworkRule = new(
        id: MarkedForReworkId,
        title: "Functionality marked for rework",
        messageFormat: "Element '{0}' is marked with 'Rework' flag and may be reworked in the future",
        category: "StswExpress",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "This element is marked for rework and may be significantly changed in future versions."
                   + "Consider reviewing its usage to avoid issues with future updates.");

    /// <summary>
    /// Gets the collection of diagnostic descriptors supported by this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(
            PlannedChangesRule,
            MarkedForChangeAccessibilityRule,
            MarkedForFinishRule,
            MarkedForRemovalRule,
            MarkedForReworkRule);

    /// <summary>
    /// Initializes the analyzer and registers the symbol action to analyze symbols with StswAttribute.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeSymbol,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.PropertyDeclaration);
    }

    /// <summary>
    /// Analyzes a symbol to check for StswAttribute and its properties.
    /// </summary>
    /// <param name="context">The symbol analysis context.</param>
    private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var symbol = context.ContainingSymbol;
        if (symbol == null)
            return;

        var attr = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == InvokingAttribute);
        if (attr is null || attr.ConstructorArguments.Length == 0)
            return;

        var changesArg = attr.NamedArguments.FirstOrDefault(x => x.Key == "PlannedChanges");
        if (changesArg.Value.Value is int changesValue)
        {
            var enumValue = (StswPlannedChanges)changesValue;

            if (enumValue.HasFlag(StswPlannedChanges.ChangeAccessibility))
            {
                var diagnostic = Diagnostic.Create(MarkedForChangeAccessibilityRule, symbol.Locations[0], symbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
            if (enumValue.HasFlag(StswPlannedChanges.Finish))
            {
                var diagnostic = Diagnostic.Create(MarkedForFinishRule, symbol.Locations[0], symbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
            if (enumValue.HasFlag(StswPlannedChanges.Remove))
            {
                var diagnostic = Diagnostic.Create(MarkedForRemovalRule, symbol.Locations[0], symbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
            if (enumValue.HasFlag(StswPlannedChanges.Rework))
            {
                var diagnostic = Diagnostic.Create(MarkedForReworkRule, symbol.Locations[0], symbol.Name);
                context.ReportDiagnostic(diagnostic);
            }

            var relevantFlags = enumValue & ~StswPlannedChanges.None
                                          & ~StswPlannedChanges.Finish
                                          & ~StswPlannedChanges.Rework
                                          & ~StswPlannedChanges.ChangeAccessibility
                                          & ~StswPlannedChanges.Remove;
            if (relevantFlags != 0)
            {
                var diagnostic = Diagnostic.Create(PlannedChangesRule, symbol.Locations[0], symbol.Name, relevantFlags.ToString());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}

/// <summary>
/// Flags representing planned changes to a feature or element.
/// </summary>
[Flags]
public enum StswPlannedChanges
{
    None = 0,
    Revision = 1,
    Finish = 2,
    Fix = 4,
    Refactor = 8,
    Rework = 16,
    NewFeatures = 32,
    LogicChanges = 64,
    VisualChanges = 128,
    ChangeAccessibility = 256,
    ChangeName = 512,
    Move = 1024,
    Remove = 2048,
}
