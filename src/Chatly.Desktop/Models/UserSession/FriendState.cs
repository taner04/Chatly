using System.Collections.ObjectModel;
using System.Linq;

namespace Chatly.Desktop.Models.UserSession;

[SingletonService]
public sealed class FriendState
    : ObservableCollectionState<Friend>
{
    private readonly ObservableCollection<Friend> _onlineFriends = [];

    public FriendState()
    {
        OnlineFriends = new ReadOnlyObservableCollection<Friend>(_onlineFriends);
    }

    public ReadOnlyObservableCollection<Friend> OnlineFriends { get; }

    internal void Add(Friend friend)
    {
        AddItem(friend);
    }

    internal void Set(IEnumerable<Friend> friends)
    {
        SetItems(friends);
    }

    internal void SetOnlineStatus(Guid userId, bool isOnline)
    {
        var friend = Items.FirstOrDefault(existing => existing.User.Id == userId);
        if (friend is null || friend.User.IsOnline == isOnline)
        {
            return;
        }

        friend.User.IsOnline = isOnline;
        if (isOnline)
        {
            _onlineFriends.Add(friend);
        }
        else
        {
            _onlineFriends.Remove(friend);
        }
    }

    internal void UpdateUserProfile(Guid userId, string? username, string? profilePictureUrl)
    {
        foreach (var friend in Items.Where(friend => friend.User.Id == userId))
        {
            friend.User.Username = username;
            friend.User.ProfilePictureUrl = profilePictureUrl;
        }
    }

    internal bool Remove(Guid userId)
    {
        return RemoveItem(userId);
    }

    protected override void OnItemAdded(Friend item)
    {
        if (item.User.IsOnline)
        {
            _onlineFriends.Add(item);
        }
    }

    protected override void OnItemRemoving(Friend item)
    {
        _onlineFriends.Remove(item);
    }

    protected override void OnItemsClearing()
    {
        _onlineFriends.Clear();
    }
}
