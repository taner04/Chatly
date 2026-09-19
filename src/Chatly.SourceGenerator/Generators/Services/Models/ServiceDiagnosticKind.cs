namespace Chatly.SourceGenerator.Generators.Services.Models;

internal enum ServiceDiagnosticKind
{
    AbstractImplementation,
    IncompatibleServiceType,
    DuplicateRegistration,
    ConflictingLifetimes,
    RedundantAsSelf,
    GenericArityMismatch
}