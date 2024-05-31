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
            ((ImmutableArray<MethodModel> BuilderMethods, ImmutableArray<MethodModel> GroupMethods) Methods,
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
                            source.AppendLine($"{method.StaticCall}(builder);");
                        }

                        var methodGroups = input.Methods.GroupMethods
                            .Where(m => !CheckMethod(context, m, true))
                            .GroupBy(m => m.GroupedAttributeParameters);

                        var groupNum = 0;

                        foreach (var g1 in methodGroups)
                        {
                            var b = $"builder.MapGroup(\"{g1.Key!.Prefix}\")";
                            var namedGroups = g1.GroupBy(static m => m.ContainingType);

                            foreach (var g2 in namedGroups)
                            {
                                var b2 = b + $".WithName(\"{g2.Key.Name}\")";
                                if (g2.Key.Tag != null)
                                    b2 += $".WithTags(\"{g2.Key.Tag}\")";

                                var groupName = "group" + groupNum++;
                                source.AppendLine($"var {groupName} = {b2};");

                                foreach (var method in g2)
                                {
                                    source.AppendLine($"{method.StaticCall}({groupName});");
                                }
                            }
                        }

                        source.AppendLine("return builder;");
                    }
                }
            }

            context.AddSource($"EndpointGenerator.g.cs", source);
        }

        private static bool CheckMethod(EmitterContext context, MethodModel method, bool isGrouped)
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
            if (method.ParameterCount != 1)
            {
                ReportDiagnostic(context, method, Diagnostics.BuilderMethodMustHaveOnlyOneArgument);
                failed = true;
            }
            else
            {
                if (!isGrouped &&
                    method.FirstParameterType != "global::Microsoft.AspNetCore.Routing.IEndpointRouteBuilder")
                {
                    ReportDiagnostic(context, method, Diagnostics.BuilderMethodMustHaveCorrectArg);
                    failed = true;
                }
                if (isGrouped &&
                    method.FirstParameterType != "global::Microsoft.AspNetCore.Routing.RouteGroupBuilder")
                {
                    ReportDiagnostic(context, method, Diagnostics.BuilderGroupMethodMustHaveCorrectArg);
                    failed = true;
                }
            }
            return failed;
        }

        private static void ReportDiagnostic(EmitterContext context, MethodModel method, DiagnosticDescriptor diagnostic)
        {
            foreach (var loc in method.Locations)
                context.ReportDiagnostic(Diagnostic.Create(diagnostic, loc, method.ErrorName));
        }
    }
}
