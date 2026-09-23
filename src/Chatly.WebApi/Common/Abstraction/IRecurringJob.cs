namespace Chatly.WebApi.Common.Abstraction;

internal interface IRecurringJob
{
    string Id { get; }
    string Queue { get; }
    string CronExpression { get; }

    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
