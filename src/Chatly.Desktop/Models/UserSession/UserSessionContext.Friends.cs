using System.Collections.ObjectModel;
using System.Linq;

namespace Chatly.Desktop.Models;

public sealed partial class UserSessionContext
{
    [ObservableProperty] public partial ObservableCollection<Friend> Friends { get; private set; } = [];

    public ObservableCollection<Friend> OnlineFriends { get; } = [];

    public bool AddFriend(Friend friend)
    {
        ArgumentNullException.ThrowIfNull(friend);

        if (Friends.Any(existing => existing.User.Id == friend.User.Id))
        {
            return false;
        }

        Friends.Add(friend);
        if (friend.User.IsOnline)
        {
            OnlineFriends.Add(friend);
        }

        return true;
    }

    public void SetFriends(IEnumerable<Friend> friends)
    {
        ArgumentNullException.ThrowIfNull(friends);
        Friends.Clear();
        OnlineFriends.Clear();

        foreach (var friend in friends)
        {
            AddFriend(friend);
        }
    }

    public void SetFriendOnlineStatus(Guid userId, bool isOnline)
    {
        var friend = Friends.FirstOrDefault(existing => existing.User.Id == userId);
        if (friend is null || friend.User.IsOnline == isOnline)
        {
            return;
        }

        friend.User.IsOnline = isOnline;
        var directChat = DirectChats.FirstOrDefault(chat => chat.User.Id == userId);
        if (directChat is not null)
        {
            directChat.User.IsOnline = isOnline;
        }

        if (isOnline)
        {
            OnlineFriends.Add(friend);
        }
        else
        {
            OnlineFriends.Remove(friend);
        }
    }

    public void UpdateUserProfile(Guid userId, string? username, string? profilePictureUrl)
    {
        UpdateProfile(CurrentUser);
        UpdateProfile(Friends.FirstOrDefault(friend => friend.User.Id == userId)?.User);
        UpdateProfile(DirectChats.FirstOrDefault(chat => chat.User.Id == userId)?.User);
        return;

        void UpdateProfile(User? user)
        {
            if (user?.Id != userId)
            {
                return;
            }

            user.Username = username;
            user.ProfilePictureUrl = profilePictureUrl;
        }
    }

    public bool RemoveFriend(Guid userId)
    {
        var friend = Friends.FirstOrDefault(existing => existing.User.Id == userId);
        var directChat = DirectChats.FirstOrDefault(chat => chat.User.Id == userId);

        if (friend is not null)
        {
            OnlineFriends.Remove(friend);
            Friends.Remove(friend);
        }

        if (directChat is not null)
        {
            DirectChats.Remove(directChat);
        }

        return friend is not null || directChat is not null;
    }
}
