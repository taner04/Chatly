using System.Text;
using Chatly.Desktop.Services.Authentication.Storage.MacOs.Interop;

namespace Chatly.Desktop.Services.Authentication.Storage.MacOs;

internal sealed partial class MacOsSecureTokenStore
{
    public Task<string?> TryReadRefreshTokenAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var service = new CfString(Service);
        using var account = new CfString(_account);
        using var query = CreateIdentityQuery(service.Handle, account.Handle);

        query.Set(SecurityConstants.SecReturnData, CoreFoundationGlobals.True);
        query.Set(SecurityConstants.SecMatchLimit, SecurityConstants.SecMatchLimitOne);

        var status = SecurityNative.SecItemCopyMatching(query.Handle, out var result);
        if (status == SecurityNative.ErrSecItemNotFound)
        {
            return Task.FromResult<string?>(null);
        }

        if (status != SecurityNative.ErrSecSuccess)
        {
            throw CreateKeychainException("Failed to read refresh token", status);
        }

        try
        {
            var data = CfData.ToArray(result);
            return Task.FromResult<string?>(Encoding.UTF8.GetString(data));
        }
        finally
        {
            if (result != 0)
            {
                CoreFoundationNative.CFRelease(result);
            }
        }
    }
}