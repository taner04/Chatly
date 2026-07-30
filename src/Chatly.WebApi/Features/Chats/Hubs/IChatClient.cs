using Chatly.Contracts.Dtos;

namespace Chatly.WebApi.Features.Chats.Hubs;

public interface IChatClient
{
    Task MessageReceived(MessageReceived messageReceived);
}