namespace Chatly.WebApi.Features.Hubs;

[SingletonService]
public sealed class OnlinePresenceTracker
{
    private readonly Dictionary<UserId, HashSet<string>> _connections = [];
    private readonly Lock _lock = new();

    public bool Connect(UserId userId, string connectionId)
    {
        lock (_lock)
        {
            if (!_connections.TryGetValue(userId, out var connections))
            {
                connections = [];
                _connections[userId] = connections;
            }

            var wasOffline = connections.Count == 0;
            connections.Add(connectionId);
            return wasOffline;
        }
    }

    public bool Disconnect(UserId userId, string connectionId)
    {
        lock (_lock)
        {
            if (!_connections.TryGetValue(userId, out var connections) || !connections.Remove(connectionId))
            {
                return false;
            }

            if (connections.Count > 0)
            {
                return false;
            }

            _connections.Remove(userId);
            return true;
        }
    }

    public bool IsOnline(UserId userId)
    {
        lock (_lock)
        {
            return _connections.ContainsKey(userId);
        }
    }
}