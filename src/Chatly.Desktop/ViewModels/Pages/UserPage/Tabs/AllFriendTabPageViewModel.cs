using Chatly.Desktop.Abstractions.Popups;

namespace Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

public sealed class AllFriendTabPageViewModel(IPopupService popupService)
    : FriendsTabViewModel(
        popupService,
        "All Friends",
        ["Alex Lee", "Maya Singh", "Jordan Brooks"],
        true)
{
    public string HelperText => "Search by username to send a friend request.";
}
