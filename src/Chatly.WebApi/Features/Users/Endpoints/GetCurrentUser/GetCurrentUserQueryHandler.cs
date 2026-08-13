using Chatly.Contracts.Users.Results;
using Chatly.WebApi.Common.Infrastructure;
using Chatly.WebApi.Common.Infrastructure.Persistence;
using Chatly.WebApi.Common.Shared.Exceptions;
using Chatly.WebApi.Features.Users.Models;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Chatly.WebApi.Features.Users.Endpoints.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(ChatlyDbContext context, CurrentUserService currentUser)
    : IQueryHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    public async ValueTask<CurrentUserResponse> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        var userId = currentUser.GetCurrentUserId();

        return await context.Users
                   .AsNoTracking()
                   .Where(user => user.Id == userId)
                   .Select(user => new CurrentUserResponse(
                       user.Id.Value,
                       user.Email,
                       user.Username,
                       user.ProfilePictureKey,
                       user.OnboardingCompleted))
                   .SingleOrDefaultAsync(cancellationToken)
               ?? throw new EntityNotFoundException<User>(userId.Value);
    }
}