using Chatly.Generated;
using Chatly.WebApi.Common.Composition.Extensions.ServiceCollection.Modules;

namespace Chatly.WebApi.Common.Composition.Extensions.ServiceCollection;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection RegisterChatlyServices(WebApplicationBuilder builder)
        {
            services
                .AddGeneratedOptions()
                .AddGeneratedServices()
                .AddChatlyAuthentication(builder.Configuration)
                .AddChatlyDbContext(builder)
                .AddChatlyApplicationServices(builder)
                .AddChatlyHangfire(builder);

            return services;
        }
    }
}
