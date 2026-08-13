using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Chatly.Desktop.Features.Shell.ViewModels;
using Chatly.Desktop.Infrastructure.Navigation;
using Chatly.Desktop.Shared.Abstractions;

namespace Chatly.Desktop.Features.Shell.Views;

public partial class MainWindow : Window, INavigationView
{
    private const double CollapsedSidebarWidth = 48;
    private const double ExpandedSidebarWidth = 220;

    private readonly INavigationService _navigationService;
    private readonly Border _sidebar;
    private readonly TextBlock _sidebarToggleIcon;
    private bool _isSidebarExpanded;

    public MainWindow(
        INavigationService navigationService,
        MainWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;

        _navigationService = navigationService;

        InitializeComponent();
        _navigationService.SetNavigationView(this);
        _navigationService.Navigated += NavigationService_OnNavigated;

        _sidebar = this.FindControl<Border>("Sidebar") ?? throw new InvalidOperationException("Sidebar not found.");
        _sidebarToggleIcon = this.FindControl<TextBlock>("SidebarToggleIcon") ??
                             throw new InvalidOperationException("SidebarToggleIcon not found.");
    }

    public MainWindowViewModel ViewModel { get; }

    public ContentControl GetPageHost() => this.FindControl<ContentControl>("PageHost") ??
                                           throw new InvalidOperationException("PageHost not found in the MainWindow.");

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: NavigationItemViewModel navigationItem })
        {
            return;
        }

        _navigationService.NavigateTo(navigationItem.Page);
    }

    private void NavigationService_OnNavigated(object? sender, NavigatedEventArgs e)
    {
        NavigationItemViewModel? selectedItem = null;

        foreach (var item in ViewModel.NavigationItems)
        {
            item.IsSelected = item.Page == e.PageType;
            if (item.IsSelected)
            {
                selectedItem = item;
            }
        }

        foreach (var item in ViewModel.FooterNavigationItems)
        {
            item.IsSelected = item.Page == e.PageType;
            if (item.IsSelected)
            {
                selectedItem = item;
            }
        }

        if (selectedItem is not null)
        {
            ViewModel.CurrentNavigationItem = selectedItem;
        }
    }

    private void SidebarToggle_OnClick(object? sender, RoutedEventArgs e)
    {
        _isSidebarExpanded = !_isSidebarExpanded;
        _sidebar.Width = _isSidebarExpanded ? ExpandedSidebarWidth : CollapsedSidebarWidth;
        _sidebarToggleIcon.Text = _isSidebarExpanded ? "<" : ">";
    }
}
