using Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

namespace Chatly.Desktop.Views.Pages.UserPage.Tabs;

[SingletonService(typeof(INavigableView<AllFriendsTabPageViewModel>))]
public partial class AllFriendsTabPage : UserControl, INavigableView<AllFriendsTabPageViewModel>
{
    public AllFriendsTabPage(AllFriendsTabPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public AllFriendsTabPageViewModel ViewModel { get; }
}