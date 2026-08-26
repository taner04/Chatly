using System;
using Chatly.Contracts.Extensions;
using Chatly.Desktop.Abstractions.Authentication;
using Chatly.Desktop.Abstractions.Navigation;
using Chatly.Desktop.Abstractions.Popups;
using Chatly.Desktop.Models;
using Chatly.Desktop.Options;
using Chatly.Desktop.Services.Api;
using Chatly.Desktop.Services.Api.Refit;
using Chatly.Desktop.Services.Api.Refit.DelegatingHandlers;
using Chatly.Desktop.Services.Api.SignalR;
using Chatly.Desktop.Services.Api.SignalR.NotificationEventStrategies;
using Chatly.Desktop.Services.Authentication;
using Chatly.Desktop.Services.Authentication.Storage;
using Chatly.Desktop.Services.Authentication.Storage.MacOs;
using Chatly.Desktop.Services.Navigation;
using Chatly.Desktop.Services.Popups;
using Chatly.Desktop.Services.Startup;
using Chatly.Desktop.ViewModels.Pages;
using Chatly.Desktop.ViewModels.Pages.ChatPage;
using Chatly.Desktop.ViewModels.Windows;
using Chatly.Desktop.Views.Pages;
using Chatly.Desktop.Views.Pages.ChatPage;
using Chatly.Desktop.Views.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using OnboardingPopup = Chatly.Desktop.Views.Popups.OnboardingPopup;
using OnboardingPopupViewModel = Chatly.Desktop.ViewModels.Popups.OnboardingPopupViewModel;
using AddFriendPopupOverlay = Chatly.Desktop.Views.Pages.ChatPage.Popups.AddFriendPopupOverlay;
using AddFriendPopupOverlayViewModel = Chatly.Desktop.ViewModels.Pages.ChatPage.Popups.AddFriendPopupOverlayViewModel;
using AllFriendTabPage = Chatly.Desktop.Views.Pages.ChatPage.Tabs.AllFriendTabPage;
using AllFriendTabPageViewModel = Chatly.Desktop.ViewModels.Pages.ChatPage.Tabs.AllFriendTabPageViewModel;

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

            services.AddTransient<OnboardingPopupViewModel>();
            services.AddPopup<OnboardingPopup, OnboardingPopupViewModel>(ServiceLifetime.Transient);

            services.AddSingleton<ChatPageViewModel>();
            services.AddNavigableView<ChatPage, ChatPageViewModel>(ServiceLifetime.Singleton);
            services.AddSingleton<AllFriendTabPageViewModel>();
            services.AddSingleton<AllFriendTabPage>();
            services.AddSingleton<UserInfoPageViewModel>();
            services.AddNavigableView<UserInfoView, UserInfoPageViewModel>(ServiceLifetime.Singleton);

            services.AddTransient<AddFriendPopupOverlayViewModel>();
            services.AddPopup<AddFriendPopupOverlay, AddFriendPopupOverlayViewModel>(ServiceLifetime.Transient);

            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IPopupService, PopupService>();
            
            services.AddSingleton<AuthenticationService>();
            services.AddSingleton<SessionService>();
            services.AddSingleton<UserSessionContext>();
            services.AddSingleton<ApplicationStartupService>();

            services.AddSingleton<NotificationHubDispatcher>();
            services.AddSingleton<NotificationHubHost>();

            services.AddNotificationStrategy<NotificationIncomingFriendRequestStrategy>();

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

        private IServiceCollection AddNotificationStrategy<T>() where T : class, INotificationStrategy
        {
            services.AddSingleton<INotificationStrategy, T>();
            return services;
        }

        private IServiceCollection AddNavigableView<TView, TViewModel>(ServiceLifetime lifetime)
            where TView : class, INavigableView<TViewModel>
            where TViewModel : class, INavigableViewModel
        {
            if (lifetime == ServiceLifetime.Scoped)
            {
                throw new ArgumentException("Scoped navigable views require scope ownership.", nameof(lifetime));
            }

            services.Add(new ServiceDescriptor(typeof(INavigableView<TViewModel>), typeof(TView), lifetime));
            return services;
        }

        private IServiceCollection AddPopup<TPopup, TViewModel>(ServiceLifetime lifetime)
            where TPopup : class, IPopupOverlay<TViewModel>
            where TViewModel : class, IPopupViewModel
        {
            services.Add(new ServiceDescriptor(typeof(IPopupOverlay<TViewModel>), typeof(TPopup), lifetime));
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
