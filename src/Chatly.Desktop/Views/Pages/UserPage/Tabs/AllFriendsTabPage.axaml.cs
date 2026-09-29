using Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

namespace Chatly.Desktop.Views.Pages.UserPage.Tabs;

internal partial class AllFriendsTabPage : UserControl, IViewFor<AllFriendsTabPageViewModel>
{
    public AllFriendsTabPage(AllFriendsTabPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    public AllFriendsTabPageViewModel ViewModel { get; }
}