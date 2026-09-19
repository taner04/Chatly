using Chatly.Contracts.Features.Messages.Models;

namespace Chatly.Contracts.Features.Messages.Endpoints.SendMessage;

public sealed class IncomingMessageNotification(MessageContract message)
    : Notification
{
    public MessageContract Message { get; } = message;
}