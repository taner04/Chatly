using System.Collections.ObjectModel;
using Chatly.Desktop.ViewModels;

namespace Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

public abstract class FriendsTabViewModel(
    FriendActionsViewModel actions,
    CallViewModel call,
    ReadOnlyObservableCollection<Friend> friends) : PageViewModelBase
{
    public FriendActionsViewModel Actions { get; } = actions;

    public CallViewModel Call { get; } = call;

    public ReadOnlyObservableCollection<Friend> Friends { get; } = friends;
}
