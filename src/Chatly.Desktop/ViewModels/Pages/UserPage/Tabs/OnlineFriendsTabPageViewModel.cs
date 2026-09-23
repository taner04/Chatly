using Chatly.Desktop.ViewModels;

namespace Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

[SingletonService]
public sealed class OnlineFriendsTabPageViewModel(
    FriendActionsViewModel actions,
    CallViewModel call,
    FriendState friendState)
    : FriendsTabViewModel(actions, call, friendState.OnlineFriends)
{
}
