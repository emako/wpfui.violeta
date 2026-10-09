using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Wpf.Ui.DependencyPropertyGenerator;

[Generator]
internal sealed class DisableDpiAwarenessGenerator : IIncrementalGenerator
{
    private const string TargetMethod = "Wpf.Ui.Violeta.Win32.DpiAware.DisableDpiAwareness";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<bool> requested = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is InvocationExpressionSyntax,
                static (syntaxContext, _) => IsDisableDpiAwarenessCall(syntaxContext))
            .Where(static called => called)
            .Collect()
            .Select(static (calls, _) => calls.Length > 0);

        context.RegisterSourceOutput(requested, static (source, enabled) =>
        {
            if (!enabled)
            {
                return;
            }

            source.AddSource("DisableDpiAwareness.g.cs", """
                [assembly: System.Windows.Media.DisableDpiAwareness]
                """);
        });
    }

    private static bool IsDisableDpiAwarenessCall(GeneratorSyntaxContext syntaxContext)
    {
        if (syntaxContext.Node is not InvocationExpressionSyntax invocation)
        {
            return false;
        }

        if (syntaxContext.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method)
        {
            return false;
        }

        return method.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)
            .StartsWith(TargetMethod, System.StringComparison.Ordinal);
    }
}
