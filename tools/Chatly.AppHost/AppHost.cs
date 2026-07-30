using Chatly.ServiceDefaults;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddPostgres("database").WithPgAdmin();

var chatlyDb = db.AddDatabase(AppHostConstants.Database);

var migration = builder.AddProject<Chatly_MigrationService>(AppHostConstants.MigrationService)
    .WithReference(chatlyDb)
    .WaitFor(chatlyDb);

builder.AddProject<Chatly_WebApi>(AppHostConstants.Api)
    .WithReference(chatlyDb)
    .WaitFor(chatlyDb)
    .WaitForCompletion(migration);

builder.Build().Run();