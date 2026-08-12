using Chatly.Desktop.Features.Users;
using Chatly.Desktop.Features.Users.Api;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chatly.Desktop.Features.Authentication.Services;

public sealed class SessionService(
    AuthenticationService authenticationService,
    UserWebService userWebService,
    UserSessionContext sessionContext)
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var accessToken = await authenticationService.AuthenticateAsync(cancellationToken);

        sessionContext.SetAccessToken(accessToken);
        try
        {
            var userResult = await userWebService.GetCurrentUserAsync(cancellationToken);
            if (userResult.IsFailure)
            {
                throw new InvalidOperationException(
                    $"Failed to retrieve the current user: {userResult.Error.Detail}");
            }

            sessionContext.SetAuthenticated(userResult.Value);
        }
        catch
        {
            sessionContext.Clear();
            throw;
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        try
        {
            await authenticationService.LogoutAsync(cancellationToken);
        }
        finally
        {
            sessionContext.Clear();
        }
    }
}
