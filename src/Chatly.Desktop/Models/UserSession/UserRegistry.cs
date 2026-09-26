namespace Chatly.Desktop.Models.UserSession;

[SingletonService]
public sealed class UserRegistry
{
    private readonly Dictionary<Guid, User> _users = [];

    internal User GetOrAdd(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (!_users.TryGetValue(user.Id, out var existing))
        {
            _users.Add(user.Id, user);
            return user;
        }

        existing.Email = user.Email;
        existing.Username = user.Username;
        existing.ProfilePictureUrl = user.ProfilePictureUrl;
        existing.OnboardingCompleted = user.OnboardingCompleted;
        existing.IsOnline = user.IsOnline;
        return existing;
    }

    internal User GetOrAdd(
        Guid userId,
        string? username,
        string? profilePictureUrl,
        bool isOnline)
    {
        if (!_users.TryGetValue(userId, out var user))
        {
            user = new User { Id = userId };
            _users.Add(userId, user);
        }

        user.Username = username;
        user.ProfilePictureUrl = profilePictureUrl;
        user.IsOnline = isOnline;
        return user;
    }

    internal User? Find(Guid userId) => _users.GetValueOrDefault(userId);

    internal void UpdateProfile(Guid userId, string? username, string? profilePictureUrl)
    {
        if (!_users.TryGetValue(userId, out var user))
        {
            return;
        }

        user.Username = username;
        user.ProfilePictureUrl = profilePictureUrl;
    }

    internal void SetOnlineStatus(Guid userId, bool isOnline)
    {
        if (_users.TryGetValue(userId, out var user))
        {
            user.IsOnline = isOnline;
        }
    }

    internal void Clear()
    {
        _users.Clear();
    }
}