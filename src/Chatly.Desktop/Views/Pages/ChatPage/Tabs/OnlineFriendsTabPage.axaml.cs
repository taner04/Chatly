using Avalonia.Controls;
using Chatly.Desktop.Abstractions.Navigation;
using Chatly.Desktop.ViewModels.Pages.ChatPage.Tabs;

namespace Chatly.Desktop.Views.Pages.ChatPage.Tabs;

public sealed class OnlineFriendsTabPage : UserControl, INavigableView<OnlineFriendsTabPageViewModel>
{
    public OnlineFriendsTabPage(OnlineFriendsTabPageViewModel viewModel)
    {
        ViewModel = viewModel;
        Content = new FriendsTabView { ViewModel = viewModel };
    }

    public OnlineFriendsTabPageViewModel ViewModel { get; }
}
