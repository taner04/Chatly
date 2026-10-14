using System.Globalization;
using System.Threading.RateLimiting;
using Chatly.WebApi.Common.Composition.Options;
using Microsoft.AspNetCore.RateLimiting;

namespace Chatly.WebApi.Common.Composition.Configs;

internal static partial class RateLimiterConfig
{
    public static void Config(
        RateLimiterOptions options,
        RateLimitingOption rateLimiting)
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                RateLimitPartitionKey.ForUser(httpContext.User)
                    // TODO: Behind a reverse proxy RemoteIpAddress is the proxy's address, so every anonymous
                    // caller would share one bucket. UseForwardedHeaders with KnownProxies/KnownNetworks
                    // set deploying behind one.
                    ?? $"ip:{httpContext.Connection.RemoteIpAddress}",
                _ =>
                new FixedWindowRateLimiterOptions
                {
                    PermitLimit = rateLimiting.HttpPermitLimit,
                    Window = rateLimiting.HttpWindow,
                    QueueLimit = 0
                }));

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = async (context, cancellationToken) =>
        {
            var httpContext = context.HttpContext;
            TimeSpan? retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var delay) ? delay : null;

            if (retryAfter is { } value)
            {
                httpContext.Response.Headers.RetryAfter =
                    RateLimitExceededException.ToRetryAfterSeconds(value).ToString(CultureInfo.InvariantCulture);
            }

            var logger = httpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(RateLimiterConfig));
            LogRequestRejected(
                logger,
                httpContext.Request.Method,
                httpContext.Request.Path,
                RateLimitPartitionKey.ForUser(httpContext.User),
                httpContext.Connection.RemoteIpAddress?.ToString(),
                retryAfter);

            var problemDetailsService = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
            _ = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = new RateLimitExceededException(retryAfter)
            });
        };
    }

    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Warning,
        Message = "Rate limit exceeded for {Method} {Path} by {Caller} from {RemoteIp}. Retry after {RetryAfter}.")]
    private static partial void LogRequestRejected(
        ILogger logger,
        string method,
        string path,
        string? caller,
        string? remoteIp,
        TimeSpan? retryAfter);
}
