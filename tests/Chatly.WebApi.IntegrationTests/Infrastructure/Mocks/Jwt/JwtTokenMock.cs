using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Chatly.WebApi.IntegrationTests.Infrastructure.Mocks.Jwt;

public static class JwtTokenMock
{
    public static readonly SecurityKey SecurityKey =
        new SymmetricSecurityKey("CHATLY_INTEGRATION_TESTS_SIGNING_KEY_32B"u8.ToArray());

    public static string Issuer => $"https://{TestSettings.Auth0Domain}/";

    public static string CreateToken(TestUser user)
    {
        var now = DateTime.UtcNow;
        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = TestSettings.Auth0Audience,
            NotBefore = now,
            Expires = now.AddHours(1),
            Subject = new ClaimsIdentity([new Claim("sub", user.Sub), new Claim("email", user.Email)]),
            SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256)
        });
    }
}