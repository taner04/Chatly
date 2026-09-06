using Avalonia.Input;
using Chatly.Desktop.Services.Popups;
using Chatly.Desktop.ViewModels.Pages.UserPage.Popups;

namespace Chatly.Desktop.Views.Pages.UserPage.Popups;

[TransientService(typeof(IPopupOverlay<AddFriendPopupOverlayViewModel>))]
public partial class AddFriendPopupOverlay : UserControl, IPopupOverlay<AddFriendPopupOverlayViewModel>
{
    public AddFriendPopupOverlay(AddFriendPopupOverlayViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public AddFriendPopupOverlayViewModel ViewModel { get; }

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
        else if (args.Type == PopupOverlayHostEventType.KeyEvent && args.Data is KeyEventArgs keyEventArgs2 &&
                 keyEventArgs2.Key == Key.Enter)
        {
            ViewModel.SearchForUsersCommand.Execute(null);
        }
    }
}