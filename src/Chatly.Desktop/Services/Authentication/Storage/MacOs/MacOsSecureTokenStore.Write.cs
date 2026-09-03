using System.Text;
using Chatly.Desktop.Services.Authentication.Storage.MacOs.Interop;

namespace Chatly.Desktop.Services.Authentication.Storage.MacOs;

internal sealed partial class MacOsSecureTokenStore
{
    public Task SaveRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        cancellationToken.ThrowIfCancellationRequested();

        var data = Encoding.UTF8.GetBytes(refreshToken);
        using var service = new CfString(Service);
        using var account = new CfString(_account);
        using var tokenData = new CfData(data);
        using var query = CreateIdentityQuery(service.Handle, account.Handle);
        using var update = new CfDictionary();

        update.Set(SecurityConstants.SecValueData, tokenData.Handle);

        var status = SecurityNative.SecItemUpdate(query.Handle, update.Handle);
        if (status == SecurityNative.ErrSecSuccess)
        {
            return Task.CompletedTask;
        }

        if (status != SecurityNative.ErrSecItemNotFound)
        {
            throw CreateKeychainException("Failed to update refresh token", status);
        }

        using var item = new CfDictionary();
        item.Set(SecurityConstants.SecClass, SecurityConstants.SecClassGenericPassword);
        item.Set(SecurityConstants.SecAttrService, service.Handle);
        item.Set(SecurityConstants.SecAttrAccount, account.Handle);
        item.Set(SecurityConstants.SecValueData, tokenData.Handle);

        status = SecurityNative.SecItemAdd(item.Handle, out var result);
        if (result != 0)
        {
            CoreFoundationNative.CFRelease(result);
        }

        if (status != SecurityNative.ErrSecSuccess)
        {
            throw CreateKeychainException("Failed to save refresh token", status);
        }

        return Task.CompletedTask;
    }
}