using Chatly.Contracts.Features.Messages.Models;

namespace Chatly.WebApi.Features.Messages.Endpoints.SendMessage;

internal sealed record SendMessageCommand(
    ChatId ChatId,
    string? Content,
    IReadOnlyCollection<IFormFile> Files) : ICommand<MessageContract>;