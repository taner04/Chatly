using Avalonia.Controls.ApplicationLifetimes;
using Chatly.Desktop.Features.Authentication.Services;
using Chatly.Desktop.Features.Home.Views;
using Chatly.Desktop.Features.Onboarding.Views;
using Chatly.Desktop.Features.Shell.Views;
using Chatly.Desktop.Features.Startup.Views;
using Chatly.Desktop.Features.Users;
using Chatly.Desktop.Infrastructure.Navigation;
using System;
using System.Threading.Tasks;

namespace Chatly.Desktop.Features.Startup.Services;

public sealed class ApplicationStartupService(
    SplashScreenWindow splashScreen,
    MainWindow mainWindow,
    SessionService sessionService,
    UserSessionContext sessionContext,
    NavigationService navigationService)
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

        var initialPage = sessionContext.CurrentUser?.OnboardingCompleted == true
            ? typeof(HomePage)
            : typeof(OnboardingPage);

        navigationService.NavigateTo(initialPage);
    }
}
