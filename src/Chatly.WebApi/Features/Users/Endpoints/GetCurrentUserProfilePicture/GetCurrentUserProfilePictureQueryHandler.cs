using Chatly.Contracts.Users.Results;
using Chatly.WebApi.Common.Infrastructure;
using Chatly.WebApi.Common.Infrastructure.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Chatly.WebApi.Features.Users.Endpoints.GetProfilePicture;

public class GetCurrentUserProfilePictureQueryHandler(
    ChatlyDbContext context,
    CurrentUserService currentUser,
    AzureBlobService blobService) : IQueryHandler<GetCurrentUserProfilePictureQuery, GetPictureResponse>
{
    public async ValueTask<GetPictureResponse> Handle(
        GetCurrentUserProfilePictureQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetCurrentUserId();

        var profilePictureKey = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => user.ProfilePictureKey)
            .SingleOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(profilePictureKey))
        {
            return new GetPictureResponse(null!);
        }

        var url = blobService.CreateReadUrl(profilePictureKey);

        return new GetPictureResponse(url?.ToString());
    }
}