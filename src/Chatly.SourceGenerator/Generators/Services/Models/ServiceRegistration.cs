using Chatly.SourceGenerator.Generators.Services.Enums;

namespace Chatly.SourceGenerator.Generators.Services.Models;

internal readonly record struct ServiceRegistration(
    string ImplementationTypeName,
    string ServiceTypeName,
    ServiceRegistrationLifetime Lifetime,
    bool IsOpenGeneric);