using Avalonia.Controls;
using Chatly.Desktop.Abstractions.Navigation;
using Chatly.Desktop.ViewModels.Pages.ChatPage.Tabs;

namespace Chatly.Desktop.Views.Pages.ChatPage.Tabs;

public sealed class AllFriendsTabPage : UserControl, INavigableView<AllFriendTabPageViewModel>
{
    public AllFriendsTabPage(AllFriendTabPageViewModel viewModel)
    {
        ViewModel = viewModel;
        Content = new FriendsTabView { ViewModel = viewModel };
    }

    public AllFriendTabPageViewModel ViewModel { get; }
}
