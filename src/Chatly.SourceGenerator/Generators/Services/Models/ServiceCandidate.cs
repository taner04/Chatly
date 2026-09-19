namespace Chatly.SourceGenerator.Generators.Services.Models;

internal sealed class ServiceCandidate : IEquatable<ServiceCandidate>
{
    internal ServiceCandidate(
        ImmutableArray<ServiceRegistrationCandidate> registrations,
        ImmutableArray<ServiceDiagnostic> diagnostics)
    {
        Registrations = registrations;
        Diagnostics = diagnostics;
    }

    internal ImmutableArray<ServiceRegistrationCandidate> Registrations { get; }
    internal ImmutableArray<ServiceDiagnostic> Diagnostics { get; }

    public bool Equals(ServiceCandidate? other) =>
        other is not null
        && Registrations.SequenceEqual(other.Registrations)
        && Diagnostics.SequenceEqual(other.Diagnostics);

    public override bool Equals(object? obj) => Equals(obj as ServiceCandidate);

    public override int GetHashCode()
    {
        var hashCode = 17;

        foreach (var registration in Registrations)
        {
            hashCode = hashCode * 31 + registration.GetHashCode();
        }

        foreach (var diagnostic in Diagnostics)
        {
            hashCode = hashCode * 31 + diagnostic.GetHashCode();
        }

        return hashCode;
    }
}