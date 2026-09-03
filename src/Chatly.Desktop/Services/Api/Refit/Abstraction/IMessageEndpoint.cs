using Chatly.Contracts.Endpoints.Messages.Requests;
using Chatly.Contracts.Endpoints.Messages.Results;
using Refit;

namespace Chatly.Desktop.Services.Api.Refit.Abstraction;

public interface IMessageEndpoint
{
    [Get("/api/chats/{chatId}/messages")]
    Task<ApiResponse<GetMessagesResponse>> GetMessagesAsync(
        Guid chatId,
        DateTimeOffset? beforeSentAt,
        Guid? beforeMessageId,
        int pageSize,
        CancellationToken cancellationToken);

    [Post("/api/messages")]
    Task<ApiResponse<SendMessageResponse>> SendMessageAsync(
        [Body] SendMessageRequest request,
        CancellationToken cancellationToken);
}