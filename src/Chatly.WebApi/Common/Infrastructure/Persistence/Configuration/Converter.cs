using Chatly.WebApi.Features.Users.Models;
using Vogen;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

[EfCoreConverter<UserId>]
internal sealed partial class EfcVogenIdConverter;