namespace Chatly.SourceGenerator.Generators.Services;

[Generator(LanguageNames.CSharp)]
public sealed class ServiceRegistrationGenerator : IIncrementalGenerator
{
    private const string SingletonServiceAttributeMetadataName =
        "Chatly.DependencyInjection.SingletonServiceAttribute";

    private const string ScopedServiceAttributeMetadataName =
        "Chatly.DependencyInjection.ScopedServiceAttribute";

    private const string TransientServiceAttributeMetadataName =
        "Chatly.DependencyInjection.TransientServiceAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static output =>
            output.AddSource(
                "Chatly.DependencyInjection.ServiceAttributes.g.cs",
                SourceText.From(ServiceAttributesSource.Text, Encoding.UTF8)));

        var singletonServices = FindServices(
            context,
            SingletonServiceAttributeMetadataName,
            ServiceRegistrationLifetime.Singleton);
        var scopedServices = FindServices(
            context,
            ScopedServiceAttributeMetadataName,
            ServiceRegistrationLifetime.Scoped);
        var transientServices = FindServices(
            context,
            TransientServiceAttributeMetadataName,
            ServiceRegistrationLifetime.Transient);

        var services = singletonServices.Collect()
            .Combine(scopedServices.Collect())
            .Combine(transientServices.Collect());

        context.RegisterSourceOutput(services, static (output, discoveredServices) =>
        {
            var cancellationToken = output.CancellationToken;
            cancellationToken.ThrowIfCancellationRequested();

            var candidates = discoveredServices.Left.Left
                .Concat(discoveredServices.Left.Right)
                .Concat(discoveredServices.Right)
                .OrderBy(static candidate => candidate.Registrations.IsEmpty
                    ? candidate.Diagnostics[0].Location.Path
                    : candidate.Registrations[0].Location.Path, StringComparer.Ordinal)
                .ToImmutableArray();

            var registrations = ImmutableArray.CreateBuilder<ServiceRegistrationCandidate>();
            foreach (var candidate in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var diagnostic in candidate.Diagnostics)
                {
                    output.ReportDiagnostic(diagnostic.ToDiagnostic());
                }

                registrations.AddRange(candidate.Registrations);
            }

            var validRegistrations = ValidateDuplicates(registrations.ToImmutable(), output);
            var source = DependencyInjectionTemplate.Render(validRegistrations, cancellationToken);
            output.AddSource(
                "Chatly.DependencyInjection.Services.g.cs",
                SourceText.From(source, Encoding.UTF8));
        });
    }

    private static IncrementalValuesProvider<ServiceCandidate> FindServices(
        IncrementalGeneratorInitializationContext context,
        string attributeMetadataName,
        ServiceRegistrationLifetime lifetime)
    {
        return context.SyntaxProvider.ForAttributeWithMetadataName(
            attributeMetadataName,
            static (node, _) => node is ClassDeclarationSyntax,
            (attributeContext, cancellationToken) => CreateCandidate(
                attributeContext,
                lifetime,
                cancellationToken));
    }

    private static ServiceCandidate CreateCandidate(
        GeneratorAttributeSyntaxContext attributeContext,
        ServiceRegistrationLifetime lifetime,
        CancellationToken cancellationToken)
    {
        var implementationType = (INamedTypeSymbol)attributeContext.TargetSymbol;
        var implementationTypeName = GetRegistrationTypeName(implementationType);
        var registrations = ImmutableArray.CreateBuilder<ServiceRegistrationCandidate>();
        var diagnostics = ImmutableArray.CreateBuilder<ServiceDiagnostic>();

        foreach (var attribute in attributeContext.Attributes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var location = SourceLocation.From(
                attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation()
                ?? implementationType.Locations.FirstOrDefault()
                ?? Location.None);

            if (implementationType.IsAbstract)
            {
                diagnostics.Add(new ServiceDiagnostic(
                    ServiceDiagnosticKind.AbstractImplementation,
                    location,
                    implementationTypeName));
                continue;
            }

            var serviceType = attribute.ConstructorArguments.Length > 0
                ? attribute.ConstructorArguments[0].Value as INamedTypeSymbol
                : null;
            var asSelf = attribute.ConstructorArguments.Length > 1
                         && attribute.ConstructorArguments[1].Value is true;

            if (asSelf && serviceType is null)
            {
                diagnostics.Add(new ServiceDiagnostic(
                    ServiceDiagnosticKind.RedundantAsSelf,
                    location,
                    implementationTypeName));
                continue;
            }

            if (serviceType is not null && !IsCompatibleServiceType(implementationType, serviceType))
            {
                diagnostics.Add(new ServiceDiagnostic(
                    ServiceDiagnosticKind.IncompatibleServiceType,
                    location,
                    GetRegistrationTypeName(serviceType),
                    implementationTypeName));
                continue;
            }

            var serviceTypeName = serviceType is null
                ? implementationTypeName
                : GetRegistrationTypeName(serviceType);
            var isOpenGeneric = implementationType.IsGenericType;

            if (serviceType is not null
                && isOpenGeneric
                && implementationType.Arity != serviceType.Arity)
            {
                diagnostics.Add(new ServiceDiagnostic(
                    ServiceDiagnosticKind.GenericArityMismatch,
                    location,
                    serviceTypeName,
                    implementationTypeName));
                continue;
            }

            registrations.Add(new ServiceRegistrationCandidate(
                new ServiceRegistration(
                    implementationTypeName,
                    serviceTypeName,
                    lifetime,
                    isOpenGeneric),
                location));

            if (asSelf)
            {
                registrations.Add(new ServiceRegistrationCandidate(
                    new ServiceRegistration(
                        implementationTypeName,
                        implementationTypeName,
                        lifetime,
                        isOpenGeneric),
                    location));
            }
        }

        return new ServiceCandidate(registrations.ToImmutable(), diagnostics.ToImmutable());
    }

    private static ImmutableArray<ServiceRegistration> ValidateDuplicates(
        ImmutableArray<ServiceRegistrationCandidate> candidates,
        SourceProductionContext output)
    {
        var cancellationToken = output.CancellationToken;
        var validRegistrations = ImmutableArray.CreateBuilder<ServiceRegistration>();

        foreach (var serviceGroup in candidates
                     .OrderBy(static candidate => candidate.Registration.ImplementationTypeName, StringComparer.Ordinal)
                     .ThenBy(static candidate => candidate.Registration.ServiceTypeName, StringComparer.Ordinal)
                     .ThenBy(static candidate => candidate.Location.Path, StringComparer.Ordinal)
                     .ThenBy(static candidate => candidate.Location.SpanStart)
                     .GroupBy(static candidate => (
                         candidate.Registration.ImplementationTypeName,
                         candidate.Registration.ServiceTypeName)))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var registrations = serviceGroup.ToImmutableArray();
            var lifetimes = registrations
                .Select(static candidate => candidate.Registration.Lifetime)
                .Distinct()
                .OrderBy(static lifetime => lifetime)
                .ToImmutableArray();

            if (lifetimes.Length > 1)
            {
                var conflict = registrations.First(candidate =>
                    candidate.Registration.Lifetime != registrations[0].Registration.Lifetime);
                output.ReportDiagnostic(new ServiceDiagnostic(
                    ServiceDiagnosticKind.ConflictingLifetimes,
                    conflict.Location,
                    conflict.Registration.ServiceTypeName,
                    conflict.Registration.ImplementationTypeName,
                    lifetimes[0].ToString(),
                    lifetimes[1].ToString()).ToDiagnostic());
                continue;
            }

            validRegistrations.Add(registrations[0].Registration);
            foreach (var duplicate in registrations.Skip(1))
            {
                cancellationToken.ThrowIfCancellationRequested();
                output.ReportDiagnostic(new ServiceDiagnostic(
                    ServiceDiagnosticKind.DuplicateRegistration,
                    duplicate.Location,
                    duplicate.Registration.ServiceTypeName,
                    duplicate.Registration.ImplementationTypeName,
                    duplicate.Registration.Lifetime.ToString()).ToDiagnostic());
            }
        }

        return validRegistrations.ToImmutable();
    }

    private static bool IsCompatibleServiceType(INamedTypeSymbol implementationType, INamedTypeSymbol serviceType)
    {
        if (MatchesServiceType(implementationType, serviceType))
        {
            return true;
        }

        for (var baseType = implementationType.BaseType; baseType is not null; baseType = baseType.BaseType)
        {
            if (MatchesServiceType(baseType, serviceType))
            {
                return true;
            }
        }

        return implementationType.AllInterfaces.Any(interfaceType =>
            MatchesServiceType(interfaceType, serviceType));
    }

    private static string GetRegistrationTypeName(INamedTypeSymbol type)
    {
        var registrationType = IsOpenGenericType(type)
            ? type.OriginalDefinition.ConstructUnboundGenericType()
            : type;
        return registrationType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    private static bool MatchesServiceType(INamedTypeSymbol candidate, INamedTypeSymbol serviceType) =>
        IsOpenGenericType(serviceType)
            ? SymbolEqualityComparer.Default.Equals(
                candidate.OriginalDefinition,
                serviceType.OriginalDefinition)
            : SymbolEqualityComparer.Default.Equals(candidate, serviceType);

    private static bool IsOpenGenericType(INamedTypeSymbol type)
    {
        return type.IsUnboundGenericType
               || type.TypeArguments.Any(static argument => argument.TypeKind == TypeKind.TypeParameter);
    }
}