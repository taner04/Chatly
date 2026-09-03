using System.Collections.ObjectModel;

namespace Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

public abstract class FriendsTabViewModel(
    FriendActionsViewModel actions,
    ObservableCollection<Friend> friends) : PageViewModelBase
{
    public FriendActionsViewModel Actions { get; } = actions;

    public ObservableCollection<Friend> Friends { get; } = friends;
}
