using Microsoft.AspNetCore.SignalR;

namespace Chatly.WebApi.Features.Hubs.HubFilters;

internal sealed class LoggingHubFilter(ILogger<LoggingHubFilter> logger) : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext, 
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        logger.LogInformation("Calling {HubMethod}", invocationContext.HubMethodName);
        try
        {
            return await next(invocationContext);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception in {HubMethod}", invocationContext.HubMethodName);
            throw;
        }
    }
}