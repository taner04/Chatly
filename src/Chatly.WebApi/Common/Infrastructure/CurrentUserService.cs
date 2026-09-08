namespace Chatly.WebApi.Common.Infrastructure;

[ScopedService]
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor)
{
    internal const string SubClaim = "sub";
    internal const string EmailClaim = "email";
    internal const string RoleClaim = "permissions";

    private HttpContext HttpContext => httpContextAccessor.HttpContext ??
                                       throw new InvalidOperationException("HTTP context is not available.");

    internal string GetAuth0Id()
    {
        return GetClaimValue<string>(SubClaim);
    }

    internal UserId GetCurrentUserId()
    {
        if (HttpContext!.Items.TryGetValue("UserId", out var id)
            && id is UserId userId)
        {
            return userId;
        }

        throw new UnauthorizedAccessException("User is not authenticated.");
    }

    internal T GetClaimValue<T>(
        string claimType)
    {
        var claimValue =
            HttpContext.User.FindFirst(claimType)?.Value ??
            throw new UnauthorizedAccessException($"Claim '{claimType}' is missing.");

        return (T)Convert.ChangeType(claimValue, typeof(T));
    }
}
