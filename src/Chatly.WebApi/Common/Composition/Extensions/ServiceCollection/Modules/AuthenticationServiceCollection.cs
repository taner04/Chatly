using Chatly.Contracts.Extensions;
using Chatly.WebApi.Common.Composition.Options;
using Chatly.WebApi.Common.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Chatly.WebApi.Common.Composition.Extensions.ServiceCollection.Modules;

internal static class AuthenticationServiceCollection
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddChatlyAuthentication(IConfiguration configuration)
        {
            var auth0 = configuration.GetOption<Auth0Option>();

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = $"https://{auth0.Domain}";
                    options.Audience = auth0.Audience;
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidAudience = auth0.Audience,
                        ValidIssuer = $"https://{auth0.Domain}/",
                        RoleClaimType = CurrentUserService.RoleClaim,
                        NameClaimType = CurrentUserService.SubClaim
                    };
                });

            services.AddAuthorization();

            return services;
        }
    }
}