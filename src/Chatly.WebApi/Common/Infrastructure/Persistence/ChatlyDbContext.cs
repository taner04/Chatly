using Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;
using Chatly.WebApi.Features.Chats.Models;
using Chatly.WebApi.Features.FriendRequests.Models;
using Chatly.WebApi.Features.Friendships.Models;
using Chatly.WebApi.Features.Messages.Models;

namespace Chatly.WebApi.Common.Infrastructure.Persistence;

public sealed class ChatlyDbContext(DbContextOptions<ChatlyDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<ChatReadState> ChatReadStates => Set<ChatReadState>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChatlyDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.RegisterAllInEfCoreVogenIdConverter();
    }
}
