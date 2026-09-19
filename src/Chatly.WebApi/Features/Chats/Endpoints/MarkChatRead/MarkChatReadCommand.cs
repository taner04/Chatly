namespace Chatly.WebApi.Features.Chats.Endpoints.MarkChatRead;

internal sealed record MarkChatReadCommand(ChatId ChatId) : ICommand;