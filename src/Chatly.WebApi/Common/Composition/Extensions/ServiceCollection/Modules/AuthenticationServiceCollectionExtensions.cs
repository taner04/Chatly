using Chatly.Shared.Extensions;
using Chatly.WebApi.Common.Composition.Options;
using Chatly.WebApi.Features.DeviceSessions.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Chatly.WebApi.Common.Composition.Extensions.ServiceCollection.Modules;

internal static class AuthenticationServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddChatlyAuthentication(IConfiguration configuration)
        {
            var oidc = configuration.GetOption<OidcOption>();

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = oidc.Authority;
                    options.Audience = oidc.Audience;
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidAudience = oidc.Audience,
                        ValidIssuer = oidc.Authority,
                        NameClaimType = CurrentUserService.SubClaim
                    };
                });

            services.AddAuthorization();
            services.AddHttpClient<IdentitySessionClient>();

            return services;
        }
    }
}