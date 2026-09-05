using Chatly.Desktop.Abstraction.Authentication;
using Chatly.Desktop.Abstraction.Settings;
using Chatly.Desktop.Options;
using Chatly.Desktop.Services.Api.Refit.Abstraction;
using Chatly.Desktop.Services.Api.Refit.DelegatingHandlers;
using Chatly.Desktop.Services.Authentication.Storage;
using Chatly.Desktop.Services.Authentication.Storage.MacOs;
using Chatly.Desktop.Services.Settings.DirectoryProviders;
using Chatly.Generated;
using Chatly.Shared.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Chatly.Desktop.DependencyInjection;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddDesktop(IConfiguration configuration)
        {
            services.AddSingleton(configuration);

            services.AddRefitGeneratedClient<IChatlyApi>()
                .ConfigureHttpClient(client =>
                {
                    var options = configuration.GetOption<WebApiClientOption>();
                    client.BaseAddress = options.BaseAddress;
                    client.Timeout = TimeSpan.FromSeconds(options.TimeoutInSeconds);
                })
                .AddHttpMessageHandler<BearerDelegatingHandler>();

            services.AddOsSpecificServices();

            services.AddGeneratedOptions();
            services.AddGeneratedServices();

            return services;
        }

        private void AddOsSpecificServices()
        {
            if (OperatingSystem.IsMacOS())
            {
                services.AddSingleton<ISecureTokenStore, MacOsSecureTokenStore>();
                services.AddSingleton<ISettingsDirectoryProvider, MacOsSettingsDirectoryProvider>();
            }
            else if (OperatingSystem.IsWindows())
            {
                services.AddSingleton<ISettingsDirectoryProvider, WindowsSettingsDirectoryProvider>();
                services.AddSingleton<ISecureTokenStore, WindowsSecureTokenStore>();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }
    }
}