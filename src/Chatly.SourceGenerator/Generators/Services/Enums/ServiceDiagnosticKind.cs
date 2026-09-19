namespace Chatly.SourceGenerator.Generators.Services.Enums;

internal enum ServiceDiagnosticKind
{
    AbstractImplementation,
    IncompatibleServiceType,
    DuplicateRegistration,
    ConflictingLifetimes,
    RedundantAsSelf,
    GenericArityMismatch
}