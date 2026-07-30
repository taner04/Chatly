using Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;
using Chatly.WebApi.Features.Chats.Models;
using Chatly.WebApi.Features.Messages.Models;
using Chatly.WebApi.Features.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace Chatly.WebApi.Common.Infrastructure.Persistence;

public sealed class ChatlyDbContext(DbContextOptions<ChatlyDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<DirectChat> DirectChats => Set<DirectChat>();
    public DbSet<GroupChat> GroupChats => Set<GroupChat>();
    public DbSet<ChatMember> ChatMembers => Set<ChatMember>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChatlyDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.RegisterAllInEfcVogenIdConverter();
    }
}