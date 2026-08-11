using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Chatly.Contracts.Extensions;
using Chatly.Desktop.Abstractions;
using Chatly.Desktop.Infrastructure;
using Chatly.Desktop.Infrastructure.Auth;
using Chatly.Desktop.Infrastructure.FileService;
using Chatly.Desktop.Options;
using Chatly.Desktop.ViewModels;
using Chatly.Desktop.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Desktop;

public class App : Application
{
    private static IServiceProvider Services { get; set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private static bool IsMacOs()
    {
#if TARGET_OSX || TARGET_MACCATALYST || TARGET_IOS || TARGET_TVOS
        return true;
#else
        return false;
#endif
    }

    private ServiceProvider ConfigureServices()
    {
        var collection = new ServiceCollection();
        
        collection.AddSingleton<MainWindow>();
        collection.AddSingleton<MainWindowViewModel>();

        collection.AddSingleton<SplashScreen>();
        collection.AddSingleton<SplashScreenViewModel>();
        
        collection.AddSingleton<NavigationService>();
        collection.AddSingleton<PageService>();

        collection.AddSingleton<HomePage>();
        collection.AddSingleton<HomePageViewModel>();
        collection.AddSingleton<AuthenticationService>();
        collection.AddSingleton<UserContext>(); 
        
        if(IsMacOs())
        {
            collection.AddSingleton<ISecureTokenStore, MacOsSecureTokenStore>();
        }
        else if(OperatingSystem.IsWindows())
        {
            collection.AddSingleton<ISecureTokenStore, WindowsSecureTokenStore>();
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
        
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
        
        collection.AddSingleton(configuration);
        collection.AddOption<Auth0Option>(configuration);

        return collection.BuildServiceProvider();
    }
    
    public override async void OnFrameworkInitializationCompleted()
    {
        Services = ConfigureServices();

        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            base.OnFrameworkInitializationCompleted();
            return;
        }

        var splashScreen = Services.GetRequiredService<SplashScreen>();

        desktop.MainWindow = splashScreen;
        splashScreen.Show();

        try
        {
            var authenticationService = Services.GetRequiredService<AuthenticationService>();
            await authenticationService.RestoreOrLoginAsync(splashScreen.ViewModel.CancellationToken);
        }
        catch (OperationCanceledException)when (splashScreen.ViewModel.CancellationToken.IsCancellationRequested)
        {
            splashScreen.Close();
            return;
        }

        var mainWindow =
            Services.GetRequiredService<MainWindow>();

        desktop.MainWindow = mainWindow;
        mainWindow.Show();

        splashScreen.Close();

        Services.GetRequiredService<NavigationService>().NavigateTo(typeof(HomePage));

        base.OnFrameworkInitializationCompleted();
    }
}
