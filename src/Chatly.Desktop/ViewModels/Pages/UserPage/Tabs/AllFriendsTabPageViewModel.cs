namespace Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

[SingletonService]
public sealed class AllFriendsTabPageViewModel(
    FriendActionsViewModel actions,
    CallViewModel call,
    FriendState friendState)
    : FriendsTabViewModel(
        actions,
        call,
        friendState.Items)
{
}