using Avalonia.Input;
using Chatly.Desktop.Services.Popups;
using Chatly.Desktop.ViewModels.Pages.UserPage.Popups;

namespace Chatly.Desktop.Views.Pages.UserPage.Popups;

[TransientService(typeof(IPopupOverlay<AddFriendPopupOverlayViewModel>))]
internal partial class AddFriendPopupOverlay : UserControl, IPopupOverlay<AddFriendPopupOverlayViewModel>
{
    public AddFriendPopupOverlay(AddFriendPopupOverlayViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public AddFriendPopupOverlayViewModel ViewModel { get; }

    public bool IsDismissible => true;

    public void HandlePopupEvent(object? sender, PopupOverlayEventArgs args)
    {
        if (args is { Type: PopupOverlayHostEventType.KeyEvent, Data: KeyEventArgs { Key: Key.Enter } })
        {
            ViewModel.SearchForUsersCommand.Execute(null);
        }
    }
}