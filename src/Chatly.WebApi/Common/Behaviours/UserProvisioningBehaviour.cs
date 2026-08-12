using Chatly.WebApi.Common.Infrastructure;
using Chatly.WebApi.Common.Infrastructure.Persistence;
using Chatly.WebApi.Features.Users.Models;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Chatly.WebApi.Common.Behaviours;

public sealed partial class UserProvisioningBehaviour<TMessage, TResponse>(
    ILogger<UserProvisioningBehaviour<TMessage, TResponse>> logger,
    CurrentUserService currentUserService,
    ChatlyDbContext context,
    IHttpContextAccessor accessor
) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var auth0Id = currentUserService.GetAuth0Id();

        var user = await context.Users
            .SingleOrDefaultAsync(u => u.Auth0Id == auth0Id, cancellationToken);

        if (user is null)
        {
            LogUserNotFound(auth0Id);

            user = new User(
                currentUserService.GetClaimValue<string>(CurrentUserService.EmailClaim),
                auth0Id
            );

            context.Users.Add(user);
            await context.SaveChangesAsync(cancellationToken);

            LogUserCreated(auth0Id);
        }
        else
        {
            LogUserFound(auth0Id);
        }

        accessor.HttpContext!.Items["UserId"] = user.Id;

        return await next(message, cancellationToken);
    }

    [LoggerMessage(0, LogLevel.Warning, "User with Auth0Id '{Auth0Id}' not found.")]
    private partial void LogUserNotFound(string auth0Id);

    [LoggerMessage(1, LogLevel.Information, "User with Auth0Id '{Auth0Id}' created.")]
    private partial void LogUserCreated(string auth0Id);

    [LoggerMessage(2, LogLevel.Information, "User with Auth0Id '{Auth0Id}' was found.")]
    private partial void LogUserFound(string auth0Id);
}