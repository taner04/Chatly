using Chatly.Rtc.Enums;

namespace Chatly.Rtc.Events;

public sealed class PeerConnectionStateChangedEventArgs(
    PeerConnectionState state,
    Exception? exception = null) : EventArgs
{
    public PeerConnectionState State { get; } = state;
    public Exception? Exception { get; } = exception;
}