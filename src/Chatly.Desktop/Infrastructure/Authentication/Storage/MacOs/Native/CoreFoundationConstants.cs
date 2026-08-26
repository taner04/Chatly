using System.Runtime.InteropServices;

namespace Chatly.Desktop.Infrastructure.Authentication.Storage.MacOs.Native;

internal static class CoreFoundationConstants
{
    private static readonly nint Handle =
        NativeLibrary.Load(CoreFoundationNative.Library);

    /// <summary>
    /// CoreFoundation true value.
    /// </summary>
    internal static readonly nint BooleanTrue =
        GetReference("kCFBooleanTrue");

    /// <summary>
    /// Default CoreFoundation callbacks for dictionary keys.
    /// </summary>
    internal static readonly nint TypeDictionaryKeyCallBacks =
        NativeLibrary.GetExport(
            Handle,
            "kCFTypeDictionaryKeyCallBacks");

    /// <summary>
    /// Default CoreFoundation callbacks for dictionary values.
    /// </summary>
    internal static readonly nint TypeDictionaryValueCallBacks =
        NativeLibrary.GetExport(
            Handle,
            "kCFTypeDictionaryValueCallBacks");

    private static nint GetReference(string name)
    {
        var symbol = NativeLibrary.GetExport(Handle, name);

        return Marshal.ReadIntPtr(symbol);
    }
}