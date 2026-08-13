using Chatly.Contracts.Users.Results;
using Chatly.WebApi.Common.Infrastructure;
using Chatly.WebApi.Common.Infrastructure.Persistence;
using Chatly.WebApi.Common.Shared.Exceptions;
using Chatly.WebApi.Features.Users.Exceptions;
using Chatly.WebApi.Features.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace Chatly.WebApi.Features.Users.Services;

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

    public CurrentUserResponse CreateResponse(User user)
    {
        var profilePictureUrl = user.ProfilePictureKey is null
            ? null
            : blobService.CreateReadUrl(
                user.ProfilePictureKey,
                TimeSpan.FromMinutes(15)).ToString();

        return new CurrentUserResponse(
            user.Id.Value,
            user.Email,
            user.Username,
            profilePictureUrl,
            user.OnboardingCompleted);
    }
}
