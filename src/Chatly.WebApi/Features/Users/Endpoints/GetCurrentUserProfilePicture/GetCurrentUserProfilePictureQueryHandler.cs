using Chatly.WebApi.Features.Users.Services.ProfilePictures;
using Chatly.WebApi.Features.Users.Services.Profiles;

namespace Chatly.WebApi.Features.Users.Endpoints.GetCurrentUserProfilePicture;

internal sealed class GetCurrentUserProfilePictureQueryHandler(
    ChatlyDbContext context,
    CurrentUserService currentUser,
    ProfilePictureUrlFactory profilePictureUrlFactory)
    : IQueryHandler<GetCurrentUserProfilePictureQuery, GetCurrentUserProfilePictureResponse>
{
    public async ValueTask<GetCurrentUserProfilePictureResponse> Handle(
        GetCurrentUserProfilePictureQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;

        var profile = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .SelectProfile()
            .SingleOrDefaultAsync(cancellationToken);

        return new GetCurrentUserProfilePictureResponse(
            profilePictureUrlFactory.CreateProfilePictureUrl(profile?.ProfilePictureKey));
    }
}