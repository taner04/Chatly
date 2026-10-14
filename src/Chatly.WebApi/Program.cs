using Chatly.ServiceDefaults;
using Chatly.Shared.Extensions;
using Chatly.WebApi.Common.Composition.Configs;
using Chatly.WebApi.Common.Composition.Configs.OpenApi;
using Chatly.WebApi.Common.Composition.Extensions;
using Chatly.WebApi.Common.Composition.Extensions.ServiceCollection;
using Chatly.WebApi.Common.Composition.Options;
using Chatly.WebApi.Features.Hubs.CallingHub;
using Chatly.WebApi.Features.Hubs.NotificationHub;

var builder = WebApplication.CreateBuilder(args);

_ = builder.AddServiceDefaults();
_ = builder.Services.AddOpenApi(OpenApiConfig.Config);
_ = builder.Services.AddProblemDetails(ProblemDetailsConfig.Config);
_ = builder.Services.AddRateLimiter(options => RateLimiterConfig.Config(options, builder.Configuration.GetOption<RateLimitingOption>()));
_ = builder.Services.AddHttpContextAccessor();
_ = builder.Services.RegisterChatlyServices(builder);

var app = builder.Build();

_ = app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    _ = app.MapOpenApi();
    _ = app.MapScalar();
}

_ = app.UseExceptionHandler();

_ = app.UseHttpsRedirectionForClients();

_ = app.UseAuthentication();
_ = app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    _ = app.AddHangfireDashboard();
}

_ = app.UseRateLimiter();
_ = app.MapEndpoints();

// Hub invocations are limited by RateLimiterHubFilter. The HTTP limiter would also count every long-polling
// request and drop connections that fall back from WebSockets.
_ = app.MapHub<NotificationHub>(
        ApiRoutes.Hubs.Notification,
        options => options.CloseOnAuthenticationExpiration = true)
    .DisableRateLimiting();
_ = app.MapHub<CallingHub>(
        ApiRoutes.Hubs.Call,
        options => options.CloseOnAuthenticationExpiration = true)
    .DisableRateLimiting();

_ = await app.InitializeBlobStorage();

app.Run();