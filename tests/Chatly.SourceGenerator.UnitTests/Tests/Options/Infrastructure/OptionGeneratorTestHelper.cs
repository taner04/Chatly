using Chatly.SourceGenerator.Generators.Options;
using Microsoft.Extensions.Options;

namespace Chatly.SourceGenerator.UnitTests.Tests.Options.Infrastructure;

internal static class OptionGeneratorTestHelper
{
    private const string OptionAttributeSource = """
                                                 namespace Chatly.Shared.Attributes;

                                                 [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
                                                 public sealed class OptionAttribute(string? name = null) : System.Attribute;
                                                 """;

    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Latest);

    private static readonly MetadataReference[] References =
    [
        .. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
        .Split(Path.PathSeparator)
        .Select(static path => MetadataReference.CreateFromFile(path)),

        MetadataReference.CreateFromFile(typeof(IServiceCollection).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(OptionsBuilder<>).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(OptionsBuilderConfigurationExtensions).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(OptionsBuilderDataAnnotationsExtensions).Assembly.Location)
    ];

    internal static string Run(string source)
    {
        var compilation = CSharpCompilation.Create(
            "OptionGeneratorTests",
            [
                CSharpSyntaxTree.ParseText(OptionAttributeSource, ParseOptions, "OptionAttribute.cs"),
                CSharpSyntaxTree.ParseText(source, ParseOptions, "TestSource.cs")
            ],
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var driver = CSharpGeneratorDriver
            .Create([new OptionGenerator().AsSourceGenerator()], parseOptions: ParseOptions)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        outputCompilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .Should().BeEmpty();

        return driver.GetRunResult().Results
            .Single()
            .GeneratedSources
            .Single(static generated => generated.HintName == "Chatly.Options.g.cs")
            .SourceText
            .ToString();
    }
}