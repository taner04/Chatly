using Chatly.Contracts.Endpoints.Chats.Results;

namespace Chatly.WebApi.Features.Chats.Endpoints.GetChats;

internal sealed record GetChatsQuery : IQuery<IReadOnlyList<GetChatsResponse>>;
