using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Chatly.Desktop.DependencyInjection;
using Chatly.Desktop.Models.Settings;
using Chatly.Desktop.Services.Startup;
using Chatly.Desktop.Views.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Desktop;

public class App : Application
{
    private IClassicDesktopStyleApplicationLifetime? _desktop;
    private MainWindow? _mainWindow;
    private ServiceProvider? _services;
    private bool _shutdownCompleted;
    private int _shutdownStarted;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _desktop = desktop;
            _services = BuildServices();
            _services.GetRequiredService<AppSettings>();
            _mainWindow = _services.GetRequiredService<MainWindow>();
            _mainWindow.Closing += MainWindow_OnClosing;
            desktop.ShutdownRequested += Desktop_OnShutdownRequested;

            await _services.GetRequiredService<ApplicationStartupService>().RunAsync(desktop);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider BuildServices()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false)
            .AddEnvironmentVariables()
            .Build();

        return new ServiceCollection()
            .AddDesktop(configuration)
            .BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });
    }

    private async void MainWindow_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_shutdownCompleted)
        {
            return;
        }

        e.Cancel = true;
        await ShutdownAsync();
    }

    private async void Desktop_OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        if (_shutdownCompleted)
        {
            return;
        }

        e.Cancel = true;
        await ShutdownAsync();
    }

    private async Task ShutdownAsync()
    {
        if (Interlocked.Exchange(ref _shutdownStarted, 1) != 0)
        {
            return;
        }

        try
        {
            var services = Interlocked.Exchange(ref _services, null);
            if (services is not null)
            {
                try
                {
                    services.GetRequiredService<AppSettings>().Save();
                }
                finally
                {
                    await services.DisposeAsync();
                }
            }
        }
        finally
        {
            _shutdownCompleted = true;

            if (_mainWindow is not null)
            {
                _mainWindow.Closing -= MainWindow_OnClosing;
            }

            if (_desktop is not null)
            {
                _desktop.ShutdownRequested -= Desktop_OnShutdownRequested;
                _desktop.Shutdown();
            }
        }
    }
}