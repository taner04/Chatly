using System.Collections.Generic;
using Chatly.Desktop.Abstractions.Popups;
using Chatly.Desktop.Abstractions.Toasts;
using Chatly.Desktop.ViewModels.Pages.ChatPage.Tabs;
using Chatly.Desktop.ViewModels.Toasts;

namespace Chatly.Desktop.ViewModels.Pages.ChatPage;

public sealed class ChatPageViewModel : ViewModelBase
{
    public ChatPageViewModel(IPopupService popupService)
    {
        OnlineFriends = new FriendsTabViewModel(popupService)
        {
            Title = "Online Friends",
            Friends = ["Alex Lee", "Maya Singh"]
        };
        AllFriends = new FriendsTabViewModel(popupService)
        {
            Title = "All Friends",
            Friends = ["Alex Lee", "Maya Singh", "Jordan Brooks"],
            ShowSearch = true
        };
    }

    public List<ChatPreviewViewModel> Chats { get; } =
    [
        new("AL", "Alex Lee", "See you in the channel.", "10:42"),
        new("MS", "Maya Singh", "Sent an attachment", "09:18"),
        new("JB", "Jordan Brooks", "Lets catch up soon.", "Yesterday")
    ];

    public FriendsTabViewModel OnlineFriends { get; }

    public FriendsTabViewModel AllFriends { get; }

    public AllFriendTabPageViewModel AllFriendTabPage { get; } = new();
}

public sealed class ChatPreviewViewModel(
    string initials,
    string name,
    string preview,
    string time) : ViewModelBase
{
    public string Initials { get; } = initials;

    public string Name { get; } = name;

    public string Preview { get; } = preview;

    public string Time { get; } = time;
}
