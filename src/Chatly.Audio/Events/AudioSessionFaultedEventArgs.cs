namespace Chatly.Audio.Events;

public sealed class AudioSessionFaultedEventArgs(Exception exception) : EventArgs
{
    public Exception Exception { get; } = exception;
}