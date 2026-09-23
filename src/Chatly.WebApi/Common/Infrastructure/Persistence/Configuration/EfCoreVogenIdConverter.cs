using Chatly.WebApi.Features.Calls.Models;
using Chatly.WebApi.Features.FriendRequests.Models;
using Chatly.WebApi.Features.Friendships.Models;
using Chatly.WebApi.Features.MessageAttachments.Models;
using Chatly.WebApi.Features.Reactions.Models;
using Chatly.WebApi.Features.StoredFiles.Models;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

[EfCoreConverter<UserId>]
[EfCoreConverter<FriendRequestId>]
[EfCoreConverter<FriendshipId>]
[EfCoreConverter<ChatId>]
[EfCoreConverter<ChatReadStateId>]
[EfCoreConverter<MessageId>]
[EfCoreConverter<ReactionId>]
[EfCoreConverter<StoredFileId>]
[EfCoreConverter<MessageAttachmentId>]
[EfCoreConverter<CallId>]
[EfCoreConverter<ActiveCallParticipantId>]
internal sealed partial class EfCoreVogenIdConverter;
