namespace Chatly.Contracts.Features.Messages.Endpoints.SendMessage;

public sealed record SendMessageRequest(Guid ChatId, string? Content);