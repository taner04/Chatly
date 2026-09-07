using Chatly.ServiceDefaults;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddPostgres("database").WithPgAdmin();

var chatlyDb = db.AddDatabase(AppHostConstants.DatabaseConnectionName);

var migration = builder.AddProject<Chatly_MigrationService>(AppHostConstants.MigrationServiceResourceName)
    .WithReference(chatlyDb)
    .WaitFor(chatlyDb);

var blobStorage = builder.AddAzureStorage(AppHostConstants.BlobStorageResourceName)
    .RunAsEmulator()
    .AddBlobs(AppHostConstants.BlobServiceConnectionName);


builder.AddProject<Chatly_WebApi>(AppHostConstants.WebApiResourceName)
    .WithReference(chatlyDb)
    .WaitFor(chatlyDb)
    .WithReference(blobStorage)
    .WaitFor(blobStorage)
    .WaitForCompletion(migration);

builder.Build().Run();