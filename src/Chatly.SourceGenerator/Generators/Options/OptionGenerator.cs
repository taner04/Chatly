namespace Chatly.SourceGenerator.Generators.Options;

[Generator(LanguageNames.CSharp)]
public sealed class OptionGenerator : IIncrementalGenerator
{
    private const string OptionAttributeMetadataName = "Chatly.Shared.Attributes.OptionAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var options = context.SyntaxProvider.ForAttributeWithMetadataName(
            OptionAttributeMetadataName,
            static (node, _) => node is ClassDeclarationSyntax,
            static (attributeContext, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var type = (INamedTypeSymbol)attributeContext.TargetSymbol;
                var attribute = attributeContext.Attributes[0];
                var configuredName = attribute.ConstructorArguments.Length > 0
                    ? attribute.ConstructorArguments[0].Value as string
                    : null;

                return (
                    TypeName: type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    SectionName: string.IsNullOrWhiteSpace(configuredName) ? type.Name : configuredName);
            });

        context.RegisterSourceOutput(options.Collect(),
            static (output, discoveredOptions) =>
            {
                var source = OptionTemplate.Render(discoveredOptions, output.CancellationToken);
                output.AddSource(
                    "Chatly.Options.g.cs",
                    SourceText.From(source, Encoding.UTF8));
            });
    }
}