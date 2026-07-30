using Chatly.WebApi.Features.Messages.Models;
using Chatly.WebApi.Features.Users.Models;
using Vogen;
using ChatId = Chatly.WebApi.Features.Chats.Models.ChatId;
using ChatMemberId = Chatly.WebApi.Features.Chats.Models.ChatMemberId;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

[EfCoreConverter<UserId>]
[EfCoreConverter<ChatId>]
[EfCoreConverter<ChatMemberId>]
[EfCoreConverter<MessageId>]
internal sealed partial class EfcVogenIdConverter;