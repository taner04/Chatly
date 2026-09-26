namespace Chatly.WebApi.Common.Infrastructure.Persistence;

internal static class ChatlyDbContextOptionsConfigurator
{
    internal static void Configure(
        DbContextOptionsBuilder optionsBuilder,
        string connectionString)
    {
        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.EnableRetryOnFailure(
                5,
                TimeSpan.FromSeconds(10),
                null);
        });
    }
}