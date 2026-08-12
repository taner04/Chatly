using Chatly.WebApi.Common.Composition.Extensions.ServiceCollection.Modules;

namespace Chatly.WebApi.Common.Composition.Extensions.ServiceCollection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection RegisterChatlyServices(WebApplicationBuilder builder)
        {
            services
                .AddChatlyConfiguration(builder.Configuration)
                .AddChatlyAuthentication(builder.Configuration)
                .AddChatlyDbContext(builder)
                .AddChatlyApplicationServices();

            return services;
        }
    }
}