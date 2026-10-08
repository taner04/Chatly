using Chatly.AppHost.Composition.Extensions.DistributedApplicationBuilder;

var builder = DistributedApplication.CreateBuilder(args);

_ = builder.AddChatlyResources();

builder.Build().Run();
