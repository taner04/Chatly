using System.Diagnostics;
using Hangfire.Server;

namespace Chatly.WebApi.Common.Infrastructure.Hangfire;

[SingletonService(typeof(IServerFilter))]
internal sealed partial class HangfireLoggingFilter(
    ILogger<HangfireLoggingFilter> logger)
    : IServerFilter
{
    private const string JobStartTimeKey = "JobStartTime";

    public void OnPerforming(PerformingContext context)
    {
        context.Items[JobStartTimeKey] = Stopwatch.GetTimestamp();

        LogJobStarted(
            logger,
            context.BackgroundJob.Id,
            context.BackgroundJob.Job.Type.Name,
            context.BackgroundJob.Job.Method.Name);
    }

    public void OnPerformed(PerformedContext context)
    {
        var elapsed = GetElapsedTime(context);

        if (context.Exception is not null)
        {
            LogJobFailed(
                logger,
                context.Exception,
                context.BackgroundJob.Id,
                context.BackgroundJob.Job.Type.Name,
                context.BackgroundJob.Job.Method.Name,
                elapsed.TotalMilliseconds);

            return;
        }

        LogJobCompleted(
            logger,
            context.BackgroundJob.Id,
            context.BackgroundJob.Job.Type.Name,
            context.BackgroundJob.Job.Method.Name,
            elapsed.TotalMilliseconds);
    }

    private static TimeSpan GetElapsedTime(PerformedContext context)
    {
        if (context.Items.TryGetValue(JobStartTimeKey, out var value) &&
            value is long startTimestamp)
        {
            return Stopwatch.GetElapsedTime(startTimestamp);
        }

        return TimeSpan.Zero;
    }

    [LoggerMessage(
        LogLevel.Information,
        "Hangfire job {JobId} started: {JobType}.{Method}")]
    private static partial void LogJobStarted(
        ILogger<HangfireLoggingFilter> logger,
        string jobId,
        string jobType,
        string method);

    [LoggerMessage(
        LogLevel.Information,
        "Hangfire job {JobId} completed in {ElapsedMilliseconds:F2} ms: {JobType}.{Method}")]
    private static partial void LogJobCompleted(
        ILogger<HangfireLoggingFilter> logger,
        string jobId,
        string jobType,
        string method,
        double elapsedMilliseconds);

    [LoggerMessage(
        LogLevel.Error,
        "Hangfire job {JobId} failed after {ElapsedMilliseconds:F2} ms: {JobType}.{Method}")]
    private static partial void LogJobFailed(
        ILogger<HangfireLoggingFilter> logger,
        Exception exception,
        string jobId,
        string jobType,
        string method,
        double elapsedMilliseconds);
}