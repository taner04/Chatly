using Chatly.SourceGenerator.Generators.Services.Enums;

namespace Chatly.SourceGenerator.Generators.Services.Diagnostics;

internal readonly record struct ServiceDiagnostic(
    ServiceDiagnosticKind Kind,
    SourceLocation Location,
    string Argument0,
    string? Argument1 = null,
    string? Argument2 = null,
    string? Argument3 = null)
{
    internal Diagnostic ToDiagnostic()
    {
        var arguments = new object?[] { Argument0, Argument1, Argument2, Argument3 };
        return Diagnostic.Create(ServiceDiagnostics.GetDescriptor(Kind), Location.ToLocation(), arguments);
    }
}