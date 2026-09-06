using Chatly.Contracts.Endpoints.Chats.Results;

namespace Chatly.WebApi.Features.Chats.Endpoints.GetChats;

public sealed record GetChatsQuery : IQuery<IReadOnlyList<GetChatsResponse>>;