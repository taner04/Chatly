namespace Chatly.Desktop.Infrastructure.Authentication.Storage.MacOs.Native;

internal static class SecurityStatus
{
    /// <summary>
    /// The operation completed successfully.
    /// </summary>
    internal const int Success = 0;

    /// <summary>
    /// An item with the same unique attributes already exists.
    /// </summary>
    internal const int DuplicateItem = -25299;

    /// <summary>
    /// No matching keychain item was found.
    /// </summary>
    internal const int ItemNotFound = -25300;
}