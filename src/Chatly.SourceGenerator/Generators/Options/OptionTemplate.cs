using System.Text;
using Chatly.SourceGenerator.Templates;
using Microsoft.CodeAnalysis.CSharp;

namespace Chatly.SourceGenerator.Generators.Options;

internal static class OptionTemplate
{
    private const string RegistrationPlaceholder = "        {{ optionRegistrations }}";

    internal static string Render(IEnumerable<(string TypeName, string SectionName)> options)
    {
        var registrations = new StringBuilder();

        foreach (var option in options.OrderBy(static option => option.TypeName, StringComparer.Ordinal))
        {
            var sectionName = SymbolDisplay.FormatLiteral(option.SectionName, true);

            registrations
                .Append(
                    "        global::Microsoft.Extensions.DependencyInjection.OptionsServiceCollectionExtensions.AddOptions<")
                .Append(option.TypeName)
                .AppendLine(">(services)")
                .Append("            .BindConfiguration(")
                .Append(sectionName)
                .AppendLine(")")
                .AppendLine("            .ValidateDataAnnotations()")
                .AppendLine("            .ValidateOnStart();");
        }

        return TemplateLoader.Load("Options.cs.template").Replace(RegistrationPlaceholder, registrations.ToString().TrimEnd());
    }
}
