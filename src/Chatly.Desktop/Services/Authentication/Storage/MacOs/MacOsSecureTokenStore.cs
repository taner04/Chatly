using System.Text;
using Chatly.Desktop.Abstraction.Authentication;
using Chatly.Desktop.Options;
using Chatly.Desktop.Services.Authentication.Storage.MacOs.Interop;
using Chatly.Desktop.Services.Authentication.Storage.MacOs.Interop.DataTypes;
using Microsoft.Extensions.Options;

namespace Chatly.Desktop.Services.Authentication.Storage.MacOs;

internal sealed class MacOsSecureTokenStore : ISecureTokenStore
{
    private const string Service = "com.chatly.desktop";
    private readonly string _account;

    public MacOsSecureTokenStore(IOptions<DesktopProfileOption> profileOptions)
    {
        var profileName = profileOptions.Value.Name;
        _account = profileName == DesktopProfileOption.DefaultName ? "refresh-token" : $"refresh-token-{profileName}";
    }

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

    private static CfDictionary CreateIdentityQuery(nint service, nint account)
    {
        var query = new CfDictionary();
        query.Set(SecurityConstants.SecClass, SecurityConstants.SecClassGenericPassword);
        query.Set(SecurityConstants.SecAttrService, service);
        query.Set(SecurityConstants.SecAttrAccount, account);
        return query;
    }

    private static InvalidOperationException CreateKeychainException(string message, int status)
    {
        return new InvalidOperationException($"{message}. OSStatus: {status}.");
    }
}