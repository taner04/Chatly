using Chatly.SourceGenerator.Generators.Services.Enums;

namespace Chatly.SourceGenerator.Generators.Services;

internal static class DependencyInjectionTemplate
{
    private const string RegistrationPlaceholder = "        {{ serviceRegistrations }}";

    internal static string Render(
        IEnumerable<ServiceRegistration> services,
        CancellationToken cancellationToken)
    {
        var registrations = new StringBuilder();

        foreach (var service in services
                     .OrderBy(static service => service.ImplementationTypeName, StringComparer.Ordinal)
                     .ThenBy(static service => service.ServiceTypeName, StringComparer.Ordinal)
                     .ThenBy(static service => service.Lifetime))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var registrationMethod = service.Lifetime switch
            {
                ServiceRegistrationLifetime.Singleton => "AddSingleton",
                ServiceRegistrationLifetime.Scoped => "AddScoped",
                ServiceRegistrationLifetime.Transient => "AddTransient",
                _ => throw new ArgumentOutOfRangeException(nameof(services), "Unsupported service lifetime.")
            };

            registrations.Append("        services.").Append(registrationMethod);

            if (service.IsOpenGeneric)
            {
                registrations
                    .Append("(typeof(")
                    .Append(service.ServiceTypeName)
                    .Append("), typeof(")
                    .Append(service.ImplementationTypeName)
                    .AppendLine("));");
                continue;
            }

            registrations.Append('<').Append(service.ServiceTypeName);

            if (service.ServiceTypeName != service.ImplementationTypeName)
            {
                registrations
                    .Append(", ")
                    .Append(service.ImplementationTypeName);
            }

            registrations.AppendLine(">();");
        }

        return TemplateLoader.Load("Services.cs.template")
            .Replace(RegistrationPlaceholder, registrations.ToString().TrimEnd());
    }
}