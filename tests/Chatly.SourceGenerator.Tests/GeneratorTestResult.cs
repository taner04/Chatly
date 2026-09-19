namespace Chatly.SourceGenerator.Tests;

internal sealed record GeneratorTestResult(
    string GeneratedSource,
    IReadOnlyList<Diagnostic> GeneratorDiagnostics,
    IReadOnlyList<Diagnostic> CompilationDiagnostics);