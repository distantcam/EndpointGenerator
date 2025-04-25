using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace EndpointGenerator;

public sealed partial class EndpointBuilderSourceGenerator : ISourceGenerator
{
    private sealed class SyntaxContextReceiver(CancellationToken cancellationToken) : ISyntaxContextReceiver
    {
        public List<MethodModel>? BuilderMethods { get; private set; }
        public List<MethodModel>? GroupBuilderMethods { get; private set; }

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            IMethodSymbol? method;
            if (GeneratorUtilities.IsMethodDeclarationWithAttributes(context.Node, cancellationToken)
                && (method = GeneratorUtilities.GetSymbol<IMethodSymbol>(context, cancellationToken)) != null)
            {
                if (GeneratorUtilities.HasAttribute(method, AttributeNames.EndpointBuilder))
                    (BuilderMethods ??= []).Add(MethodModel.Create(method));
                if (GeneratorUtilities.HasAttribute(method, AttributeNames.EndpointGroupBuilder))
                    (GroupBuilderMethods ??= []).Add(MethodModel.Create(method));
            }
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(static () =>
            new SyntaxContextReceiver(CancellationToken.None));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not SyntaxContextReceiver receiver ||
            (receiver.BuilderMethods == null && receiver.GroupBuilderMethods == null))
            return;

        var assemblyName = context.Compilation.AssemblyName;

        var methods = (
            receiver.BuilderMethods?.ToImmutableArray() ?? ImmutableArray<MethodModel>.Empty,
            receiver.GroupBuilderMethods?.ToImmutableArray() ?? ImmutableArray<MethodModel>.Empty
        );

        Emitter.GenerateSource(context, (methods, assemblyName));
    }
}
