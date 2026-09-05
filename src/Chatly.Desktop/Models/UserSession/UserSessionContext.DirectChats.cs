using System.Collections.ObjectModel;
using System.Linq;

namespace Chatly.Desktop.Models.UserSession;

public sealed partial class UserSessionContext
{
    public ObservableCollection<DirectChat> DirectChats { get; } = [];

    public bool AddDirectChat(DirectChat directChat)
    {
        ArgumentNullException.ThrowIfNull(directChat);
        var existing = DirectChats.FirstOrDefault(chat => chat.Id == directChat.Id);
        if (existing is not null)
        {
            existing.User.IsOnline = directChat.User.IsOnline;
            return false;
        }

        DirectChats.Add(directChat);
        return true;
    }

    public void SetDirectChats(IEnumerable<DirectChat> directChats)
    {
        ArgumentNullException.ThrowIfNull(directChats);
        DirectChats.Clear();

        foreach (var directChat in directChats)
        {
            AddDirectChat(directChat);
        }
    }
}