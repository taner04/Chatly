using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Chatly.Contracts.Extensions;
using Chatly.Desktop.Abstractions;
using Chatly.Desktop.Infrastructure;
using Chatly.Desktop.Infrastructure.Auth;
using Chatly.Desktop.Options;
using Chatly.Desktop.ViewModels;
using Chatly.Desktop.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Desktop;

public class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();

        collection.AddSingleton<MainWindow>();
        collection.AddSingleton<MainWindowViewModel>();

        collection.AddSingleton<NavigationService>();
        collection.AddSingleton<PageService>();

        collection.AddSingleton<HomePageView>();
        collection.AddSingleton<HomePageViewModel>();
        collection.AddSingleton<LoginPageView>();
        collection.AddSingleton<LoginPageViewModel>();
        collection.AddSingleton<AuthenticationService>();
        
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
        
        collection.AddSingleton(configuration);
        collection.AddOption<Auth0Option>(configuration);

        Services = collection.BuildServiceProvider();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Services.GetRequiredService<MainWindow>();

            Services
                .GetRequiredService<NavigationService>()
                .NavigateTo(typeof(HomePageView));
        }
    }
}
