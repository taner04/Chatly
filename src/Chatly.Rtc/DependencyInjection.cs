using Chatly.Generated;
using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Rtc;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddChatlyRtc()
        {
            services.AddGeneratedOptions();
            services.AddGeneratedServices();

            return services;
        }
    }
}