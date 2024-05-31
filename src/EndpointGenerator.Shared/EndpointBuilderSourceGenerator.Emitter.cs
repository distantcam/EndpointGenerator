using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

#if ROSLYN_3
using EmitterContext = Microsoft.CodeAnalysis.GeneratorExecutionContext;
#elif ROSLYN_4
using EmitterContext = Microsoft.CodeAnalysis.SourceProductionContext;
#endif

namespace EndpointGenerator;

[Generator(LanguageNames.CSharp)]
public partial class EndpointBuilderSourceGenerator
{
    private static class Emitter
    {
        public static void GenerateSource(
            EmitterContext context,
            ((ImmutableArray<IMethodSymbol> BuilderMethods, ImmutableArray<IMethodSymbol> GroupMethods) Methods,
            string? AssemblyName) input)
        {
            if (input.Methods.BuilderMethods.IsDefaultOrEmpty && input.Methods.GroupMethods.IsDefaultOrEmpty)
                return;

            var methodName = Regex.Replace(input.AssemblyName, "\\W", "");

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
$"public static IEndpointRouteBuilder Map{methodName}Endpoints(this IEndpointRouteBuilder builder)"))
                    {
                        foreach (var method in input.Methods.BuilderMethods)
                        {
                            if (CheckMethod(context, method, false)) continue;

                            var type = method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                            var name = method.Name;

                            source.AppendLine($"{type}.{name}(builder);");
                        }

                        var methodGroups = input.Methods.GroupMethods
                            .Where(m => !CheckMethod(context, m, true))
                            .GroupBy(GetAttributeParameters);

                        var groupNum = 0;

                        foreach (var g1 in methodGroups)
                        {
                            var b = $"builder.MapGroup(\"{g1.Key!.Prefix}\")";
                            var namedGroups = g1.GroupBy(static m => m.ContainingType, SymbolEqualityComparer.Default);

                            foreach (var g2 in namedGroups)
                            {
                                var b2 = b + $".WithName(\"{g2.Key!.Name}\")";
                                if (!g2.Key.ContainingNamespace.IsGlobalNamespace)
                                    b2 += $".WithTags(\"{g2.Key.ContainingNamespace}\")";

                                var groupName = "group" + groupNum++;
                                source.AppendLine($"var {groupName} = {b2};");

                                foreach (var method in g2)
                                {
                                    var type = method.ContainingType
                                        .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                                    var name = method.Name;

                                    source.AppendLine($"{type}.{name}({groupName});");
                                }
                            }
                        }

                        source.AppendLine("return builder;");
                    }
                }
            }

            context.AddSource($"EndpointGenerator.g.cs", source);
        }

        private static bool CheckMethod(EmitterContext context, IMethodSymbol method, bool isGrouped)
        {
            var failed = false;
            if (!method.IsStatic)
            {
                ReportDiagnostic(context, method, Diagnostics.BuilderMethodMustBeStatic);
                failed = true;
            }
            if (method.DeclaredAccessibility != Accessibility.Public &&
                method.DeclaredAccessibility != Accessibility.Internal &&
                method.DeclaredAccessibility != Accessibility.ProtectedOrInternal)
            {
                ReportDiagnostic(context, method, Diagnostics.BuilderMethodMustBeAccessible);
                failed = true;
            }
            var args = method.Parameters;
            if (args.Length != 1)
            {
                ReportDiagnostic(context, method, Diagnostics.BuilderMethodMustHaveOnlyOneArgument);
                failed = true;
            }
            else
            {
                var argType = args[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                if (!isGrouped && argType != "global::Microsoft.AspNetCore.Routing.IEndpointRouteBuilder")
                {
                    ReportDiagnostic(context, method, Diagnostics.BuilderMethodMustHaveCorrectArg);
                    failed = true;
                }
                if (isGrouped && argType != "global::Microsoft.AspNetCore.Routing.RouteGroupBuilder")
                {
                    ReportDiagnostic(context, method, Diagnostics.BuilderGroupMethodMustHaveCorrectArg);
                    failed = true;
                }
            }
            return failed;
        }

        private static void ReportDiagnostic(EmitterContext context, IMethodSymbol method, DiagnosticDescriptor diagnostic)
        {
            foreach (var loc in method.Locations)
                context.ReportDiagnostic(Diagnostic.Create(diagnostic, loc, method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
        }

        private static GroupedAttributeParameters? GetAttributeParameters(IMethodSymbol method)
        {
            var attribute = method.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::EndpointGenerator.EndpointGroupBuilderAttribute");
            if (attribute == null) return null;

            return new GroupedAttributeParameters(
                attribute.ConstructorArguments[0].Value?.ToString() ?? string.Empty
            );
        }

        private record GroupedAttributeParameters(string Prefix);
    }
}
