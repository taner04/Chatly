using Chatly.Generated;
using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Audio;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddChatlyAudio()
        {
            services.AddGeneratedOptions();
            services.AddGeneratedServices();

            return services;
        }
    }
}