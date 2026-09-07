namespace Chatly.WebApi.Features.Users.Endpoints.GetCurrentUserProfilePicture;

public class GetCurrentUserProfilePictureQueryHandler(
    ChatlyDbContext context,
    CurrentUserService currentUser,
    AzureBlobService blobService)
    : IQueryHandler<GetCurrentUserProfilePictureQuery, GetCurrentUserProfilePictureResponse>
{
    public async ValueTask<GetCurrentUserProfilePictureResponse> Handle(
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
            return new GetCurrentUserProfilePictureResponse(null!);
        }

        var url = blobService.CreateReadUrl(profilePictureKey);

        return new GetCurrentUserProfilePictureResponse(url?.ToString());
    }
}