using Chatly.Contracts.Extensions;
using Chatly.Desktop.Abstractions.Authentication;
using Chatly.Desktop.Abstractions.Navigation;
using Chatly.Desktop.Models;
using Chatly.Desktop.Options;
using Chatly.Desktop.Services.Api;
using Chatly.Desktop.Services.Api.Refit;
using Chatly.Desktop.Services.Api.Refit.DelegatingHandlers;
using Chatly.Desktop.Services.Api.SignalR;
using Chatly.Desktop.Services.Authentication;
using Chatly.Desktop.Services.Authentication.Storage;
using Chatly.Desktop.Services.Navigation;
using Chatly.Desktop.Services.Startup;
using Chatly.Desktop.ViewModels.Overlays;
using Chatly.Desktop.ViewModels.Pages;
using Chatly.Desktop.ViewModels.Windows;
using Chatly.Desktop.Views.Overlays;
using Chatly.Desktop.Views.Pages;
using Chatly.Desktop.Views.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System;
using Chatly.Desktop.Services.Api.SignalR.NotificationEventStrategies;

namespace Chatly.Desktop.DependencyInjection;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddDesktop(IConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddOption<Auth0Option>(configuration);
            services.AddOption<WebApiClientOption>(configuration);
            
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<SplashScreenWindow>();
            services.AddSingleton<SplashScreenViewModel>();

            services.AddPopup<OnboardingOverlay, OnboardingOverlayViewModel>();

            services.AddSingleton<HomePage>();
            services.AddSingleton<HomePageViewModel>();
            services.AddSingleton<PopupOverlayHostViewModel>();
            services.AddSingleton<UserInfoPage>();
            services.AddSingleton<UserInfoPageViewModel>();

            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<PageService>();
            services.AddSingleton<AuthenticationService>();
            services.AddSingleton<SessionService>();
            services.AddSingleton<UserSessionContext>();
            services.AddSingleton<ApplicationStartupService>();

            services.AddSingleton<NotificationHubDispatcher>();
            services.AddSingleton<NotificationHubHost>();
            
            services.AddNotificationStrategies<NotificationIncomingFriendRequestStrategy>();

            services.AddTransient<BearerDelegatingHandler>();
            services.AddRefitGeneratedClient<IUserEndpoint>()
                .ConfigureHttpClient(client =>
                {
                    var options = configuration.GetOption<WebApiClientOption>();
                    client.BaseAddress = options.BaseAddress;
                    client.Timeout = TimeSpan.FromSeconds(options.TimeoutInSeconds);
                })
                .AddHttpMessageHandler<BearerDelegatingHandler>();

            services.AddRefitGeneratedClient<IFriendRequestEndpoint>()
                .ConfigureHttpClient(client =>
                {
                    var options = configuration.GetOption<WebApiClientOption>();
                    client.BaseAddress = options.BaseAddress;
                    client.Timeout = TimeSpan.FromSeconds(options.TimeoutInSeconds);
                })
                .AddHttpMessageHandler<BearerDelegatingHandler>();

            services.AddTransient<UserWebService>();
            services.AddTransient<FriendRequestWebService>();
            services.AddSecureTokenStore();

            return services;
        }

        private IServiceCollection AddNotificationStrategies<T>() where T : class, INotificationStrategy
        {
            services.AddSingleton<INotificationStrategy, T>();
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
