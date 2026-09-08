using Chatly.Desktop.Models.UserSession;

namespace Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

[SingletonService]
public sealed class AllFriendsTabPageViewModel(
    FriendActionsViewModel actions,
    FriendState friendState)
    : FriendsTabViewModel(
        actions,
        friendState.Items)
{
}
