using System.Net;

namespace Chatly.WebApi.Common.Shared.Exceptions;

internal sealed class RateLimitExceededException(TimeSpan? retryAfter)
    : ChatlyException(
        "Too many requests",
        retryAfter is { } delay
            ? $"Too many requests. Please try again in {ToRetryAfterSeconds(delay)} seconds."
            : "Too many requests. Please try again later.",
        "RateLimit.Exceeded",
        HttpStatusCode.TooManyRequests)
{
    internal static int ToRetryAfterSeconds(TimeSpan retryAfter) => (int)Math.Ceiling(retryAfter.TotalSeconds);
}
