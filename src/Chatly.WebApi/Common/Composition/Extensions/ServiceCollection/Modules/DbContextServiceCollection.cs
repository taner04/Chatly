using Chatly.ServiceDefaults;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Chatly.WebApi.Common.Composition.Extensions.ServiceCollection.Modules;

internal static class DbContextServiceCollection
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddChatlyDbContext(WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString(AppHostConstants.DatabaseConnectionName);
            ArgumentNullException.ThrowIfNull(connectionString);

            services.AddDbContext<ChatlyDbContext>((serviceProvider, options) =>
            {
                options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());

                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }

                options.UseNpgsql(connectionString);
            });

            return services;
        }
    }
}