namespace Chatly.Desktop.Services.Api.Refit.Abstraction;

public interface IChatlyApi : IChatEndpoint, IFriendRequestEndpoint, IFriendshipEndpoint, IMessageEndpoint,
    IUserEndpoint
{
}