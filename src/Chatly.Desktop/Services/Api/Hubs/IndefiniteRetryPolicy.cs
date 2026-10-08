using Microsoft.AspNetCore.SignalR.Client;

namespace Chatly.Desktop.Services.Api.Hubs;

internal sealed class IndefiniteRetryPolicy : IRetryPolicy
{
    private static readonly TimeSpan[] InitialDelays =
    [
        TimeSpan.Zero,
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(10)
    ];

    private static readonly TimeSpan MaximumDelay = TimeSpan.FromSeconds(30);

    public TimeSpan? NextRetryDelay(RetryContext retryContext) =>
        retryContext.PreviousRetryCount < InitialDelays.Length
            ? InitialDelays[retryContext.PreviousRetryCount]
            : MaximumDelay;
}