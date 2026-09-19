namespace Chatly.SourceGenerator.Generators.Services.Models;

internal readonly record struct ServiceRegistrationCandidate(
    ServiceRegistration Registration,
    SourceLocation Location);