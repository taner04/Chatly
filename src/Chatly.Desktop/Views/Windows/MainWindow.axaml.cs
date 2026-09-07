using Avalonia.Input;
using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.ViewModels.Windows;
using Chatly.Desktop.Views.Popups;
using Chatly.Desktop.Views.Toasts;

namespace Chatly.Desktop.Views.Windows;

[SingletonService]
public partial class MainWindow : Window, INavigationView
{
    private readonly INavigationService _navigationService;

    public MainWindow(
        INavigationService navigationService,
        IPopupService popupService,
        IToastService toastService,
        PopupOverlayHost popupHost,
        ToastHostOverlay toastHost,
        MainWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        _navigationService = navigationService;
        InitializeComponent();

        _navigationService.SetNavigationView(this);

        PopupHostContainer.Content = popupHost;
        ToastHostContainer.Content = toastHost;

        popupService.SetPopupHost(popupHost);
        toastService.SetToastHost(toastHost);
    }

    public MainWindowViewModel ViewModel { get; }

    public void SetPage<T>(INavigableView<T> view) where T : INavigableViewModel
    {
        ArgumentNullException.ThrowIfNull(view);
        PageHost.Content = view;
    }

    protected override async void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        switch (e.InitialPressMouseButton)
        {
            case MouseButton.XButton1:
                await _navigationService.GoBackAsync();
                break;
            case MouseButton.XButton2:
                await _navigationService.GoForwardAsync();
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