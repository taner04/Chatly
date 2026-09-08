using Chatly.WebApi.Features.Chats.Models;

namespace Chatly.WebApi.Features.Chats.Endpoints.MarkChatRead;

internal sealed record MarkChatReadCommand(ChatId ChatId) : ICommand;
