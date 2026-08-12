using Chatly.ServiceDefaults;
using Chatly.WebApi.Common.Composition.Configs;
using Chatly.WebApi.Common.Composition.Configs.OpenApi;
using Chatly.WebApi.Common.Composition.Extensions;
using Chatly.WebApi.Common.Composition.Extensions.ServiceCollection;

var builder = WebApplication.CreateBuilder(args);

_ = builder.AddServiceDefaults();
_ = builder.Services.AddOpenApi(OpenApiConfig.Config);
_ = builder.Services.AddProblemDetails(ProblemDetailsConfig.Config);
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
_ = app.UseHttpsRedirection();
_ = app.UseAuthentication();
_ = app.UseAuthorization();
_ = app.MapEndpoints();

app.Run();