using System.Threading.Tasks;
using Chatly.Desktop.Options;
using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient;
using Microsoft.Extensions.Options;

namespace Chatly.Desktop.Infrastructure.Auth;

public sealed class AuthenticationService
{
    private readonly Auth0Option _options;
    private readonly OidcClient _client;

    public AuthenticationService(IOptions<Auth0Option> auth0Options)
    {
        _options = auth0Options.Value;
        _client = new OidcClient(new OidcClientOptions
        {
            Authority = $"https://{_options.Domain}",
            ClientId = _options.ClientId,
            Scope = _options.Scope,
            RedirectUri = _options.RedirectUri,
            Browser = new AvaloniaAuthenticationBrowser()
        });
    }

    public async Task<LoginResult> LoginAsync()
    {
        var loginRequest = new LoginRequest
        {
            FrontChannelExtraParameters = new Parameters
            {
                { "connection", _options.ConnectionName },
                { "audience", _options.Audience }
            }
        };
        
        return await _client.LoginAsync(loginRequest);
    }
    
    public async Task LogoutAsync()
    {
        await _client.LogoutAsync();
    }
}
