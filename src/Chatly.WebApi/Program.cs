using Chatly.WebApi.Common.Composition;
using Chatly.WebApi.Features.Chats.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalar();
}

app.UseHttpsRedirection();

app.MapHub<ChatHub>($"/{nameof(ChatHub).ToLowerInvariant()}");
app.MapEndpoints();

app.Run();