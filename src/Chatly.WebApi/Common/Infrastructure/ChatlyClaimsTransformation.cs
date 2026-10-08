using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Chatly.WebApi.Common.Infrastructure;

[ScopedService(typeof(IClaimsTransformation))]
internal sealed partial class ChatlyClaimsTransformation(
    ChatlyDbContext context,
    ILogger<ChatlyClaimsTransformation> logger) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (CurrentUserService.FindUserId(principal) is not null
            || principal.FindFirst(CurrentUserService.SubClaim)?.Value is not { } identityId)
        {
            return principal;
        }

        var userId = await FindUserIdAsync(identityId) ?? await CreateUserAsync(principal, identityId);

        var transformed = principal.Clone();
        transformed.AddIdentity(new ClaimsIdentity(
            [new Claim(CurrentUserService.UserIdClaim, userId.Value.ToString())],
            CurrentUserService.ChatlyAuthenticationType));
        return transformed;
    }

    private Task<UserId?> FindUserIdAsync(string identityId) =>
        context.Users
            .AsNoTracking()
            .Where(user => user.IdentityId == identityId)
            .Select(user => (UserId?)user.Id)
            .SingleOrDefaultAsync();

    private async Task<UserId> CreateUserAsync(ClaimsPrincipal principal, string identityId)
    {
        var email = principal.FindFirst(CurrentUserService.EmailClaim)?.Value
                    ?? throw new UnauthorizedAccessException($"Claim '{CurrentUserService.EmailClaim}' is missing.");
        var user = new User(email, identityId);
        context.Users.Add(user);

        try
        {
            await context.SaveChangesAsync();
            LogUserCreated(identityId);
            return user.Id;
        }
        catch (DbUpdateException exception) when (exception.IsUniqueViolation())
        {
            context.Entry(user).State = EntityState.Detached;
            return await FindUserIdAsync(identityId)
                   ?? throw new InvalidOperationException("The concurrently created user was not found.");
        }
    }

    [LoggerMessage(LogLevel.Information, "User with IdentityId '{IdentityId}' created.")]
    private partial void LogUserCreated(string identityId);
}