namespace Chatly.Desktop.Services.Authentication.Storage.MacOs.Interop;

internal sealed unsafe class CfData : IDisposable
{
    public CfData(ReadOnlySpan<byte> data)
    {
        fixed (byte* ptr = data)
        {
            Handle = CoreFoundationNative.CFDataCreate(
                0,
                ptr,
                data.Length);
        }

        if (Handle == 0)
        {
            throw new InvalidOperationException(
                "Failed to create CFData.");
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

    public static byte[] ToArray(nint handle)
    {
        if (handle == 0)
        {
            return [];
        }

        var length =
            CoreFoundationNative.CFDataGetLength(handle);

        if (length <= 0)
        {
            return [];
        }

        var source =
            CoreFoundationNative.CFDataGetBytePtr(handle);

        if (source == 0)
        {
            return [];
        }

        var result = new byte[(int)length];

        fixed (byte* destination = result)
        {
            Buffer.MemoryCopy(
                (void*)source,
                destination,
                result.Length,
                result.Length);
        }

        return result;
    }
}