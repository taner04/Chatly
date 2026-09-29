namespace Chatly.WebApi.Common.Infrastructure;

[SingletonService]
public sealed class OnlinePresenceTracker
{
    private readonly Lock _lock = new();
    private readonly Dictionary<UserId, UserPresence> _presences = [];

    internal bool Connect(UserId userId, string connectionId)
    {
        lock (_lock)
        {
            if (_presences.TryGetValue(userId, out var presence))
            {
                presence.Connections.Add(connectionId);
                return false;
            }

            _presences[userId] = new UserPresence { Connections = { connectionId } };
            return true;
        }
    }

    internal bool TryBeginOffline(UserId userId, string connectionId, out long offlineVersion)
    {
        lock (_lock)
        {
            offlineVersion = 0;
            if (!_presences.TryGetValue(userId, out var presence)
                || !presence.Connections.Remove(connectionId)
                || presence.Connections.Count > 0)
            {
                return false;
            }

            offlineVersion = ++presence.OfflineVersion;
            return true;
        }
    }

    internal bool TryCompleteOffline(UserId userId, long offlineVersion)
    {
        lock (_lock)
        {
            if (!_presences.TryGetValue(userId, out var presence)
                || presence.Connections.Count > 0
                || presence.OfflineVersion != offlineVersion)
            {
                return false;
            }

            _presences.Remove(userId);
            return true;
        }
    }

    internal bool IsOnline(UserId userId)
    {
        lock (_lock)
        {
            return _presences.ContainsKey(userId);
        }
    }

    internal IReadOnlyList<UserId> GetOnlineUserIds()
    {
        lock (_lock)
        {
            return [.. _presences.Keys];
        }
    }

    private sealed class UserPresence
    {
        public HashSet<string> Connections { get; } = [];

        public long OfflineVersion { get; set; }
    }
}