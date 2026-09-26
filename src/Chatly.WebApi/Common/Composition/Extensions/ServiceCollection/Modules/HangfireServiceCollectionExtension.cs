using Chatly.ServiceDefaults;
using Chatly.WebApi.Common.Infrastructure.BackgroundServices;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.States;

namespace Chatly.WebApi.Common.Composition.Extensions.ServiceCollection.Modules;

internal static class HangfireServiceCollectionExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddChatlyHangfire(WebApplicationBuilder builder)
        {
            services.AddHangfire((_, configuration) =>
            {
                configuration.UseSimpleAssemblyNameTypeSerializer();
                configuration.UseRecommendedSerializerSettings();
                configuration.UsePostgreSqlStorage(c =>
                {
                    c.UseNpgsqlConnection(
                        builder.Configuration.GetConnectionString(AppHostConstants.DatabaseConnectionName));
                });
            });

            services.AddHostedService<HangfireFireBackgroundService>();

            if (builder.Configuration.GetValue("Hangfire:ServerEnabled", true))
            {
                services.AddHangfireServer(options =>
                {
                    options.Queues = [EnqueuedState.DefaultQueue];
                    options.WorkerCount = Math.Max(1, Environment.ProcessorCount / 2);
                    options.SchedulePollingInterval = TimeSpan.FromSeconds(15);
                });
            }

            return services;
        }
    }
}