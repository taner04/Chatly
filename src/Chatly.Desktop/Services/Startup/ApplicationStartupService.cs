using Avalonia.Controls.ApplicationLifetimes;
using Chatly.Desktop.Services.Api.SignalR;
using Chatly.Desktop.Services.Authentication;
using Chatly.Desktop.ViewModels.Pages.UserPage;
using Chatly.Desktop.ViewModels.Popups;
using Chatly.Desktop.Views.Windows;

namespace Chatly.Desktop.Services.Startup;

[SingletonService]
public sealed class ApplicationStartupService(
    SplashScreenWindow splashScreen,
    MainWindow mainWindow,
    SessionService sessionService,
    UserSessionContext sessionContext,
    INavigationService navigationService,
    IPopupService popupService,
    NotificationHubConnection notificationHubConnection,
    ClientNotificationDispatcher clientNotificationDispatcher)
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

        await notificationHubConnection.StartHubAsync(clientNotificationDispatcher.DispatchAsync);

        if (sessionContext.CurrentUser?.OnboardingCompleted == false)
        {
            await popupService.ShowAsync<OnboardingPopupViewModel>();
        }

        navigationService.NavigateTo<UserPageViewModel>();
    }
}