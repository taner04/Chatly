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

    internal void RemoveByUserId(Guid userId)
    {
        var directChat = Items.FirstOrDefault(chat => chat.User.Id == userId);
        if (directChat is not null)
        {
            RemoveItem(directChat);
        }
    }
}