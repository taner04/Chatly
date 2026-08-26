using System.Runtime.InteropServices;

namespace Chatly.Desktop.Infrastructure.Authentication.Storage.MacOs.Native;

internal static class SecurityConstants
{
    private static readonly nint Handle =
        NativeLibrary.Load(SecurityNative.Library);

    // Item classes

    internal static readonly nint SecClass =
        GetReference("kSecClass");

    internal static readonly nint SecClassGenericPassword =
        GetReference("kSecClassGenericPassword");

    // Item attributes

    internal static readonly nint SecAttrService =
        GetReference("kSecAttrService");

    internal static readonly nint SecAttrAccount =
        GetReference("kSecAttrAccount");

    // Secret value

    internal static readonly nint SecValueData =
        GetReference("kSecValueData");

    // Return options

    internal static readonly nint SecReturnData =
        GetReference("kSecReturnData");

    internal static readonly nint SecReturnAttributes =
        GetReference("kSecReturnAttributes");

    internal static readonly nint SecReturnRef =
        GetReference("kSecReturnRef");

    internal static readonly nint SecReturnPersistentRef =
        GetReference("kSecReturnPersistentRef");

    // Matching

    internal static readonly nint SecMatchLimit =
        GetReference("kSecMatchLimit");

    internal static readonly nint SecMatchLimitOne =
        GetReference("kSecMatchLimitOne");

    private static nint GetReference(string name)
    {
        var symbol = NativeLibrary.GetExport(Handle, name);

        return Marshal.ReadIntPtr(symbol);
    }
}