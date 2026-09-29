using System.Runtime.CompilerServices;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.Services.Calls;

[SingletonService]
public sealed partial class CallCoordinator(
    ICallingHubServer callingHub,
    CallSession session,
    ILogger<CallCoordinator> logger) : IAsyncDisposable
{
    public CallSnapshot Snapshot => session.Snapshot;

    public async ValueTask DisposeAsync() => await ShutdownAsync();

    public event Action<CallSnapshot>? SnapshotChanged
    {
        add => session.SnapshotChanged += value;
        remove => session.SnapshotChanged -= value;
    }

    public Task StartCallAsync(Guid remoteUserId, CancellationToken cancellationToken = default) =>
        session.RunAsync(async () =>
        {
            if (session.Snapshot.HasCall)
            {
                return;
            }

            var call = await callingHub.StartCallAsync(remoteUserId);
            session.SetCall(call);
            await session.StartOutgoingToneAsync();
        }, cancellationToken);

    public Task AcceptAsync(CancellationToken cancellationToken = default) =>
        session.RunAsync(async () =>
        {
            if (session.Snapshot is not { CallId: { } callId, IsIncoming: true })
            {
                return;
            }

            var call = await callingHub.AcceptCallAsync(callId);
            await session.StopTonesAsync();
            session.SetCall(call);
            await session.EnsureMediaAsync(cancellationToken);
        }, cancellationToken);

    public Task RejectAsync(CancellationToken cancellationToken = default) =>
        session.RunAsync(async () =>
        {
            if (session.Snapshot is not { CallId: { } callId, IsIncoming: true })
            {
                return;
            }

            try
            {
                await callingHub.RejectCallAsync(callId);
            }
            finally
            {
                await session.TeardownAsync(callId);
            }
        }, cancellationToken);

    public Task EndAsync(CancellationToken cancellationToken = default) =>
        session.RunAsync(async () =>
        {
            if (session.Snapshot.CallId is not { } callId)
            {
                return;
            }

            try
            {
                await callingHub.EndCallAsync(callId);
            }
            finally
            {
                await session.TeardownAsync(callId);
            }
        }, cancellationToken);

    public Task SetMutedAsync(bool muted, CancellationToken cancellationToken = default) =>
        session.RunAsync(() => session.SetMutedAsync(muted), cancellationToken);

    public Task ReconcileAsync(CancellationToken cancellationToken = default) =>
        session.RunAsync(() => ReconcileCoreAsync(cancellationToken), cancellationToken);

    public Task ShutdownAsync() => session.StopAsync(ShutdownCoreAsync);

    private async Task ReconcileCoreAsync(CancellationToken cancellationToken)
    {
        var call = await callingHub.GetCurrentCallAsync();
        if (call is null || call.State == CallState.Ended)
        {
            await session.TeardownAsync();
            return;
        }

        if (session.Snapshot.CallId != call.CallId)
        {
            await session.TeardownAsync();
        }

        session.SetCall(call);
        await session.StopTonesAsync();
        if (call.State == CallState.Ringing)
        {
            if (call.Role == CallRole.Receiver)
            {
                await session.StartIncomingToneAsync();
            }
            else
            {
                await session.StartOutgoingToneAsync();
            }

            return;
        }

        await session.EnsureMediaAsync(cancellationToken);
    }

    private async Task ShutdownCoreAsync()
    {
        if (session.Snapshot.CallId is { } callId)
        {
            await CallCoordinatorUtilities.TryInvokeAsync(
                () => callingHub.EndCallAsync(callId),
                LogOperationFailed);
        }

        await session.TeardownAsync();
    }

    [LoggerMessage(LogLevel.Warning, "Call operation {Operation} failed.")]
    private partial void LogOperationFailed(
        Exception exception,
        [CallerMemberName] string operation = "");
}