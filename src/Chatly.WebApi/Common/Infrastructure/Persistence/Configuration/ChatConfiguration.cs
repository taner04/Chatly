namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal sealed class ChatConfiguration : UserPairEntityConfiguration<Chat, ChatId>
{
    protected override string TableName => "Chats";
    protected override string DistinctUsersConstraintName => "CK_Chats_DistinctUsers";
}