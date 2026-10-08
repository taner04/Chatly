namespace Chatly.Desktop.Utilities;

internal sealed class AtomicFlag
{
    private int _value;

    internal bool TrySet() => Interlocked.Exchange(ref _value, 1) == 0;

    internal bool TryReset() => Interlocked.Exchange(ref _value, 0) == 1;
}