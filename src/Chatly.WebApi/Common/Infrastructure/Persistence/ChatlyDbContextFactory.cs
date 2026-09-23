using Chatly.ServiceDefaults;
using Microsoft.EntityFrameworkCore.Design;

namespace Chatly.WebApi.Common.Infrastructure.Persistence;

public sealed class ChatlyDbContextFactory : IDesignTimeDbContextFactory<ChatlyDbContext>
{
    public ChatlyDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                          ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true);

        if (!string.IsNullOrWhiteSpace(environment))
        {
            configurationBuilder.AddJsonFile($"appsettings.{environment}.json", true);
        }

        var configuration = configurationBuilder
            .AddEnvironmentVariables()
            .Build();
        var connectionString = configuration.GetConnectionString(
            AppHostConstants.DatabaseConnectionName);

        var optionsBuilder = new DbContextOptionsBuilder<ChatlyDbContext>();
        ChatlyDbContextOptionsConfigurator.Configure(optionsBuilder, connectionString!);

        return new ChatlyDbContext(optionsBuilder.Options);
    }
}