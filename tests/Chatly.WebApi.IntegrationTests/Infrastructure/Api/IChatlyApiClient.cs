using Chatly.Contracts.Common.Pagination;
using Chatly.Contracts.Features.Chats.Endpoints.GetChats;
using Chatly.Contracts.Features.DeviceSessions.Models;
using Chatly.Contracts.Features.FriendRequests.Endpoints.SendFriendRequest;
using Chatly.Contracts.Features.FriendRequests.Models;
using Chatly.Contracts.Features.Friendships.Models;
using Chatly.Contracts.Features.Messages.Endpoints.GetMessages;
using Chatly.Contracts.Features.Messages.Models;
using Chatly.Contracts.Features.Reactions.Endpoints.SetReaction;
using Chatly.Contracts.Features.Reactions.Models;
using Chatly.Contracts.Features.Users.Endpoints.GetCurrentUser;
using Chatly.Contracts.Features.Users.Endpoints.GetCurrentUserProfilePicture;
using Chatly.Contracts.Features.Users.Endpoints.SearchUsers;
using Chatly.Contracts.Features.Users.Endpoints.UpdateUsername;
using Refit;

namespace Chatly.WebApi.IntegrationTests.Infrastructure.Api;

public interface IChatlyApiClient
{
    [Get(ApiRoutes.Health.Status)]
    Task<IApiResponse> CheckStatusAsync(CancellationToken cancellationToken);

    [Get(ApiRoutes.Users.Current)]
    Task<IApiResponse<CurrentUserResponse>> GetCurrentUserAsync(CancellationToken cancellationToken);

    [Put(ApiRoutes.Users.Username)]
    Task<IApiResponse<CurrentUserResponse>> UpdateUsernameAsync(
        [Body] UpdateUsernameRequest request,
        CancellationToken cancellationToken);

    [Multipart]
    [Put(ApiRoutes.Users.Onboarding)]
    Task<IApiResponse<CurrentUserResponse>> CompleteOnboardingAsync(
        [AliasAs("newUsername")] string newUsername,
        CancellationToken cancellationToken);

    [Multipart]
    [Put(ApiRoutes.Users.ProfilePicture)]
    Task<IApiResponse<CurrentUserResponse>> UpdateProfilePictureAsync(
        [AliasAs("file")] StreamPart file,
        CancellationToken cancellationToken);

    [Get(ApiRoutes.Users.ProfilePicture)]
    Task<IApiResponse<GetCurrentUserProfilePictureResponse>> GetCurrentUserProfilePictureAsync(
        CancellationToken cancellationToken);

    [Get(ApiRoutes.Users.Search)]
    Task<IApiResponse<PaginationResult<UserSearchResponse>>> SearchUsersAsync(
        [AliasAs("searchName")] string searchName,
        [AliasAs("pageIndex")] int pageIndex,
        [AliasAs("pageSize")] int pageSize,
        CancellationToken cancellationToken);

    [Get(ApiRoutes.FriendRequests.Collection)]
    Task<IApiResponse<PaginationResult<FriendRequestContract>>> GetFriendRequestsAsync(
        [AliasAs("pageIndex")] int pageIndex,
        [AliasAs("pageSize")] int pageSize,
        CancellationToken cancellationToken);

    [Post(ApiRoutes.FriendRequests.Collection)]
    Task<IApiResponse> SendFriendRequestAsync(
        [Body] SendFriendRequestRequest request,
        CancellationToken cancellationToken);

    [Post(ApiRoutes.FriendRequests.Accept)]
    Task<IApiResponse<FriendshipContract>> AcceptFriendRequestAsync(
        Guid friendRequestId,
        CancellationToken cancellationToken);

    [Post(ApiRoutes.FriendRequests.Reject)]
    Task<IApiResponse> RejectFriendRequestAsync(
        Guid friendRequestId,
        CancellationToken cancellationToken);

    [Get(ApiRoutes.Users.Sessions)]
    Task<IApiResponse<IReadOnlyList<DeviceSessionContract>>> GetDeviceSessionsAsync(
        CancellationToken cancellationToken);

    [Delete(ApiRoutes.Users.SessionById)]
    Task<IApiResponse> RevokeDeviceSessionAsync(Guid sessionId, CancellationToken cancellationToken);

    [Delete(ApiRoutes.Users.Sessions)]
    Task<IApiResponse> RevokeOtherDeviceSessionsAsync(CancellationToken cancellationToken);

    [Delete(ApiRoutes.Users.CurrentSession)]
    Task<IApiResponse> RevokeCurrentDeviceSessionAsync(CancellationToken cancellationToken);

    [Get(ApiRoutes.Friendships.Collection)]
    Task<IApiResponse<IReadOnlyList<FriendshipContract>>> GetFriendshipsAsync(CancellationToken cancellationToken);

    [Delete(ApiRoutes.Friendships.ByAssociatedUserId)]
    Task<IApiResponse> RemoveFriendshipAsync(Guid associatedUserId, CancellationToken cancellationToken);

    [Get(ApiRoutes.Chats.Collection)]
    Task<IApiResponse<IReadOnlyList<GetChatsResponse>>> GetChatsAsync(CancellationToken cancellationToken);

    [Put(ApiRoutes.Chats.Read)]
    Task<IApiResponse> MarkChatReadAsync(Guid chatId, CancellationToken cancellationToken);

    [Get(ApiRoutes.Chats.ChatMessages)]
    Task<IApiResponse<GetMessagesResponse>> GetMessagesAsync(
        Guid chatId,
        [AliasAs("pageSize")] int pageSize,
        CancellationToken cancellationToken);

    [Multipart]
    [Post(ApiRoutes.Messages.Collection)]
    Task<IApiResponse<MessageContract>> SendMessageAsync(
        [AliasAs("chatId")] Guid chatId,
        [AliasAs("content")] string content,
        CancellationToken cancellationToken);

    [Multipart]
    [Post(ApiRoutes.Messages.Collection)]
    Task<IApiResponse<MessageContract>> SendMessageWithFilesAsync(
        [AliasAs("chatId")] Guid chatId,
        [AliasAs("content")] string content,
        [AliasAs("files")] IEnumerable<StreamPart> files,
        CancellationToken cancellationToken);

    [Get(ApiRoutes.Chats.ChatMessages)]
    Task<IApiResponse<GetMessagesResponse>> GetMessagesBeforeAsync(
        Guid chatId,
        [AliasAs("beforeSentAt")] [Query(Format = "O")]
        DateTimeOffset beforeSentAt,
        [AliasAs("beforeMessageId")] Guid beforeMessageId,
        [AliasAs("pageSize")] int pageSize,
        CancellationToken cancellationToken);

    [Delete(ApiRoutes.Messages.ById)]
    Task<IApiResponse> RemoveMessageAsync(Guid messageId, CancellationToken cancellationToken);

    [Put(ApiRoutes.Messages.Reaction)]
    Task<IApiResponse<MessageReactionContract>> SetReactionAsync(
        Guid messageId,
        [Body] SetReactionRequest request,
        CancellationToken cancellationToken);

    [Delete(ApiRoutes.Reactions.ById)]
    Task<IApiResponse> RemoveReactionAsync(Guid reactionId, CancellationToken cancellationToken);
}