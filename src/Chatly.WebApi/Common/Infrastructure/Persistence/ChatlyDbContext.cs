using Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;
using Chatly.WebApi.Features.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace Chatly.WebApi.Common.Infrastructure.Persistence;

public sealed class ChatlyDbContext(DbContextOptions<ChatlyDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChatlyDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.RegisterAllInEfcVogenIdConverter();
    }
}