using Chatly.WebApi.Features.Users.Exceptions;
using Chatly.WebApi.Features.Users.Services.ProfilePictures;

namespace Chatly.WebApi.Features.Users.Services;

[ScopedService]
internal sealed class UserService(
    ChatlyDbContext context,
    CurrentUserService currentUser,
    ProfilePictureUrlFactory profilePictureUrlFactory)
{
    internal async Task<User> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var userId = currentUser.GetCurrentUserId();

        return await context.Users
                   .Include(user => user.ProfilePictureFile)
                   .SingleOrDefaultAsync(
                       user => user.Id == userId,
                       cancellationToken)
               ?? throw new EntityNotFoundException<User>(userId.Value);
    }

    internal async Task UpdateUsernameAsync(
        User user,
        string username,
        CancellationToken cancellationToken)
    {
        var usernameExists = await context.Users.AnyAsync(
            candidate => candidate.Id != user.Id && candidate.Username == username,
            cancellationToken);

        if (usernameExists)
        {
            throw new UsernameAlreadyExistsException(username);
        }

        user.Username = username;
    }

    internal CurrentUserResponse CreateResponse(User user) =>
        new(
            user.Id.Value,
            user.Email,
            user.Username,
            profilePictureUrlFactory.CreateProfilePictureUrl(user.ProfilePictureFile?.BlobName),
            user.OnboardingCompleted);
}