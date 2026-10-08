namespace Chatly.Desktop.Models.UserSession;

[SingletonService]
public sealed partial class UserSessionContext(UserRegistry userRegistry) : ObservableObject
{
    [ObservableProperty] public partial User? CurrentUser { get; private set; }

    internal event EventHandler? DeviceSessionRevoked;

    internal void NotifyDeviceSessionRevoked() => DeviceSessionRevoked?.Invoke(this, EventArgs.Empty);

    internal void SetAuthenticated(User user)
    {
        CurrentUser = userRegistry.GetOrAdd(user);
    }

    internal void Clear()
    {
        CurrentUser = null;
        userRegistry.Clear();
    }
}