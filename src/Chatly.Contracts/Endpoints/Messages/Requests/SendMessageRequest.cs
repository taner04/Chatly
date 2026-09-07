namespace Chatly.Contracts.Endpoints.Messages.Requests;

public sealed record SendMessageRequest(Guid ChatId, string Content);