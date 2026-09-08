namespace Chatly.Desktop.Models.UserSession;

[SingletonService]
public sealed partial class FriendRequestState
    : ObservableCollectionState<FriendRequest>
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPendingRequests))]
    public partial int PendingCount { get; private set; }

    public bool HasPendingRequests => PendingCount > 0;

    internal bool Add(FriendRequest friendRequest)
    {
        return AddItem(friendRequest);
    }

    internal void Set(IEnumerable<FriendRequest> friendRequests)
    {
        SetItems(friendRequests);
    }

    internal void Remove(Guid friendRequestId)
    {
        RemoveItem(friendRequestId);
    }

    internal void SetPendingCount(int count)
    {
        PendingCount = Math.Max(0, count);
    }

    internal void IncrementPendingCount()
    {
        PendingCount++;
    }

    internal void DecrementPendingCount()
    {
        PendingCount = Math.Max(0, PendingCount - 1);
    }

    protected override void OnCleared()
    {
        PendingCount = 0;
    }
}
