using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Chatly.Desktop.Abstractions;
using Chatly.Desktop.Infrastructure;
using Chatly.Desktop.ViewModels;

namespace Chatly.Desktop.Views;

public partial class MainWindow : Window, INavigationView
{
    private readonly NavigationService _navigationService;

    public MainWindow(
        NavigationService navigationService,
        MainWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;
        
        _navigationService = navigationService;

        InitializeComponent();
        _navigationService.SetNavigationView(this);
    }

    public MainWindowViewModel ViewModel { get; }
    
    public ContentControl GetPageHost() => this.FindControl<ContentControl>("PageHost") ?? throw new InvalidOperationException("PageHost not found in the MainWindow.");

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if(sender is Button { DataContext: NavigationItemViewModel navigationItem } && navigationItem.Page != _navigationService.CurrentPage)
        {
            _navigationService.NavigateTo(navigationItem.Page);
        }
    }
}
