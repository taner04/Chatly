namespace Chatly.WebApi.Common.Infrastructure;

[ScopedService]
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor)
{
    public const string SubClaim = "sub";
    public const string EmailClaim = "email";
    public const string RoleClaim = "permissions";

    private HttpContext HttpContext => httpContextAccessor.HttpContext ??
                                       throw new InvalidOperationException("HTTP context is not available.");

    public string GetAuth0Id()
    {
        return GetClaimValue<string>(SubClaim);
    }

    public UserId GetCurrentUserId()
    {
        if (HttpContext!.Items.TryGetValue("UserId", out var id)
            && id is UserId userId)
        {
            return userId;
        }

        throw new UnauthorizedAccessException("User is not authenticated.");
    }

    public T GetClaimValue<T>(
        string claimType)
    {
        var claimValue =
            HttpContext.User.FindFirst(claimType)?.Value ??
            throw new UnauthorizedAccessException($"Claim '{claimType}' is missing.");

        return (T)Convert.ChangeType(claimValue, typeof(T));
    }
}