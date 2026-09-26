using Avalonia.Controls.ApplicationLifetimes;
using Chatly.Desktop.Abstraction.Audio;
using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Services.Authentication;
using Chatly.Desktop.Services.Calls;
using Chatly.Desktop.Services.Settings;
using Chatly.Desktop.ViewModels.Pages.UserPage;
using Chatly.Desktop.ViewModels.Popups;
using Chatly.Desktop.Views.Windows;

namespace Chatly.Desktop.Services.Startup;

[SingletonService]
internal sealed class ApplicationStartupService(
    SplashScreenWindow splashScreen,
    MainWindow mainWindow,
    StartupConnectivityService startupConnectivityService,
    SessionService sessionService,
    UserSessionContext sessionContext,
    INavigationService navigationService,
    IPopupService popupService,
    IEnumerable<IHubHost> hubHosts,
    ThemeService themeService,
    IAudioHost audioHost,
    CallCoordinator callCoordinator,
    ILogger<ApplicationStartupService> logger)
{
    private static readonly TimeSpan ApiRetryInterval = TimeSpan.FromSeconds(10);

    public async Task RunAsync(IClassicDesktopStyleApplicationLifetime desktop)
    {
        themeService.Apply();
        desktop.MainWindow = splashScreen;
        splashScreen.Show();

        try
        {
            await WaitForApiAsync(splashScreen.ViewModel.CancellationToken);
            splashScreen.ViewModel.StartupMessage = "Authenticating...";
            await sessionService.StartAsync(splashScreen.ViewModel.CancellationToken);
        }
        catch (OperationCanceledException) when (splashScreen.ViewModel.CancellationToken.IsCancellationRequested)
        {
            splashScreen.Close();
            desktop.Shutdown();
            return;
        }

        await audioHost.InitializeAsync(splashScreen.ViewModel.CancellationToken);

        foreach (var hub in hubHosts)
        {
            await hub.StartAsync(splashScreen.ViewModel.CancellationToken);
        }

        await navigationService.NavigateToAsync<UserPageViewModel>();

        desktop.MainWindow = mainWindow;
        mainWindow.Show();
        splashScreen.Close();

        try
        {
            await callCoordinator.ReconcileAsync(CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to reconcile the current call during startup.");
        }

        if (sessionContext.CurrentUser?.OnboardingCompleted == false)
        {
            await popupService.ShowAsync<OnboardingPopupViewModel>();
        }
    }

    internal async Task StopAsync(CancellationToken cancellationToken)
    {
        await callCoordinator.ShutdownAsync();

        foreach (var hub in hubHosts)
        {
            await hub.StopAsync(cancellationToken);
        }
    }

    private async Task WaitForApiAsync(CancellationToken cancellationToken)
    {
        splashScreen.ViewModel.StartupMessage = "Connecting to Chatly...";

        while (!await startupConnectivityService.IsReadyAsync(cancellationToken))
        {
            await ShowRetryCountdownAsync(ApiRetryInterval, cancellationToken);
        }
    }

    private async Task ShowRetryCountdownAsync(
        TimeSpan retryInterval,
        CancellationToken cancellationToken)
    {
        for (var secondsRemaining = (int)retryInterval.TotalSeconds;
             secondsRemaining >= 0;
             secondsRemaining--)
        {
            splashScreen.ViewModel.StartupMessage =
                $"Chatly is unavailable. Retrying in {secondsRemaining} sec...";

            if (secondsRemaining > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}