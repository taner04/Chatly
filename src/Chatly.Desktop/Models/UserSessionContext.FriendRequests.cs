using System.Collections.ObjectModel;
using System.Linq;

namespace Chatly.Desktop.Models;

public sealed partial class UserSessionContext
{
    [ObservableProperty] public partial ObservableCollection<FriendRequest> FriendRequests { get; private set; } = [];

    public bool AddFriendRequest(FriendRequest friendRequest)
    {
        ArgumentNullException.ThrowIfNull(friendRequest);

        if (FriendRequests.Any(request => request.Id == friendRequest.Id))
        {
            return false;
        }

        FriendRequests.Add(friendRequest);
        return true;
    }

    public void SetFriendRequests(IEnumerable<FriendRequest> friendRequests)
    {
        ArgumentNullException.ThrowIfNull(friendRequests);
        FriendRequests.Clear();

        foreach (var friendRequest in friendRequests)
        {
            AddFriendRequest(friendRequest);
        }
    }

    public bool RemoveFriendRequest(Guid friendRequestId)
    {
        var friendRequest = FriendRequests.FirstOrDefault(request => request.Id == friendRequestId);
        return friendRequest is not null && FriendRequests.Remove(friendRequest);
    }
}