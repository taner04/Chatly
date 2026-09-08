using System.Linq;

namespace Chatly.Desktop.Models.UserSession;

[SingletonService]
public sealed class DirectChatState
    : ObservableCollectionState<DirectChat>
{
    internal void Add(DirectChat directChat)
    {
        AddItem(directChat);
    }

    internal void Set(IEnumerable<DirectChat> directChats)
    {
        SetItems(directChats);
    }

    internal void SetOnlineStatus(Guid userId, bool isOnline)
    {
        var directChat = Items.FirstOrDefault(chat => chat.User.Id == userId);
        directChat?.User.IsOnline = isOnline;
    }

    internal void UpdateUserProfile(Guid userId, string? username, string? profilePictureUrl)
    {
        foreach (var chat in Items.Where(chat => chat.User.Id == userId))
        {
            chat.User.Username = username;
            chat.User.ProfilePictureUrl = profilePictureUrl;
        }
    }

    internal void RemoveByUserId(Guid userId)
    {
        var directChat = Items.FirstOrDefault(chat => chat.User.Id == userId);
        if (directChat is not null)
        {
            RemoveItem(directChat);
        }
    }

    protected override void OnExistingItem(DirectChat existing, DirectChat incoming)
    {
        existing.User.IsOnline = incoming.User.IsOnline;
    }
}
