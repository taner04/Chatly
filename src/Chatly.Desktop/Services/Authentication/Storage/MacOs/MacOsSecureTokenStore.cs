using Chatly.Desktop.Abstraction.Authentication;
using Chatly.Desktop.Options;
using Chatly.Desktop.Services.Authentication.Storage.MacOs.Interop;
using Microsoft.Extensions.Options;

namespace Chatly.Desktop.Services.Authentication.Storage.MacOs;

internal sealed partial class MacOsSecureTokenStore : ISecureTokenStore
{
    private const string Service = "com.chatly.desktop";
    private readonly string _account;

    public MacOsSecureTokenStore(IOptions<DesktopProfileOption> profileOptions)
    {
        var profileName = profileOptions.Value.Name;
        _account = profileName == DesktopProfileOption.DefaultName
            ? "refresh-token"
            : $"refresh-token-{profileName}";
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