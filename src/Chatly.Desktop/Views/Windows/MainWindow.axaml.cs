using Avalonia.Input;
using Chatly.Desktop.ViewModels;
using Chatly.Desktop.ViewModels.Windows;
using Chatly.Desktop.Views.Calls;
using Chatly.Desktop.Views.Popups;
using Chatly.Desktop.Views.Toasts;

namespace Chatly.Desktop.Views.Windows;

[SingletonService]
internal partial class MainWindow : Window, INavigationView
{
    private readonly INavigationService _navigationService;

    public MainWindow(
        INavigationService navigationService,
        IPopupService popupService,
        IToastService toastService,
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

        _navigationService.SetNavigationView(this);

        CallMediaContainer.Content = callMediaView;
        PopupHostContainer.Content = popupHost;
        ToastHostContainer.Content = toastHost;

        popupService.SetPopupHost(popupHost);
        toastService.SetToastHost(toastHost);
    }

    public MainWindowViewModel ViewModel { get; }

    public CallViewModel Call { get; }

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