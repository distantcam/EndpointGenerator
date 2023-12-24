using System.Collections.Immutable;
using System.Text.RegularExpressions;
using EndpointBuilder;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EndpointGenerator;

[Generator(LanguageNames.CSharp)]
public sealed partial class EndpoointBuilderSourceGenerator : IIncrementalGenerator
{
    public static readonly DiagnosticDescriptor BuilderMethodMustBeStatic = new(
        id: "ENDP001",
        title: "Method must be static",
        messageFormat: "The endpoint method '{0}' must be static",
        category: "EndpointGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor BuilderMethodMustBeAccessible = new(
        id: "ENDP002",
        title: "Method must be accessible",
        messageFormat: "The endpoint method '{0}' must be public, internal, or protected internal",
        category: "EndpointGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor BuilderMethodMustHaveOnlyOneArgument = new(
        id: "ENDP003",
        title: "Method must have only one argument",
        messageFormat: "The endpoint method '{0}' must have only one argument",
        category: "EndpointGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor BuilderMethodMustHaveCorrectArg = new(
        id: "ENDP004",
        title: "Method must have correct argument type",
        messageFormat: "The endpoint method '{0}' must have only one argument of type 'Microsoft.AspNetCore.Routing.IEndpointRouteBuilder'",
        category: "EndpointGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor BuilderGroupMethodMustHaveCorrectArg = new(
        id: "ENDP005",
        title: "Method must have correct argument type",
        messageFormat: "The endpoint method '{0}' must have only one argument of type 'Microsoft.AspNetCore.Routing.RouteGroupBuilder'",
        category: "EndpointGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);


    private static bool IsMethodDeclaration(SyntaxNode node, CancellationToken cancellationToken)
        => node is MethodDeclarationSyntax { AttributeLists.Count: > 0 };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider
            .Select(static (c, _) => c.AssemblyName);

        var builderMethods = context.SyntaxProvider.ForAttributeWithMetadataName(
            "EndpointGenerator.EndpointBuilderAttribute",
            IsMethodDeclaration,
            static (c, ct) => (IMethodSymbol)c.TargetSymbol)
        .Collect();

        var groupBuilderMethods = context.SyntaxProvider.ForAttributeWithMetadataName(
            "EndpointGenerator.EndpointGroupBuilderAttribute",
            IsMethodDeclaration,
            static (c, ct) => (IMethodSymbol)c.TargetSymbol)
        .Collect();

        context.RegisterSourceOutput(assemblyName.Combine(builderMethods.Combine(groupBuilderMethods)), GenerateMapping);
    }

    private static void GenerateMapping(
        SourceProductionContext context,
        (string? AssemblyName, (ImmutableArray<IMethodSymbol> BuilderMethods, ImmutableArray<IMethodSymbol> GroupMethods) Methods) model)
    {
        if (model.Methods.BuilderMethods.IsDefaultOrEmpty && model.Methods.GroupMethods.IsDefaultOrEmpty)
            return;

        var methodName = Regex.Replace(model.AssemblyName, "\\W", "");

        var source = new CodeBuilder().AppendHeader().AppendLine();

        source.AppendLineNoIndent("#pragma warning disable CS8019");
        source.AppendLine("using global::Microsoft.AspNetCore.Builder;");
        source.AppendLine("using global::Microsoft.AspNetCore.Http;");
        source.AppendLine("using global::Microsoft.AspNetCore.Routing;");
        source.AppendLineNoIndent("#pragma warning restore CS8019");
        source.AppendLine();

        using (source.StartBlock("namespace Microsoft.Extensions.DependencyInjection"))
        {
            source.AddCompilerGeneratedAttribute();
            source.AddGeneratedCodeAttribute();
            using (source.StartBlock("internal static class EndpointExtensions"))
            {
                using (source.StartBlock(
$"public static IEndpointRouteBuilder Map{methodName}(this IEndpointRouteBuilder builder)"))
                {
                    foreach (var method in model.Methods.BuilderMethods)
                    {
                        if (CheckMethod(context, method, false)) continue;

                        var type = method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        var name = method.Name;

                        source.AppendLine($"{type}.{name}(builder);");
                    }

                    foreach (var method in model.Methods.GroupMethods)
                    {
                        if (CheckMethod(context, method, true)) continue;

                        var type = method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        var name = method.Name;

                        var endpointName = method.ContainingType.Name;

                        var ns = method.ContainingType.ContainingNamespace;

                        if (ns.IsGlobalNamespace)
                        {
                            source.AppendLine(
$"{type}.{name}(builder.MapGroup(\"\").WithName(\"{endpointName}\"));");
                        }
                        else
                        {
                            source.AppendLine(
$"{type}.{name}(builder.MapGroup(\"\").WithName(\"{endpointName}\").WithTags(\"{ns}\"));");
                        }
                    }

                    source.AppendLine("return builder;");
                }
            }
        }

        context.AddSource($"EndpointGenerator.g.cs", source);
    }

    private static bool CheckMethod(SourceProductionContext context, IMethodSymbol method, bool isGrouped)
    {
        var failed = false;
        if (!method.IsStatic)
        {
            ReportDiagnostic(context, method, BuilderMethodMustBeStatic);
            failed = true;
        }
        if (method.DeclaredAccessibility != Accessibility.Public &&
            method.DeclaredAccessibility != Accessibility.Internal &&
            method.DeclaredAccessibility != Accessibility.ProtectedOrInternal)
        {
            ReportDiagnostic(context, method, BuilderMethodMustBeAccessible);
            failed = true;
        }
        var args = method.Parameters;
        if (args.Length != 1)
        {
            ReportDiagnostic(context, method, BuilderMethodMustHaveOnlyOneArgument);
            failed = true;
        }
        else
        {
            var argType = args[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (!isGrouped && argType != "global::Microsoft.AspNetCore.Routing.IEndpointRouteBuilder")
            {
                ReportDiagnostic(context, method, BuilderMethodMustHaveCorrectArg);
                failed = true;
            }
            if (isGrouped && argType != "global::Microsoft.AspNetCore.Routing.RouteGroupBuilder")
            {
                ReportDiagnostic(context, method, BuilderGroupMethodMustHaveCorrectArg);
                failed = true;
            }
        }
        return failed;
    }

    private static void ReportDiagnostic(SourceProductionContext context, IMethodSymbol method, DiagnosticDescriptor diagnostic)
    {
        foreach (var loc in method.Locations)
            context.ReportDiagnostic(Diagnostic.Create(diagnostic, loc, method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
    }
}
