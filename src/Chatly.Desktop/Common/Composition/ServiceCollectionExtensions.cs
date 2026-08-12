using System;
using Chatly.Contracts.Extensions;
using Chatly.Desktop.Common.Api;
using Chatly.Desktop.Common.Options;
using Chatly.Desktop.Features.Authentication.Abstractions;
using Chatly.Desktop.Features.Authentication.Services;
using Chatly.Desktop.Features.Home.ViewModels;
using Chatly.Desktop.Features.Home.Views;
using Chatly.Desktop.Features.Onboarding.ViewModels;
using Chatly.Desktop.Features.Shell.ViewModels;
using Chatly.Desktop.Features.Shell.Views;
using Chatly.Desktop.Features.Startup.ViewModels;
using Chatly.Desktop.Features.Startup.Views;
using Chatly.Desktop.Features.Startup.Services;
using Chatly.Desktop.Features.Onboarding.Views;
using Chatly.Desktop.Features.Users;
using Chatly.Desktop.Features.Users.Api;
using Chatly.Desktop.Infrastructure.Authentication.Storage;
using Chatly.Desktop.Infrastructure.Navigation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Chatly.Desktop.Common.Composition;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddDesktop(IConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddOption<Auth0Option>(configuration);

            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<SplashScreenWindow>();
            services.AddSingleton<SplashScreenViewModel>();
            services.AddSingleton<HomePage>();
            services.AddSingleton<HomePageViewModel>();
            services.AddSingleton<OnboardingPage>();
            services.AddSingleton<OnboardingPageViewModel>();

            services.AddSingleton<NavigationService>();
            services.AddSingleton<PageService>();
            services.AddSingleton<AuthenticationService>();
            services.AddSingleton<SessionService>();
            services.AddSingleton<UserSessionContext>();
            services.AddSingleton<ApplicationStartupService>();

            services.AddTransient<BearerDelegatingHandler>();
            services.AddRefitGeneratedClient<IUserEndpoint>()
                .ConfigureHttpClient(client =>
                {
                    var options = configuration.GetOption<WebApiClientOption>();
                    client.BaseAddress = options.BaseAddress;
                    client.Timeout = TimeSpan.FromSeconds(options.TimeoutInSeconds);
                })
                .AddHttpMessageHandler<BearerDelegatingHandler>();

            services.AddTransient<UserWebService>();
            services.AddSecureTokenStore();

            return services;
        }

        private void AddSecureTokenStore()
        {
            if (OperatingSystem.IsMacOS())
            {
                services.AddSingleton<ISecureTokenStore, MacOsSecureTokenStore>();
                return;
            }

            if (OperatingSystem.IsWindows())
            {
                services.AddSingleton<ISecureTokenStore, WindowsSecureTokenStore>();
                return;
            }

            throw new PlatformNotSupportedException();
        }
    }
}
