using System.Threading.RateLimiting;
using Chatly.WebApi.Common.Composition.Options;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace Chatly.WebApi.Features.Hubs.HubFilters;

internal sealed partial class RateLimiterHubFilter(
    IOptions<RateLimitingOption> rateLimitingOptions,
    ILogger<RateLimiterHubFilter> logger) : IHubFilter, IDisposable
{
    private readonly PartitionedRateLimiter<HubCallerContext> _invocationLimiter = CreateLimiter(
        rateLimitingOptions.Value.HubPermitLimit,
        rateLimitingOptions.Value.HubWindow);

    private readonly PartitionedRateLimiter<HubCallerContext> _connectionLimiter = CreateLimiter(
        rateLimitingOptions.Value.HubConnectionPermitLimit,
        rateLimitingOptions.Value.HubConnectionWindow);

    private static PartitionedRateLimiter<HubCallerContext> CreateLimiter(
        int permitLimit,
        TimeSpan window) =>
        PartitionedRateLimiter.Create<HubCallerContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                RateLimitPartitionKey.ForUser(context.User)
                    ?? $"connection:{context.ConnectionId}",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = window,
                    QueueLimit = 0
                }));

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        using var lease = _invocationLimiter.AttemptAcquire(invocationContext.Context);

        if (lease.IsAcquired)
        {
            return await next(invocationContext);
        }

        var retryAfter = GetRetryAfter(lease);
        LogInvocationRejected(
            invocationContext.Hub.GetType().Name,
            invocationContext.HubMethodName,
            invocationContext.Context.UserIdentifier,
            invocationContext.Context.ConnectionId,
            retryAfter);

        throw new HubException(new RateLimitExceededException(retryAfter).Message);
    }

    public async Task OnConnectedAsync(
        HubLifetimeContext context,
        Func<HubLifetimeContext, Task> next)
    {
        using var lease = _connectionLimiter.AttemptAcquire(context.Context);

        if (lease.IsAcquired)
        {
            await next(context);
            return;
        }

        var retryAfter = GetRetryAfter(lease);
        LogConnectionRejected(
            context.Hub.GetType().Name,
            context.Context.UserIdentifier,
            context.Context.ConnectionId,
            retryAfter);

        throw new HubException(new RateLimitExceededException(retryAfter).Message);
    }

    private static TimeSpan? GetRetryAfter(RateLimitLease lease) =>
        lease.TryGetMetadata(MetadataName.RetryAfter, out var delay) ? delay : null;

    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Warning,
        Message = "Rate limit exceeded for {Hub}.{HubMethod} by user {UserId} on connection {ConnectionId}. Retry after {RetryAfter}.")]
    private partial void LogInvocationRejected(
        string hub,
        string hubMethod,
        string? userId,
        string connectionId,
        TimeSpan? retryAfter);

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "Connection rate limit exceeded for {Hub} by user {UserId} on connection {ConnectionId}. Retry after {RetryAfter}.")]
    private partial void LogConnectionRejected(
        string hub,
        string? userId,
        string connectionId,
        TimeSpan? retryAfter);

    public void Dispose()
    {
        _invocationLimiter.Dispose();
        _connectionLimiter.Dispose();
    }
}
