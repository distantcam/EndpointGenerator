using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EndpointGenerator;

public partial class EndpointBuilderSourceGenerator
{
    private static class Parser
    {
        public const string EndpointBuilderAttributeFullName = "EndpointGenerator.EndpointBuilderAttribute";
        public const string EndpointGroupBuilderAttributeFullName = "EndpointGenerator.EndpointGroupBuilderAttribute";

        public static bool IsMethodDeclaration(SyntaxNode node, CancellationToken cancellationToken)
            => node is MethodDeclarationSyntax { AttributeLists.Count: > 0 };

        public static IMethodSymbol? GetBuilderMarkedMethodSymbol(
            GeneratorSyntaxContext context,
            CancellationToken cancellationToken)
        {
            if (!MemberHasAttribute(EndpointBuilderAttributeFullName, context, cancellationToken))
                return null;
            return context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken) as IMethodSymbol;
        }

        public static IMethodSymbol? GetGroupBuilderMarkedMethodSymbol(
            GeneratorSyntaxContext context,
            CancellationToken cancellationToken)
        {
            if (!MemberHasAttribute(EndpointGroupBuilderAttributeFullName, context, cancellationToken))
                return null;
            return context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken) as IMethodSymbol;
        }

        private static bool MemberHasAttribute(
            string attribute,
            GeneratorSyntaxContext context,
            CancellationToken cancellationToken)
        {
            foreach (var attributeListSyntax in ((MemberDeclarationSyntax)context.Node).AttributeLists)
                foreach (var attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (context.SemanticModel.GetSymbolInfo(attributeSyntax, cancellationToken).Symbol is not IMethodSymbol attributeSymbol) continue;

                    var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                    var fullName = attributeContainingTypeSymbol.ToDisplayString();

                    if (fullName != attribute) continue;

                    return true;
                }
            return false;
        }
    }
}
