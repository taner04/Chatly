using Chatly.Desktop.ViewModels.Popups;

namespace Chatly.Desktop.Views.Popups;

internal partial class UserInfoPopup : UserControl, IViewFor<UserInfoPopupViewModel>
{
    public UserInfoPopup(UserInfoPopupViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    public UserInfoPopupViewModel ViewModel { get; }
}