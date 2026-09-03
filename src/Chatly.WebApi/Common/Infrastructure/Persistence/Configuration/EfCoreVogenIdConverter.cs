using Chatly.WebApi.Features.Chats.Models;
using Chatly.WebApi.Features.FriendRequests.Models;
using Chatly.WebApi.Features.Friendships.Models;
using Chatly.WebApi.Features.Messages.Models;
using Vogen;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

[EfCoreConverter<UserId>]
[EfCoreConverter<FriendRequestId>]
[EfCoreConverter<FriendshipId>]
[EfCoreConverter<ChatId>]
[EfCoreConverter<MessageId>]
internal sealed partial class EfCoreVogenIdConverter;