using Microsoft.CodeAnalysis;

namespace EndpointGenerator;

public sealed partial class EndpointBuilderSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider
            .Select(static (c, _) => c.AssemblyName)
            .WithTrackingName(TrackingNames.AssemblyName);

        var builderMethods = context.SyntaxProvider.ForAttributeWithMetadataName(
            "EndpointGenerator.EndpointBuilderAttribute",
            Parser.IsMethodDeclaration,
            static (c, ct) => (IMethodSymbol)c.TargetSymbol)
        .WithTrackingName(TrackingNames.BuilderModels)
        .Collect();

        var groupBuilderMethods = context.SyntaxProvider.ForAttributeWithMetadataName(
            "EndpointGenerator.EndpointGroupBuilderAttribute",
            Parser.IsMethodDeclaration,
            static (c, ct) => (IMethodSymbol)c.TargetSymbol)
        .WithTrackingName(TrackingNames.GroupBuilderModels)
        .Collect();

        context.RegisterSourceOutput(
            builderMethods.Combine(groupBuilderMethods).Combine(assemblyName),
            Emitter.GenerateSource);
    }
}
