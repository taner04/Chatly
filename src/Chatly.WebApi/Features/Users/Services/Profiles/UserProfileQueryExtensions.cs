namespace Chatly.WebApi.Features.Users.Services.Profiles;

internal static class UserProfileQueryExtensions
{
    extension(IQueryable<User> users)
    {
        internal IQueryable<UserProfileRow> SelectProfile()
        {
            return users.Select(user => new UserProfileRow(
                user.Id,
                user.Username,
                user.ProfilePictureFile == null
                    ? null
                    : user.ProfilePictureFile.BlobName));
        }
    }
}