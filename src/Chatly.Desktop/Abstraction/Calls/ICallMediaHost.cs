using Chatly.Desktop.Models.Calls;

namespace Chatly.Desktop.Abstraction.Calls;

public interface ICallMediaHost
{
    event EventHandler? RemoteParticipantJoined;

    event EventHandler? RemoteParticipantLeft;

    event EventHandler<string>? Disconnected;

    event EventHandler<bool>? ReconnectingChanged;

    Task JoinAsync(string serverUrl, string token, CallAudioOptions audioOptions, CancellationToken cancellationToken);

    Task ApplyAudioOptionsAsync(CallAudioOptions audioOptions);

    Task<IReadOnlyList<CallAudioDevice>> GetAudioDevicesAsync(CancellationToken cancellationToken);

    Task SetMicrophoneEnabledAsync(bool enabled);

    Task LeaveAsync();
}