using Chatly.WebApi.Features.Chats.Models;
using Mediator;

namespace Chatly.WebApi.Features.Messages.Endpoints.SendMessage;

public sealed record SendMessageCommand(ChatId ChatId, string Content) : ICommand;