using Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;
using Chatly.WebApi.Features.Calls.Models;
using Chatly.WebApi.Features.DeviceSessions.Models;
using Chatly.WebApi.Features.FriendRequests.Models;
using Chatly.WebApi.Features.Friendships.Models;
using Chatly.WebApi.Features.MessageAttachments.Models;
using Chatly.WebApi.Features.Reactions.Models;
using Chatly.WebApi.Features.StoredFiles.Models;

namespace Chatly.WebApi.Common.Infrastructure.Persistence;

public sealed class ChatlyDbContext(DbContextOptions<ChatlyDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Call> Calls => Set<Call>();
    public DbSet<ActiveCallParticipant> ActiveCallParticipants => Set<ActiveCallParticipant>();
    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<ChatReadState> ChatReadStates => Set<ChatReadState>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Reaction> Reactions => Set<Reaction>();
    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();
    public DbSet<MessageAttachment> MessageAttachments => Set<MessageAttachment>();
    public DbSet<DeviceSession> DeviceSessions => Set<DeviceSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChatlyDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.RegisterAllInEfCoreVogenIdConverter();
    }
}