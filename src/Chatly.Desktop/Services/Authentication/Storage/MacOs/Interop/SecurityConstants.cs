using System.Runtime.InteropServices;

namespace Chatly.Desktop.Services.Authentication.Storage.MacOs.Interop;

internal static class SecurityConstants
{
    private const string SecurityFramework =
        "/System/Library/Frameworks/Security.framework/Security";

    private static readonly nint LibraryHandle =
        NativeLibrary.Load(SecurityFramework);

    internal static readonly nint SecClass =
        GetConstant("kSecClass");

    internal static readonly nint SecClassGenericPassword =
        GetConstant("kSecClassGenericPassword");

    internal static readonly nint SecAttrService =
        GetConstant("kSecAttrService");

    internal static readonly nint SecAttrAccount =
        GetConstant("kSecAttrAccount");

    internal static readonly nint SecValueData =
        GetConstant("kSecValueData");

    internal static readonly nint SecReturnData =
        GetConstant("kSecReturnData");

    internal static readonly nint SecMatchLimit =
        GetConstant("kSecMatchLimit");

    internal static readonly nint SecMatchLimitOne =
        GetConstant("kSecMatchLimitOne");

    private static unsafe nint GetConstant(string name)
    {
        var export = NativeLibrary.GetExport(LibraryHandle, name);

        // These symbols are exported as CFTypeRef variables.
        return *(nint*)export;
    }
}