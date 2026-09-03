using System.Text;

namespace Chatly.Desktop.Services.Authentication.Storage.MacOs.Interop.DataTypes;

internal sealed unsafe class CfString : Cf
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
    }
}