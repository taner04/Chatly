using Chatly.Contracts.Features.Friendships.Models;

namespace Chatly.WebApi.Features.Friendships.Endpoints.GetFriendships;

internal sealed record GetFriendshipsQuery : IQuery<IReadOnlyList<FriendshipContract>>;