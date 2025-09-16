using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace StswExpress.Analyzers;

/// <summary>
/// Analyzer to check the status of elements marked with StswInfoAttribute.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StswAnalyzer : DiagnosticAnalyzer
{
    private const string InvokingAttribute = "StswExpress.Commons.StswInfoAttribute";

    public const string SinceVersionInfoId = "STSW000";
    public const string IsNotTestedId = "STSW001";
    public const string MarkedForRemovalId = "STSW002";
    public const string PlannedChangesId = "STSW003";

    private static readonly DiagnosticDescriptor SinceVersionInfoRule = new(
        id: SinceVersionInfoId,
        title: "Feature version info",
        messageFormat: "Element '{0}' was added in version {1}",
        category: "StswExpress",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "This element was introduced in a specific version of the project to help track feature history.");

    private static readonly DiagnosticDescriptor IsNotTestedRule = new(
        id: IsNotTestedId,
        title: "Not tested functionality",
        messageFormat: "Element '{0}' is marked as untested (IsTested = false)",
        category: "StswExpress",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "This element is marked as untested."
                   + "It may not work correctly in all scenarios and is not guaranteed to be stable."
                   + "Consider testing it before using in production code."
                   + "If you need a stable version, consider using an alternative solution or implementing it yourself.");

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

    /// <summary>
    /// Gets the collection of diagnostic descriptors supported by this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(SinceVersionInfoRule, IsNotTestedRule, MarkedForRemovalRule, PlannedChangesRule);

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

        /// SinceVersion
        //var sinceVersion = attr.ConstructorArguments[0].Value?.ToString();
        //if (!string.IsNullOrWhiteSpace(sinceVersion) && ShouldReportVersion(sinceVersion))
        //{
        //    var diagnostic = Diagnostic.Create(SinceVersionInfoRule, symbol.Locations[0], symbol.Name, sinceVersion);
        //    context.ReportDiagnostic(diagnostic);
        //}

        /// IsTested = false
        var isTested = true;
        foreach (var arg in attr.NamedArguments)
        {
            if (arg.Key == "IsTested" && arg.Value.Value is bool val)
                isTested = val;
        }

        if (!isTested)
        {
            var diagnostic = Diagnostic.Create(IsNotTestedRule, symbol.Locations[0], symbol.Name);
            context.ReportDiagnostic(diagnostic);
        }

        /// Changes > 0
        var changesArg = attr.NamedArguments.FirstOrDefault(x => x.Key == "Changes");
        if (changesArg.Value.Value is int changesValue)
        {
            var enumValue = (StswPlannedChanges)changesValue;

            if (enumValue.HasFlag(StswPlannedChanges.Remove))
            {
                var diagnostic = Diagnostic.Create(MarkedForRemovalRule, symbol.Locations[0], symbol.Name);
                context.ReportDiagnostic(diagnostic);
            }

            var relevantFlags = enumValue & ~StswPlannedChanges.None & ~StswPlannedChanges.Remove;
            if (relevantFlags != 0)
            {
                var diagnostic = Diagnostic.Create(PlannedChangesRule, symbol.Locations[0], symbol.Name, relevantFlags.ToString());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
    /*
    /// <summary>
    /// The version of the analyzer assembly.
    /// </summary>
    private static readonly Version AnalyzerVersion = typeof(StswAnalyzer).Assembly.GetName().Version ?? new Version(0, 0);

    /// <summary>
    /// Determines if the version should be reported based on the sinceVersion parameter.
    /// </summary>
    /// <param name="sinceVersion">The version string to check.</param>
    /// <returns><see langword="true"> if the version should be reported, otherwise <see langword="false"/>.</returns>
    private static bool ShouldReportVersion(string? sinceVersion)
    {
        (var major1, var minor1) = ParseVersion(sinceVersion ?? "0.0.0");
        return (AnalyzerVersion.Major == major1) && (AnalyzerVersion.Minor - minor1 <= 3);
    }

    /// <summary>
    /// Parses the version string into major and minor components.
    /// </summary>
    /// <param name="version">The version string to parse.</param>
    /// <returns>A tuple containing the major and minor version numbers.</returns>
    private static (int major, int minor) ParseVersion(string version)
    {
        var parts = version.Split('.');
        var major = parts.Length > 0 && int.TryParse(parts[0], out var m) ? m : 0;
        var minor = parts.Length > 1 && int.TryParse(parts[1], out var n) ? n : 0;
        return (major, minor);
    }
    */
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
    Remove = 1024,
}
