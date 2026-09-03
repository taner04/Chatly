namespace Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

[SingletonService]
public sealed class AllFriendsTabPageViewModel(
    FriendActionsViewModel actions,
    UserSessionContext userSessionContext)
    : FriendsTabViewModel(
        actions,
        userSessionContext.Friends)
{
}
