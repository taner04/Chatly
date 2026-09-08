using Chatly.Contracts.Endpoints.Friendships.Results;

namespace Chatly.WebApi.Features.Friendships.Endpoints.GetFriendships;

internal sealed record GetFriendshipsQuery : IQuery<IReadOnlyList<GetFriendshipsResponse>>;
