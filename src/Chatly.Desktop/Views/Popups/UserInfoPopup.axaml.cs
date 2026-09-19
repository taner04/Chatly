using Chatly.Desktop.ViewModels.Popups;

namespace Chatly.Desktop.Views.Popups;

[TransientService(typeof(IPopupOverlay<UserInfoPopupViewModel>))]
internal partial class UserInfoPopup : UserControl, IPopupOverlay<UserInfoPopupViewModel>
{
    public UserInfoPopup(UserInfoPopupViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public UserInfoPopupViewModel ViewModel { get; }

    public bool IsDismissible => true;
}