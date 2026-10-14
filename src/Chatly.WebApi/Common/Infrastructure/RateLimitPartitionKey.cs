using System.Security.Claims;

namespace Chatly.WebApi.Common.Infrastructure;

internal static class RateLimitPartitionKey
{
    // Server-verified identity only; client-supplied values such as the device id header
    // would let a caller rotate them to get a fresh bucket per request.
    internal static string? ForUser(ClaimsPrincipal? user) =>
        CurrentUserService.FindUserId(user) is { } userId
            ? $"user:{userId}"
            : user?.FindFirst(CurrentUserService.SubClaim)?.Value is { } subject
                ? $"sub:{subject}"
                : null;
}
