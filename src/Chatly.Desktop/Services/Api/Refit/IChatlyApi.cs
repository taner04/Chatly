using Chatly.Desktop.Services.Api.Refit.Endpoints;

namespace Chatly.Desktop.Services.Api.Refit;

public interface IChatlyApi : IAttachmentEndpoint, IChatEndpoint, IFriendRequestEndpoint, IFriendshipEndpoint,
    IHealthEndpoint,
    IMessageEndpoint, IReactionEndpoint, IUserEndpoint
{
}