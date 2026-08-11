using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Chatly.Desktop.Abstractions;
using Chatly.Desktop.Infrastructure;
using Chatly.Desktop.ViewModels;

namespace Chatly.Desktop.Views;

public partial class MainWindow : Window, INavigationView
{
    private const double CollapsedSidebarWidth = 48;
    private const double ExpandedSidebarWidth = 220;

    private readonly NavigationService _navigationService;
    private readonly Border _sidebar;
    private readonly TextBlock _sidebarToggleIcon;
    private bool _isSidebarExpanded;

    public MainWindow(
        NavigationService navigationService,
        MainWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;
        
        _navigationService = navigationService;

        InitializeComponent();
        _navigationService.SetNavigationView(this);

        _sidebar = this.FindControl<Border>("Sidebar") ?? throw new InvalidOperationException("Sidebar not found.");
        _sidebarToggleIcon = this.FindControl<TextBlock>("SidebarToggleIcon") ?? throw new InvalidOperationException("SidebarToggleIcon not found.");
    }

    public MainWindowViewModel ViewModel { get; }
    
    public ContentControl GetPageHost() => this.FindControl<ContentControl>("PageHost") ?? throw new InvalidOperationException("PageHost not found in the MainWindow.");

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: NavigationItemViewModel navigationItem } || navigationItem.Page == _navigationService.CurrentPage)
        {
            return;
        }

        foreach (var item in ViewModel.NavigationItems)
        {
            item.IsSelected = item == navigationItem;
        }

        ViewModel.CurrentNavigationItem = navigationItem;
        _navigationService.NavigateTo(navigationItem.Page);
    }

    private void SidebarToggle_OnClick(object? sender, RoutedEventArgs e)
    {
        _isSidebarExpanded = !_isSidebarExpanded;
        _sidebar.Width = _isSidebarExpanded ? ExpandedSidebarWidth : CollapsedSidebarWidth;
        _sidebarToggleIcon.Text = _isSidebarExpanded ? "<" : ">";
    }
}
