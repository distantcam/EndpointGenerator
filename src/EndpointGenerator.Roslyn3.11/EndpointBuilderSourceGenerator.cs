using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EndpointGenerator;

public sealed partial class EndpointBuilderSourceGenerator : ISourceGenerator
{
    private sealed class SyntaxContextReceiver(CancellationToken cancellationToken) : ISyntaxContextReceiver
    {
        public List<IMethodSymbol>? BuilderMethods { get; private set; }
        public List<IMethodSymbol>? GroupBuilderMethods { get; private set; }

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is MethodDeclarationSyntax { AttributeLists.Count: > 0 })
            {
                var method = Parser.GetBuilderMarkedMethodSymbol(context, cancellationToken);
                if (method != null)
                    (BuilderMethods ??= []).Add(method);

                method = Parser.GetGroupBuilderMarkedMethodSymbol(context, cancellationToken);
                if (method != null)
                    (GroupBuilderMethods ??= []).Add(method);
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
            receiver.BuilderMethods?.ToImmutableArray() ?? ImmutableArray<IMethodSymbol>.Empty,
            receiver.GroupBuilderMethods?.ToImmutableArray() ?? ImmutableArray<IMethodSymbol>.Empty
        );

        Emitter.GenerateSource(context, (methods, assemblyName));
    }
}
