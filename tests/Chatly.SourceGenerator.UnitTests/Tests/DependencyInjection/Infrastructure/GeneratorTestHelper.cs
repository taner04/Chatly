namespace Chatly.SourceGenerator.UnitTests.Tests.DependencyInjection.Infrastructure;

internal static class GeneratorTestHelper
{
    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Latest);

    private static readonly MetadataReference[] References =
    [
        .. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
        .Split(Path.PathSeparator)
        .Select(static path => MetadataReference.CreateFromFile(path)),

        MetadataReference.CreateFromFile(
            typeof(IServiceCollection).Assembly.Location)
    ];

    internal static GeneratorTestResult Run(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, ParseOptions, "TestSource.cs");
        var compilation = CSharpCompilation.Create(
            "GeneratorTests",
            [syntaxTree],
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new ServiceRegistrationGenerator().AsSourceGenerator()],
            parseOptions: ParseOptions);

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out _);

        var runResult = driver.GetRunResult();
        var generatedSource = runResult.Results
            .Single()
            .GeneratedSources
            .Single(static source => source.HintName == "Chatly.DependencyInjection.Services.g.cs")
            .SourceText
            .ToString();

        return new GeneratorTestResult(
            generatedSource,
            runResult.Diagnostics,
            outputCompilation.GetDiagnostics());
    }
}