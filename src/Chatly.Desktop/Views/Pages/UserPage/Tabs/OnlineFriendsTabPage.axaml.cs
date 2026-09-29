using Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

namespace Chatly.Desktop.Views.Pages.UserPage.Tabs;

internal partial class OnlineFriendsTabPage : UserControl, IViewFor<OnlineFriendsTabPageViewModel>
{
    public OnlineFriendsTabPage(OnlineFriendsTabPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    public OnlineFriendsTabPageViewModel ViewModel { get; }
}