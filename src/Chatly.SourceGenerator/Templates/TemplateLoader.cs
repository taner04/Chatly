namespace Chatly.SourceGenerator.Templates;

internal static class TemplateLoader
{
    private const string ResourcePrefix = "Chatly.SourceGenerator.Templates.";

    internal static string Load(string templateName)
    {
        var resourceName = ResourcePrefix + templateName;
        using var stream = typeof(TemplateLoader).Assembly.GetManifestResourceStream(resourceName) ??
                           throw new InvalidOperationException($"Embedded template '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}