using Avalonia.Input;
using Chatly.Desktop.ViewModels;
using Chatly.Desktop.ViewModels.Windows;
using Chatly.Desktop.Views.Calls;
using Chatly.Desktop.Views.Popups;
using Chatly.Desktop.Views.Toasts;

namespace Chatly.Desktop.Views.Windows;

[SingletonService]
internal partial class MainWindow : Window
{
    private readonly INavigationService _navigationService;

    public MainWindow(
        INavigationService navigationService,
        PopupOverlayHost popupHost,
        ToastHostOverlay toastHost,
        MainWindowViewModel viewModel,
        CallViewModel callViewModel,
        CallMediaView callMediaView)
    {
        ViewModel = viewModel;
        Call = callViewModel;
        DataContext = this;
        _navigationService = navigationService;
        InitializeComponent();

        CallMediaContainer.Content = callMediaView;
        PopupHostContainer.Content = popupHost;
        ToastHostContainer.Content = toastHost;
    }

    public MainWindowViewModel ViewModel { get; }

    public CallViewModel Call { get; }

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