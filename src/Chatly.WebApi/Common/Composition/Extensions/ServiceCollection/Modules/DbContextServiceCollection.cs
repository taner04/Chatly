using Chatly.ServiceDefaults;
using Chatly.WebApi.Common.Infrastructure.Persistence;
using Chatly.WebApi.Common.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Chatly.WebApi.Common.Composition.Extensions.ServiceCollection.Modules;

internal static class DbContextServiceCollection
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddChatlyDbContext(WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString(AppHostConstants.Database);
            ArgumentNullException.ThrowIfNull(connectionString);

            services.AddScoped<ISaveChangesInterceptor, AuditableInterceptor>();
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