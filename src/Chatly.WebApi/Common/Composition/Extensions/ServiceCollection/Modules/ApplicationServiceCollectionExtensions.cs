using Azure.Storage.Blobs;
using Chatly.ServiceDefaults;
using Chatly.WebApi.Common.Behaviours;
using Chatly.WebApi.Common.Infrastructure.BackgroundServices;
using Chatly.WebApi.Features.Hubs.HubFilters;
using Microsoft.AspNetCore.SignalR;

namespace Chatly.WebApi.Common.Composition.Extensions.ServiceCollection.Modules;

internal static class ApplicationServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddChatlyApplicationServices(WebApplicationBuilder builder)
        {
            services.AddSingleton(_ =>
            {
                var connectionString = builder.Configuration.GetConnectionString(
                                           AppHostConstants.BlobServiceConnectionName)
                                       ?? throw new InvalidOperationException(
                                           $"Connection string '{AppHostConstants.BlobServiceConnectionName}' is missing.");

                return new BlobServiceClient(connectionString);
            });

            services.AddHostedService<PresenceCheckerBackgroundService>();

            services.AddMediator(options =>
            {
                options.ServiceLifetime = ServiceLifetime.Scoped;
                options.GenerateTypesAsInternal = true;
                options.PipelineBehaviors =
                [
                    typeof(LoggingBehaviour<,>),
                    typeof(DeviceSessionBehaviour<,>),
                    typeof(FluentValidationBehaviour<,>)
                ];
            });

            services.AddSingleton<RateLimiterHubFilter>();
            services.AddSignalR(options =>
            {
                options.AddFilter<RateLimiterHubFilter>();
                options.AddFilter<LoggingHubFilter>();
            });
            services.AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes: true);

            return services;
        }
    }
}