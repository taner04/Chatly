using Chatly.Desktop.Models.UserSession;

namespace Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

[SingletonService]
public sealed class OnlineFriendsTabPageViewModel(
    FriendActionsViewModel actions,
    FriendState friendState)
    : FriendsTabViewModel(actions, friendState.OnlineFriends)
{
}
