using Chatly.WebApi.Features.FriendRequests.Models;
using Chatly.WebApi.Features.Outbox.Models;
using Chatly.WebApi.Features.Users.Models;
using Vogen;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

[EfCoreConverter<UserId>]
[EfCoreConverter<FriendRequestId>]
internal sealed partial class EfcVogenIdConverter;
