using System.Diagnostics;

namespace Chatly.WebApi.Common.Behaviours;

internal sealed partial class LoggingBehaviour<TMessage, TResponse>(
    IHttpContextAccessor httpContextAccessor,
    ILogger<LoggingBehaviour<TMessage, TResponse>> logger) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private readonly Type _behaviorType = typeof(TMessage);

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var properties = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["@RequestData"] = message
        };

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is not null)
        {
            properties["User"] = httpContext.User?.Identity?.Name;
            properties["RemoteIP"] = httpContext.Connection.RemoteIpAddress;

            var httpRequest = httpContext.Request;
            properties["ConnectingIP"] = httpRequest.Headers["CF-Connecting-IP"];
            properties["RequestMethod"] = httpRequest.Method;
            properties["RequestPath"] = httpRequest.Path.ToString();
        }

        try
        {
            var sw = Stopwatch.StartNew();
            var response = await next(message, cancellationToken);

            using var scope = logger.BeginScope(properties);
            LogSuccess(_behaviorType, sw.Elapsed.TotalMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            using var scope = logger.BeginScope(properties);
            LogException(_behaviorType, ex);

            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Executed {Type} handler in {Elapsed} ms")]
    private partial void LogSuccess(Type type, double elapsed);

    [LoggerMessage(Level = LogLevel.Error, Message = "Exception during {Type} handler")]
    private partial void LogException(Type type, Exception exception);
}
