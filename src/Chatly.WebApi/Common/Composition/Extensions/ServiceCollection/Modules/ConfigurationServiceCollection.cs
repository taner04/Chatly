using Chatly.Contracts.Extensions;
using Chatly.WebApi.Common.Composition.Options;

namespace Chatly.WebApi.Common.Composition.Extensions.ServiceCollection.Modules;

internal static class ConfigurationServiceCollection
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddChatlyConfiguration(IConfiguration configuration)
        {
            services.AddOption<Auth0Option>(configuration);

            return services;
        }
    }
}