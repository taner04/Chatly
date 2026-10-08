using Microsoft.Extensions.Configuration;

namespace Chatly.WebApi.IntegrationTests.Infrastructure;

public static class TestSettings
{
    private const string WebApiSection = "WebApi";

    public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("testsettings.json", false)
        .Build();

    public static string PostgresImage => Required("TestContainers:PostgresImage");

    public static string AzuriteImage => Required("TestContainers:AzuriteImage");

    public static string OidcAuthority => Required($"{WebApiSection}:OidcOption:Authority");

    public static string OidcAudience => Required($"{WebApiSection}:OidcOption:Audience");

    public static string LiveKitApiKey => Required($"{WebApiSection}:LiveKitOption:ApiKey");

    public static string LiveKitApiSecret => Required($"{WebApiSection}:LiveKitOption:ApiSecret");

    public static IEnumerable<KeyValuePair<string, string>> WebApiSettings() =>
        Configuration.GetSection(WebApiSection)
            .AsEnumerable(true)
            .Where(setting => setting.Value is not null)
            .Select(setting => new KeyValuePair<string, string>(setting.Key, setting.Value!));

    private static string Required(string key) =>
        Configuration[key] ?? throw new InvalidOperationException($"testsettings.json is missing '{key}'.");
}