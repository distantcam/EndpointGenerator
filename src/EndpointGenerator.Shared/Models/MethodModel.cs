using Microsoft.CodeAnalysis;

internal record struct ContainingTypeModel(string Name, string? Tag);

internal record GroupedAttributeParametersModel(string Prefix);

internal record struct MethodModel(
    string StaticCall,
    string ErrorName,

    bool IsStatic,
    Accessibility DeclaredAccessibility,

    int ParameterCount,
    string? FirstParameterType,

    ContainingTypeModel ContainingType,

    GroupedAttributeParametersModel? GroupedAttributeParameters,

    EquatableList<Location> Locations
)
{
    public static MethodModel Create(IMethodSymbol method)
    {
        var attribute = method.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::EndpointGenerator.EndpointGroupBuilderAttribute");
        var groupedAttributeParameters = attribute != null
            ? new GroupedAttributeParametersModel(attribute.ConstructorArguments[0].Value?.ToString() ?? string.Empty)
            : null;

        return new(
            StaticCall: $"{method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{method.Name}",
            ErrorName: method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat),

            IsStatic: method.IsStatic,
            DeclaredAccessibility: method.DeclaredAccessibility,

            ParameterCount: method.Parameters.Length,
            FirstParameterType: method.Parameters.Length > 0
                ? method.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                : null,

            ContainingType: new(
                Name: method.ContainingType.Name,
                Tag: !method.ContainingType.ContainingNamespace.IsGlobalNamespace
                    ? method.ContainingType.ContainingNamespace.ToString()
                    : null
            ),

            GroupedAttributeParameters: groupedAttributeParameters,

            Locations: new(method.Locations)
        );
    }
}
