using Chatly.WebApi.Features.Chats.Models;

namespace Chatly.WebApi.Features.Chats.Endpoints.MarkChatRead;

public sealed record MarkChatReadCommand(ChatId ChatId) : ICommand;
