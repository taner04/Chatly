namespace Chatly.Desktop.Services.Authentication.Storage.MacOs.Interop;

internal sealed class CfDictionary : IDisposable
{
    public CfDictionary()
    {
        Handle =
            CoreFoundationNative.CFDictionaryCreateMutable(
                0,
                0,
                0,
                0);

        if (Handle == 0)
        {
            throw new InvalidOperationException(
                "Failed to create CFDictionary.");
        }
    }

    public nint Handle { get; }

    public void Dispose()
    {
        if (Handle != 0)
        {
            CoreFoundationNative.CFRelease(Handle);
        }
    }

    public void Set(
        nint key,
        nint value)
    {
        ArgumentOutOfRangeException.ThrowIfZero(key);
        ArgumentOutOfRangeException.ThrowIfZero(value);

        CoreFoundationNative.CFDictionarySetValue(
            Handle,
            key,
            value);
    }
}