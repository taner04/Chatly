using Chatly.Contracts.Features.Messages.Endpoints.GetMessages;
using Chatly.Contracts.Features.Messages.Models;
using Refit;

namespace Chatly.Desktop.Services.Api.Refit.Endpoints;

public interface IMessageEndpoint
{
    [Get(ApiRoutes.Chats.Messages)]
    Task<ApiResponse<GetMessagesResponse>> GetMessagesAsync(
        Guid chatId,
        DateTimeOffset? beforeSentAt,
        Guid? beforeMessageId,
        int pageSize,
        CancellationToken cancellationToken);

    [Multipart]
    [Post(ApiRoutes.Messages.Collection)]
    Task<ApiResponse<MessageContract>> SendMessageAsync(
        [AliasAs("chatId")] Guid chatId,
        [AliasAs("content")] string? content,
        [AliasAs("files")] IEnumerable<StreamPart> files,
        CancellationToken cancellationToken);

    [Delete(ApiRoutes.Messages.ById)]
    Task<IApiResponse> RemoveMessageAsync(
        Guid messageId,
        CancellationToken cancellationToken);
}