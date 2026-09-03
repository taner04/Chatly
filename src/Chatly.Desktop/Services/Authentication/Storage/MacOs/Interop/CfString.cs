using System.Text;

namespace Chatly.Desktop.Services.Authentication.Storage.MacOs.Interop;

internal sealed unsafe class CfString : IDisposable
{
    public CfString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var bytes = Encoding.UTF8.GetBytes(value + '\0');

        fixed (byte* ptr = bytes)
        {
            Handle = CoreFoundationNative.CFStringCreateWithCString(
                0,
                ptr,
                CoreFoundationConstants.StringEncodingUtf8);
        }

        if (Handle == 0)
        {
            throw new InvalidOperationException(
                "Failed to create CFString.");
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
}