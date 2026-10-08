using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.Services.Authentication;

[SingletonService]
internal sealed partial class RevokedDeviceSignOutService(
    UserSessionContext sessionContext,
    SessionService sessionService,
    IPopupService popupService,
    ILogger<RevokedDeviceSignOutService> logger) : IDisposable
{
    private readonly AtomicFlag _signingOut = new();
    private readonly AtomicFlag _started = new();

    public void Dispose()
    {
        if (_started.TryReset())
        {
            sessionContext.DeviceSessionRevoked -= OnDeviceSessionRevoked;
        }
    }

    internal void Start()
    {
        if (_started.TrySet())
        {
            sessionContext.DeviceSessionRevoked += OnDeviceSessionRevoked;
        }
    }

    private void OnDeviceSessionRevoked(object? sender, EventArgs e) => _ = SignOutAsync();

    private async Task SignOutAsync()
    {
        if (!_signingOut.TrySet())
        {
            return;
        }

        await UiThreadDispatcher.SafeInvokeAsync(async () =>
        {
            try
            {
                await popupService.ShowMessageAsync(
                    "Signed out",
                    "Your session has ended. Chatly will close now; sign in again the next time you start it.");
                await sessionService.LogoutAsync(CancellationToken.None);
            }
            catch (Exception exception)
            {
                LogSignOutFailed(exception);
            }
            finally
            {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.Shutdown();
                }
            }
        });
    }

    [LoggerMessage(LogLevel.Warning, "Failed to log out the revoked device.")]
    private partial void LogSignOutFailed(Exception exception);
}