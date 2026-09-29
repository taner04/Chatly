namespace Chatly.SourceGenerator.UnitTests.Tests.DependencyInjection.Infrastructure;

internal sealed record GeneratorTestResult(
    string GeneratedSource,
    IReadOnlyList<Diagnostic> GeneratorDiagnostics,
    IReadOnlyList<Diagnostic> CompilationDiagnostics);