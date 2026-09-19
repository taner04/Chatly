using Chatly.Contracts.Features.Chats.Endpoints.GetChats;

namespace Chatly.WebApi.Features.Chats.Endpoints.GetChats;

internal sealed record GetChatsQuery : IQuery<IReadOnlyList<GetChatsResponse>>;