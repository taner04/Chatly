namespace Chatly.WebApi.Common.Infrastructure.Persistence;

internal static class ChatlyDbContextOptionsConfigurator
{
    internal static void Configure(
        DbContextOptionsBuilder options,
        string connectionString)
    {
        options.UseNpgsql(connectionString);
    }
}