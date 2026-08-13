using Chatly.WebApi.Features.FriendShips.Models;
using Chatly.WebApi.Features.Users.Models;
using Vogen;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

[EfCoreConverter<UserId>]
[EfCoreConverter<FriendShipId>]
internal sealed partial class EfcVogenIdConverter;