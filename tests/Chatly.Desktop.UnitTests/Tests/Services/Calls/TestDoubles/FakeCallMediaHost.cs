using Chatly.Desktop.Abstraction.Calls;
using Chatly.Desktop.Models.Calls;

namespace Chatly.Desktop.UnitTests.Tests.Services.Calls.TestDoubles;

internal sealed class FakeCallMediaHost : ICallMediaHost
{
    internal List<(string ServerUrl, string Token)> Joins { get; } = [];

    internal List<CallAudioOptions> AudioOptions { get; } = [];

    internal int LeaveCount { get; private set; }

    internal List<bool> MicrophoneStates { get; } = [];

    public event EventHandler? RemoteParticipantJoined;

    public event EventHandler? RemoteParticipantLeft;

    public event EventHandler<string>? Disconnected;

    public event EventHandler<bool>? ReconnectingChanged;

    public Task JoinAsync(
        string serverUrl,
        string token,
        CallAudioOptions audioOptions,
        CancellationToken cancellationToken)
    {
        Joins.Add((serverUrl, token));
        AudioOptions.Add(audioOptions);
        return Task.CompletedTask;
    }

    public Task ApplyAudioOptionsAsync(CallAudioOptions audioOptions)
    {
        AudioOptions.Add(audioOptions);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CallAudioDevice>> GetAudioDevicesAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<CallAudioDevice>>([]);

    public Task SetMicrophoneEnabledAsync(bool enabled)
    {
        MicrophoneStates.Add(enabled);
        return Task.CompletedTask;
    }

    public Task LeaveAsync()
    {
        LeaveCount++;
        return Task.CompletedTask;
    }

    internal void RaiseRemoteParticipantJoined() => RemoteParticipantJoined?.Invoke(this, EventArgs.Empty);

    internal void RaiseRemoteParticipantLeft() => RemoteParticipantLeft?.Invoke(this, EventArgs.Empty);

    internal void RaiseDisconnected(string reason) => Disconnected?.Invoke(this, reason);

    internal void RaiseReconnectingChanged(bool isReconnecting) => ReconnectingChanged?.Invoke(this, isReconnecting);
}