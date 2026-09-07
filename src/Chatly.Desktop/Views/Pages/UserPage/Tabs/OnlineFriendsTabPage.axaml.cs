using Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

namespace Chatly.Desktop.Views.Pages.UserPage.Tabs;

[SingletonService(typeof(INavigableView<OnlineFriendsTabPageViewModel>))]
public partial class OnlineFriendsTabPage : UserControl, INavigableView<OnlineFriendsTabPageViewModel>
{
    public OnlineFriendsTabPage(OnlineFriendsTabPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public OnlineFriendsTabPageViewModel ViewModel { get; }
}