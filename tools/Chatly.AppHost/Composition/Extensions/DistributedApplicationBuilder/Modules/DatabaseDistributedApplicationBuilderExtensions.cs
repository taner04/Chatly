using Chatly.ServiceDefaults;
using Projects;

namespace Chatly.AppHost.Composition.Extensions.DistributedApplicationBuilder.Modules;

internal static class DatabaseDistributedApplicationBuilderExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        internal IResourceBuilder<PostgresDatabaseResource> AddChatlyDatabase() =>
            builder.AddPostgres("database")
                .WithPgAdmin()
                .AddDatabase(AppHostConstants.DatabaseConnectionName);

        internal IResourceBuilder<ProjectResource> AddChatlyMigrations(
            IResourceBuilder<PostgresDatabaseResource> database) =>
            builder.AddProject<Chatly_MigrationService>(AppHostConstants.MigrationServiceResourceName)
                .WithReference(database)
                .WaitFor(database);
    }
}
