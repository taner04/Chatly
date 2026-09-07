using System.Runtime.InteropServices;

namespace Chatly.Desktop.Services.Authentication.Storage.MacOs.Interop;

internal static class CoreFoundationGlobals
{
    private const string CoreFoundationFramework =
        "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    private static readonly nint LibraryHandle =
        NativeLibrary.Load(CoreFoundationFramework);

    internal static readonly nint True =
        GetConstant("kCFBooleanTrue");

    private static unsafe nint GetConstant(string name)
    {
        var export = NativeLibrary.GetExport(LibraryHandle, name);
        return *(nint*)export;
    }
}