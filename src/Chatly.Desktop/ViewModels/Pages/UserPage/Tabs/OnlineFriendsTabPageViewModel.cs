using UserSessionContext = Chatly.Desktop.Models.UserSession.UserSessionContext;

namespace Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

[SingletonService]
public sealed class OnlineFriendsTabPageViewModel(
    FriendActionsViewModel actions,
    UserSessionContext userSessionContext)
    : FriendsTabViewModel(actions, userSessionContext.OnlineFriends)
{
}