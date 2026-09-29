using Chatly.Desktop.ViewModels.Pages.UserPage;

namespace Chatly.Desktop.Views.Pages.UserPage;

internal partial class UserPage : UserControl, IViewFor<UserPageViewModel>
{
    public UserPage(UserPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    public UserPageViewModel ViewModel { get; }
}