using System.Collections.ObjectModel;
using System.ComponentModel;

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

    internal bool Remove(Guid userId) => RemoveItem(userId);

    protected override void OnItemAdded(Friend item)
    {
        item.User.PropertyChanged += User_PropertyChanged;
        if (item.User.IsOnline)
        {
            _onlineFriends.Add(item);
        }
    }

    protected override void OnExistingItem(Friend existing, Friend incoming)
    {
        existing.ChatId = incoming.ChatId;
    }

    protected override void OnItemRemoving(Friend item)
    {
        item.User.PropertyChanged -= User_PropertyChanged;
        _onlineFriends.Remove(item);
    }

    protected override void OnItemsClearing()
    {
        foreach (var friend in Items)
        {
            friend.User.PropertyChanged -= User_PropertyChanged;
        }

        _onlineFriends.Clear();
    }

    private void User_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(User.IsOnline) || sender is not User user)
        {
            return;
        }

        var friend = Items.FirstOrDefault(existing => ReferenceEquals(existing.User, user));
        if (friend is null)
        {
            return;
        }

        if (user.IsOnline && !_onlineFriends.Contains(friend))
        {
            _onlineFriends.Add(friend);
        }
        else if (!user.IsOnline)
        {
            _onlineFriends.Remove(friend);
        }
    }
}