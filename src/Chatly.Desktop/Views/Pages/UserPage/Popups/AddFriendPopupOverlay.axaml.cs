using Chatly.Desktop.ViewModels.Pages.UserPage.Popups;

namespace Chatly.Desktop.Views.Pages.UserPage.Popups;

internal partial class AddFriendPopupOverlay : UserControl, IViewFor<AddFriendPopupOverlayViewModel>
{
    public AddFriendPopupOverlay(AddFriendPopupOverlayViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    public AddFriendPopupOverlayViewModel ViewModel { get; }
}