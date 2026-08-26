using System.Runtime.InteropServices;

namespace Chatly.Desktop.Infrastructure.Authentication.Storage.MacOs.Native;

internal static partial class SecurityNative
{
    internal const string Library =
        "/System/Library/Frameworks/Security.framework/Security";

    /// <summary>
    /// Adds one or more items to the keychain.
    /// </summary>
    [LibraryImport(Library)]
    internal static partial int SecItemAdd(
        nint attributes,
        out nint result);

    /// <summary>
    /// Returns an item that matches the supplied query.
    /// </summary>
    [LibraryImport(Library)]
    internal static partial int SecItemCopyMatching(
        nint query,
        out nint result);

    /// <summary>
    /// Updates items that match the supplied query.
    /// </summary>
    [LibraryImport(Library)]
    internal static partial int SecItemUpdate(
        nint query,
        nint attributesToUpdate);

    /// <summary>
    /// Deletes items that match the supplied query.
    /// </summary>
    [LibraryImport(Library)]
    internal static partial int SecItemDelete(
        nint query);
}