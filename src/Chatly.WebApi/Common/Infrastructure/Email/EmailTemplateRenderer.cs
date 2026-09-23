using System.Text.Encodings.Web;

namespace Chatly.WebApi.Common.Infrastructure.Email;

[SingletonService]
internal sealed class EmailTemplateRenderer
{
    public async Task<string> RenderAsync(
        string templateName,
        IReadOnlyDictionary<string, string> values,
        CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Common",
            "Infrastructure",
            "Email",
            "Templates",
            $"{templateName}.html");

        var template = await File.ReadAllTextAsync(
            path,
            cancellationToken);

        if (template is null)
        {
            throw new InvalidOperationException($"Email template '{templateName}' not found.");
        }

        foreach (var (key, value) in values)
        {
            template = template.Replace(
                $"{{{{{key}}}}}",
                HtmlEncoder.Default.Encode(value));
        }

        return template;
    }
}
