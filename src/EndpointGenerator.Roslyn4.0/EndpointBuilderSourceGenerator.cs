using Microsoft.CodeAnalysis;

namespace EndpointGenerator;

public sealed partial class EndpointBuilderSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider
            .Select(static (c, _) => c.AssemblyName);

        var builderMethods = context.SyntaxProvider.CreateSyntaxProvider(
            GeneratorUtilities.IsMethodDeclarationWithAttributes,
            GeneratorUtilities.GetSymbol<IMethodSymbol>)
        .Where(static x => GeneratorUtilities.HasAttribute(x, AttributeNames.EndpointBuilder))
        .Select(static (x, _) => MethodModel.Create(x!))
        .Collect();

        var groupBuilderMethods = context.SyntaxProvider.CreateSyntaxProvider(
            GeneratorUtilities.IsMethodDeclarationWithAttributes,
            GeneratorUtilities.GetSymbol<IMethodSymbol>)
        .Where(static x => GeneratorUtilities.HasAttribute(x, AttributeNames.EndpointGroupBuilder))
        .Select(static (x, _) => MethodModel.Create(x!))
        .Collect();

        context.RegisterSourceOutput(
            builderMethods.Combine(groupBuilderMethods).Combine(assemblyName),
            Emitter.GenerateSource);
    }
}
