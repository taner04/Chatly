namespace Chatly.Desktop.Services.Authentication.Storage.MacOs.Interop.DataTypes;

internal abstract class Cf : IDisposable
{
    private nint _handle;

    public nint Handle
    {
        get => _handle;
        protected init
        {
            ArgumentOutOfRangeException.ThrowIfZero(value);
            _handle = value;
        }
    }

    public void Dispose()
    {
        var handle = Interlocked.Exchange(ref _handle, 0);
        if (handle != 0)
        {
            CoreFoundationNative.CFRelease(handle);
        }

        GC.SuppressFinalize(this);
    }
}
