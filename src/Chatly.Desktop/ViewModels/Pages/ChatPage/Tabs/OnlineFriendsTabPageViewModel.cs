using Chatly.Desktop.Abstractions.Popups;

namespace Chatly.Desktop.ViewModels.Pages.ChatPage.Tabs;

public sealed class OnlineFriendsTabPageViewModel(IPopupService popupService)
    : FriendsTabViewModel(popupService, "Online Friends", ["Alex Lee", "Maya Singh"]);
