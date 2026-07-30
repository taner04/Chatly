namespace Chatly.Contracts.Dtos;

public readonly record struct MessageReceived(Guid UserId, Guid ChatId, string Content);