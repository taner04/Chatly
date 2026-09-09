using System.Net;
using Chatly.WebApi.Features.Chats.Models;

namespace Chatly.WebApi.Features.Chats.Exceptions;

internal sealed class ChatAccessDeniedException(ChatId chatId)
    : ChatlyException(
        "Chat access denied",
        $"You do not have access to chat '{chatId.Value}'.",
        "Chat.AccessDenied",
        HttpStatusCode.Forbidden);
