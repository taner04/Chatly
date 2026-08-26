using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Chatly.Desktop.Abstractions.Navigation;
using Chatly.Desktop.Abstractions.Popups;
using Chatly.Desktop.ViewModels.Windows;
using PopupOverlayHost = Chatly.Desktop.Views.Popups.PopupOverlayHost;

namespace Chatly.Desktop.Views.Windows;

public partial class MainWindow : Window, INavigationView
{
    private const double CollapsedSidebarWidth = 48;
    private const double ExpandedSidebarWidth = 220;

    private readonly Border _sidebar;
    private readonly TextBlock _sidebarToggleIcon;
    private readonly INavigationService _navigationService;
    private bool _isSidebarExpanded;

    public MainWindow(
        INavigationService navigationService,
        IPopupService popupService,
        MainWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;
        _navigationService = navigationService;
        InitializeComponent();
        
        _navigationService.SetNavigationView(this);
        popupService.SetPopupHost(this.FindControl<PopupOverlayHost>("PopupHost") ?? throw new InvalidOperationException("Popup host not found."));

        _sidebar = this.FindControl<Border>("Sidebar") ?? throw new InvalidOperationException("Sidebar not found.");
        _sidebarToggleIcon = this.FindControl<TextBlock>("SidebarToggleIcon") ?? throw new InvalidOperationException("SidebarToggleIcon not found.");
    }

    public MainWindowViewModel ViewModel { get; }

    public void SetPage<T>(INavigableView<T> view) where T : INavigableViewModel
    {
        ArgumentNullException.ThrowIfNull(view);
        PageHost.Content = view;
    }

    public ContentControl GetPageHost()
    {
        return this.FindControl<ContentControl>("PageHost") ??
               throw new InvalidOperationException("PageHost not found in the MainWindow.");
    }

    private void SidebarToggle_OnClick(object? sender, RoutedEventArgs e)
    {
        _isSidebarExpanded = !_isSidebarExpanded;
        _sidebar.Width = _isSidebarExpanded ? ExpandedSidebarWidth : CollapsedSidebarWidth;
        _sidebarToggleIcon.Text = _isSidebarExpanded ? "<" : ">";
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        switch (e.InitialPressMouseButton)
        {
            case MouseButton.XButton1:
                _navigationService.GoBack();
                break;
            case MouseButton.XButton2:
                _navigationService.GoForward();
                break;
            case MouseButton.None:
            case MouseButton.Left:
            case MouseButton.Right:
            case MouseButton.Middle:
            default:
                break;
        }
    }
}
