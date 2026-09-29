using Chatly.ServiceDefaults;
using Chatly.WebApi.Common.Infrastructure.Email;
using Chatly.WebApi.IntegrationTests.Infrastructure.Mocks.Jwt;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Chatly.WebApi.IntegrationTests.Infrastructure;

internal sealed class WebApiFactory(string databaseConnectionString, string blobConnectionString)
    : WebApplicationFactory<Program>
{
    public IEmailService EmailService { get; } = Substitute.For<IEmailService>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        var settings = TestSettings.WebApiSettings()
            .Append(new KeyValuePair<string, string>($"ConnectionStrings:{AppHostConstants.DatabaseConnectionName}",
                databaseConnectionString))
            .Append(new KeyValuePair<string, string>($"ConnectionStrings:{AppHostConstants.BlobServiceConnectionName}",
                blobConnectionString))
            .ToList();

        foreach (var (key, value) in settings)
        {
            builder.UseSetting(key, value);
        }

        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(settings.Select(setting =>
                new KeyValuePair<string, string?>(setting.Key, setting.Value))));

        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureTestServices(services =>
        {
            services.AddMockJwtBearerOptions();
            services.RemoveAll<IEmailService>();
            services.AddSingleton(EmailService);
        });
    }
}