using Chatly.Contracts.Endpoints.Messages.Results;
using Chatly.WebApi.Features.Chats.Models;

namespace Chatly.WebApi.Features.Messages.Endpoints.SendMessage;

internal sealed record SendMessageCommand(ChatId ChatId, string Content) : ICommand<SendMessageResponse>;
