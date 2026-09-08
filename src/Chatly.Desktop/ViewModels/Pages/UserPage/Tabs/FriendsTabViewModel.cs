using System.Collections.ObjectModel;

namespace Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

public abstract class FriendsTabViewModel(
    FriendActionsViewModel actions,
    ReadOnlyObservableCollection<Friend> friends) : PageViewModelBase
{
    public FriendActionsViewModel Actions { get; } = actions;

    public ReadOnlyObservableCollection<Friend> Friends { get; } = friends;
}
