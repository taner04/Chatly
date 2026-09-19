namespace Chatly.WebApi.Features.Messages.Endpoints.RemoveMessage;

public sealed record RemoveMessageCommand(MessageId MessageId) : ICommand;