using Chatly.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Chatly.WebApi.Common.Infrastructure.Persistence;

public sealed class ChatlyDbContextFactory : IDesignTimeDbContextFactory<ChatlyDbContext>
{
    public ChatlyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ChatlyDbContext>();
        optionsBuilder.UseNpgsql(AppHostConstants.Database);

        return new ChatlyDbContext(optionsBuilder.Options);
    }
}