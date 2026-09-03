using Chatly.SourceGenerator.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Chatly.SourceGenerator;

[Generator]
public sealed class Generator : IIncrementalGenerator
{
    private const string OptionAttributeMetadataName = "Chatly.Shared.Attributes.OptionAttribute";

    private const string SingletonServiceAttributeMetadataName =
        "Chatly.Shared.Attributes.Service.SingletonServiceAttribute";

    private const string ScopedServiceAttributeMetadataName =
        "Chatly.Shared.Attributes.Service.ScopedServiceAttribute";

    private const string TransientServiceAttributeMetadataName =
        "Chatly.Shared.Attributes.Service.TransientServiceAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var options = context.SyntaxProvider.ForAttributeWithMetadataName(
            OptionAttributeMetadataName,
            static (node, _) => node is ClassDeclarationSyntax,
            static (attributeContext, _) =>
            {
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
                output.AddSource("Options.g.cs", OptionTemplate.Render(discoveredOptions));
            });

        var services = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                static (syntaxContext, _) =>
                {
                    var implementationType = (INamedTypeSymbol)syntaxContext.SemanticModel.GetDeclaredSymbol(
                        (ClassDeclarationSyntax)syntaxContext.Node)!;
                    var implementationTypeName = implementationType.ToDisplayString(
                        SymbolDisplayFormat.FullyQualifiedFormat);
                    var registrations =
                        new List<(string ImplementationTypeName, string ServiceTypeName, int Lifetime)>();

                    foreach (var attribute in implementationType.GetAttributes())
                    {
                        var lifetime = attribute.AttributeClass?.ToDisplayString() switch
                        {
                            SingletonServiceAttributeMetadataName => 0,
                            ScopedServiceAttributeMetadataName => 1,
                            TransientServiceAttributeMetadataName => 2,
                            _ => -1
                        };

                        if (lifetime < 0)
                        {
                            continue;
                        }

                        var serviceType = attribute.ConstructorArguments.Length > 0
                            ? attribute.ConstructorArguments[0].Value as ITypeSymbol
                            : null;
                        var serviceTypeName = serviceType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                                              ?? implementationTypeName;
                        var asSelf = attribute.ConstructorArguments.Length > 1
                                     && attribute.ConstructorArguments[1].Value is true;

                        registrations.Add((implementationTypeName, serviceTypeName, lifetime));

                        if (asSelf && serviceType is not null)
                        {
                            registrations.Add((implementationTypeName, implementationTypeName, lifetime));
                        }
                    }

                    return registrations.ToArray();
                })
            .Where(static registrations => registrations.Length > 0);

        context.RegisterSourceOutput(services.Collect(), static (output, discoveredServices) =>
        {
            output.AddSource(
                "Services.g.cs",
                DependencyInjectionTemplate.Render(discoveredServices.SelectMany(static services => services)));
        });
    }
}