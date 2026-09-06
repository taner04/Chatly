using Chatly.MigrationService;
using Chatly.ServiceDefaults;
using Chatly.WebApi.Common.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHostedService<DatabaseMigrationWorker>();

builder.Services.AddOpenTelemetry().WithTracing(t => { t.AddSource(DatabaseMigrationWorker.ActivitySourceName); });

builder.AddNpgsqlDbContext<ChatlyDbContext>(AppHostConstants.DatabaseConnectionName);

var host = builder.Build();

host.Run();