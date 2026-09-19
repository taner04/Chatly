using Chatly.Desktop.Services.Api.Refit.Abstraction;

namespace Chatly.Desktop.Services.Api.Refit;

public interface IChatlyApi : IAttachmentEndpoint, IChatEndpoint, IFriendRequestEndpoint, IFriendshipEndpoint,
    IHealthEndpoint,
    IMessageEndpoint, IReactionEndpoint, IUserEndpoint
{
}