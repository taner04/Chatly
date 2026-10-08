using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Chatly.WebApi.IntegrationTests.Infrastructure.Mocks.Jwt;

public static class JwtTokenMock
{
    public static readonly SecurityKey SecurityKey =
        new SymmetricSecurityKey("CHATLY_INTEGRATION_TESTS_SIGNING_KEY_32B"u8.ToArray());

    public static string Issuer => TestSettings.OidcAuthority;

    public static string CreateToken(TestUser user, params Claim[] additionalClaims)
    {
        var now = DateTime.UtcNow;
        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = TestSettings.OidcAudience,
            NotBefore = now,
            Expires = now.AddHours(1),
            Subject = new ClaimsIdentity(
                [new Claim("sub", user.Sub), new Claim("email", user.Email), .. additionalClaims]),
            SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256)
        });
    }

    public static string CreateLogoutToken(
        string identitySessionId,
        string audience = "chatly-desktop",
        bool includeLogoutEvent = true)
    {
        var claims = new Dictionary<string, object>
        {
            ["sid"] = identitySessionId,
            ["jti"] = Guid.NewGuid().ToString()
        };

        if (includeLogoutEvent)
        {
            claims["events"] = new Dictionary<string, object>
            {
                ["http://schemas.openid.net/event/backchannel-logout"] = new Dictionary<string, object>()
            };
        }

        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = audience,
            IssuedAt = DateTime.UtcNow,
            Claims = claims,
            SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256)
        });
    }
}