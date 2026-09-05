using Chatly.WebApi.Features.Users.Exceptions;

namespace Chatly.WebApi.Features.Users.Services;

[ScopedService]
public sealed class UserService(
    ChatlyDbContext context,
    CurrentUserService currentUser,
    AzureBlobService blobService)
{
    public async Task<User> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var userId = currentUser.GetCurrentUserId();

        return await context.Users.SingleOrDefaultAsync(
                   user => user.Id == userId,
                   cancellationToken)
               ?? throw new EntityNotFoundException<User>(userId.Value);
    }

    public async Task UpdateUsernameAsync(
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

    public async Task<List<UserId>> GetProfileUpdateRecipientIdsAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        var recipientIds = await context.Friendships
            .AsNoTracking()
            .Where(friendship =>
                friendship.FirstUserId == userId ||
                friendship.SecondUserId == userId)
            .Select(friendship => friendship.FirstUserId == userId
                ? friendship.SecondUserId
                : friendship.FirstUserId)
            .ToListAsync(cancellationToken);

        recipientIds.Add(userId);
        return recipientIds;
    }

    public CurrentUserResponse CreateResponse(User user)
    {
        return new CurrentUserResponse(
            user.Id.Value,
            user.Email,
            user.Username,
            blobService.CreateReadUrl(user.ProfilePictureKey)?.ToString(),
            user.OnboardingCompleted);
    }
}