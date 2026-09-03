using System.Runtime.InteropServices;

namespace Chatly.Desktop.Services.Authentication.Storage.MacOs.Interop;

internal static partial class SecurityNative
{
    internal const int ErrSecSuccess = 0;
    internal const int ErrSecDuplicateItem = -25299;
    internal const int ErrSecItemNotFound = -25300;

    private const string SecurityFramework =
        "/System/Library/Frameworks/Security.framework/Security";

    [LibraryImport(SecurityFramework)]
    internal static partial int SecItemAdd(
        nint attributes,
        out nint result);

    [LibraryImport(SecurityFramework)]
    internal static partial int SecItemCopyMatching(
        nint query,
        out nint result);

    [LibraryImport(SecurityFramework)]
    internal static partial int SecItemUpdate(
        nint query,
        nint attributesToUpdate);

    [LibraryImport(SecurityFramework)]
    internal static partial int SecItemDelete(
        nint query);
}