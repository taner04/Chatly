using Azure.Storage.Blobs;
using Chatly.Generated;
using Chatly.ServiceDefaults;
using Chatly.WebApi.Common.Composition.Extensions.ServiceCollection.Modules;

namespace Chatly.WebApi.Common.Composition.Extensions.ServiceCollection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection RegisterChatlyServices(WebApplicationBuilder builder)
        {
            services.AddSingleton(_ =>
            {
                var connectionString = builder.Configuration.GetConnectionString(
                                           AppHostConstants.BlobServiceConnectionName)
                                       ?? throw new InvalidOperationException(
                                           $"Connection string '{AppHostConstants.BlobServiceConnectionName}' is missing.");

                return new BlobServiceClient(connectionString);
            });
            services
                .AddGeneratedOptions(builder.Configuration)
                .AddGeneratedServices()
                .AddChatlyAuthentication(builder.Configuration)
                .AddChatlyDbContext(builder)
                .AddChatlyApplicationServices();

            return services;
        }
    }
}