using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Chatly.WebApi.IntegrationTests.Infrastructure.Mocks.Jwt;

public static class JwtBearerOptionsMock
{
    internal static IServiceCollection AddMockJwtBearerOptions(this IServiceCollection services)
    {
        services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            var configuration = new OpenIdConnectConfiguration
            {
                Issuer = JwtTokenMock.Issuer
            };

            configuration.SigningKeys.Add(JwtTokenMock.SecurityKey);
            options.Configuration = configuration;
        });

        return services;
    }
}
