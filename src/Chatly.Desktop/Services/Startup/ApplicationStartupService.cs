using Avalonia.Controls.ApplicationLifetimes;
using Chatly.Desktop.Abstractions.Navigation;
using Chatly.Desktop.Abstractions.Popups;
using Chatly.Desktop.Abstractions.Toasts;
using Chatly.Desktop.Extentions;
using Chatly.Desktop.Models;
using Chatly.Desktop.Services.Api.SignalR;
using Chatly.Desktop.Services.Authentication;
using Chatly.Desktop.ViewModels.Pages.ChatPage;
using Chatly.Desktop.ViewModels.Popups;
using Chatly.Desktop.Views.Windows;
using System;
using System.Threading.Tasks;

namespace Chatly.Desktop.Services.Startup;

public sealed class ApplicationStartupService(
    SplashScreenWindow splashScreen,
    MainWindow mainWindow,
    SessionService sessionService,
    UserSessionContext sessionContext,
    INavigationService navigationService,
    IPopupService popupService,
    IToastService toastService,
    NotificationHubHost notificationHubHost)
{
    public async Task RunAsync(IClassicDesktopStyleApplicationLifetime desktop)
    {
        desktop.MainWindow = splashScreen;
        splashScreen.Show();

        try
        {
            await sessionService.StartAsync(splashScreen.ViewModel.CancellationToken);
        }
        catch (OperationCanceledException) when (splashScreen.ViewModel.CancellationToken.IsCancellationRequested)
        {
            splashScreen.Close();
            desktop.Shutdown();
            return;
        }

        desktop.MainWindow = mainWindow;
        mainWindow.Show();
        splashScreen.Close();

        await notificationHubHost.StartHubAsync();

        if (sessionContext.CurrentUser?.OnboardingCompleted == false)
        {
            await popupService.ShowAsync<OnboardingPopupViewModel>();
        }

        navigationService.NavigateTo<ChatPageViewModel>();

        toastService.AddNotificiation("You have successfully logged in.");
    }
}
