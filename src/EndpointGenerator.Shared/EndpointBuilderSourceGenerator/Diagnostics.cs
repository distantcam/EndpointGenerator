using Microsoft.CodeAnalysis;

namespace EndpointGenerator;

public partial class EndpointBuilderSourceGenerator
{
    private static class Diagnostics
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
    }
}
