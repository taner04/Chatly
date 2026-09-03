using Chatly.Desktop.Services.Authentication.Storage.MacOs.Interop;

namespace Chatly.Desktop.Services.Authentication.Storage.MacOs;

internal sealed partial class MacOsSecureTokenStore
{
    public Task DeleteRefreshTokenAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var service = new CfString(Service);
        using var account = new CfString(_account);
        using var query = CreateIdentityQuery(service.Handle, account.Handle);

        var status = SecurityNative.SecItemDelete(query.Handle);
        if (status is not (SecurityNative.ErrSecSuccess or SecurityNative.ErrSecItemNotFound))
        {
            throw CreateKeychainException("Failed to delete refresh token", status);
        }

        return Task.CompletedTask;
    }
}