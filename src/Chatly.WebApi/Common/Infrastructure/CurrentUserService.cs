using System.Security.Claims;
using Chatly.WebApi.Features.DeviceSessions.Models;
using Chatly.WebApi.Features.DeviceSessions.Services;

namespace Chatly.WebApi.Common.Infrastructure;

[ScopedService]
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor)
{
    internal const string UserIdClaim = "chatly_user_id";
    internal const string ChatlyAuthenticationType = "Chatly";
    internal const string SubClaim = "sub";
    internal const string SessionIdClaim = "sid";
    internal const string EmailClaim = "email";

    internal UserId UserId =>
        FindUserId(HttpContext.User) ?? throw new UnauthorizedAccessException("User is not authenticated.");

    internal DeviceInfo Device => field ??= DeviceInfoReader.Read(HttpContext.Request.Headers);

    internal string? IdentitySessionId => FindIdentitySessionId(HttpContext.User);

    private HttpContext HttpContext => httpContextAccessor.HttpContext ??
                                       throw new UnauthorizedAccessException(
                                           "No user is associated with the current operation.");

    internal static UserId? FindUserId(ClaimsPrincipal? principal) =>
        principal?.Identities
            .FirstOrDefault(identity => identity.AuthenticationType == ChatlyAuthenticationType)?
            .FindFirst(UserIdClaim)?.Value is { } value
        && Guid.TryParse(value, out var userId)
            ? UserId.From(userId)
            : null;

    internal static string? FindIdentitySessionId(ClaimsPrincipal? principal) =>
        principal?.FindFirst(SessionIdClaim)?.Value;
}