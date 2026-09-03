using Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

namespace Chatly.Desktop.Views.Pages.UserPage.Tabs;

[SingletonService(typeof(INavigableView<PendingFriendRequestTabPageViewModel>))]
public partial class PendingFriendRequestTabPage
    : UserControl, INavigableView<PendingFriendRequestTabPageViewModel>
{
    public PendingFriendRequestTabPage(PendingFriendRequestTabPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    public PendingFriendRequestTabPageViewModel ViewModel { get; }
}