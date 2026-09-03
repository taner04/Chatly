using Avalonia.Input;
using Chatly.Desktop.Services.Popups;
using Chatly.Desktop.ViewModels.Popups;

namespace Chatly.Desktop.Views.Popups;

[TransientService(typeof(IPopupOverlay<UserInfoPopupViewModel>))]
public partial class UserInfoPopup : UserControl, IPopupOverlay<UserInfoPopupViewModel>
{
    public UserInfoPopup(UserInfoPopupViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;

        InitializeComponent();
    }

    public UserInfoPopupViewModel ViewModel { get; }

    public void HandlePopupEvent(object? sender, PopupOverlayEventArgs args)
    {
        if (args is { Type: PopupOverlayHostEventType.KeyEvent, Data: KeyEventArgs { Key: Key.Escape } })
        {
            ViewModel.CloseOverlay();
        }
        else if (args.Type == PopupOverlayHostEventType.MouseEvent &&
                 args.Data is PointerPressedEventArgs pointerPressedEventArgs)
        {
            var clickPosition = pointerPressedEventArgs.GetPosition(this);
            if (!Bounds.Contains(clickPosition))
            {
                ViewModel.CloseOverlay();
            }
        }
    }
}