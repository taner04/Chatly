using Chatly.MigrationService;
using Chatly.ServiceDefaults;
using Chatly.WebApi.Common.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHostedService<Worker>();

builder.Services.AddOpenTelemetry().WithTracing(t => { t.AddSource(Worker.ActivitySourceName); });

builder.AddNpgsqlDbContext<ChatlyDbContext>(AppHostConstants.Database);

var host = builder.Build();

host.Run();