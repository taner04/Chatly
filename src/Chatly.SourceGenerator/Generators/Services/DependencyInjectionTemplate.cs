using System.Text;
using Chatly.SourceGenerator.Templates;

namespace Chatly.SourceGenerator.Generators.Services;

internal static class DependencyInjectionTemplate
{
    private const string RegistrationPlaceholder = "        {{ serviceRegistrations }}";

    internal static string Render(
        IEnumerable<(string ImplementationTypeName, string ServiceTypeName, int Lifetime)> services)
    {
        var registrations = new StringBuilder();

        foreach (var service in services
                     .Distinct()
                     .OrderBy(static service => service.ImplementationTypeName, StringComparer.Ordinal)
                     .ThenBy(static service => service.ServiceTypeName, StringComparer.Ordinal))
        {
            var registrationMethod = service.Lifetime switch
            {
                0 => "AddSingleton",
                1 => "AddScoped",
                2 => "AddTransient",
                _ => throw new ArgumentOutOfRangeException(nameof(services), "Unsupported service lifetime.")
            };

            registrations
                .Append("        services.")
                .Append(registrationMethod)
                .Append('<')
                .Append(service.ServiceTypeName);

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