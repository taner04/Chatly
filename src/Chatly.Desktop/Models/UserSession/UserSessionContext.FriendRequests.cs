using System.Collections.ObjectModel;
using System.Linq;

namespace Chatly.Desktop.Models.UserSession;

public sealed partial class UserSessionContext
{
    [ObservableProperty] public partial ObservableCollection<FriendRequest> FriendRequests { get; private set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPendingFriendRequests))]
    public partial int PendingFriendRequestCount { get; private set; }

    public bool HasPendingFriendRequests => PendingFriendRequestCount > 0;

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

    internal void SetPendingFriendRequestCount(int count)
    {
        PendingFriendRequestCount = Math.Max(0, count);
    }

    internal void IncrementPendingFriendRequestCount()
    {
        PendingFriendRequestCount++;
    }

    internal void DecrementPendingFriendRequestCount()
    {
        PendingFriendRequestCount = Math.Max(0, PendingFriendRequestCount - 1);
    }
}