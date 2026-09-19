namespace Chatly.SourceGenerator.Generators.Services;

internal static class ServiceDiagnostics
{
    private const string Category = "Chatly.DependencyInjection";

    private static readonly DiagnosticDescriptor AbstractImplementation = new(
        "CHATLYDI001",
        "Service implementation is abstract",
        "Service implementation '{0}' must not be abstract",
        Category,
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor IncompatibleServiceType = new(
        "CHATLYDI002",
        "Service type is incompatible",
        "Service type '{0}' is not the implementation type, a base class, or an implemented interface of '{1}'",
        Category,
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor DuplicateRegistration = new(
        "CHATLYDI003",
        "Duplicate service registration",
        "Service type '{0}' with implementation '{1}' is registered more than once with lifetime '{2}'",
        Category,
        DiagnosticSeverity.Warning,
        true);

    private static readonly DiagnosticDescriptor ConflictingLifetimes = new(
        "CHATLYDI004",
        "Service registration has conflicting lifetimes",
        "Service type '{0}' with implementation '{1}' has conflicting lifetimes '{2}' and '{3}'",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor RedundantAsSelf = new(
        "CHATLYDI005",
        "asSelf requires an explicit service type",
        "'asSelf: true' requires an explicit service type for implementation '{0}'",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor GenericArityMismatch = new(
        "CHATLYDI006",
        "Open generic service registration has incompatible arity",
        "Open generic service type '{0}' and implementation '{1}' must have matching generic arity",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static DiagnosticDescriptor GetDescriptor(ServiceDiagnosticKind kind) => kind switch
    {
        ServiceDiagnosticKind.AbstractImplementation => AbstractImplementation,
        ServiceDiagnosticKind.IncompatibleServiceType => IncompatibleServiceType,
        ServiceDiagnosticKind.DuplicateRegistration => DuplicateRegistration,
        ServiceDiagnosticKind.ConflictingLifetimes => ConflictingLifetimes,
        ServiceDiagnosticKind.RedundantAsSelf => RedundantAsSelf,
        ServiceDiagnosticKind.GenericArityMismatch => GenericArityMismatch,
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };
}