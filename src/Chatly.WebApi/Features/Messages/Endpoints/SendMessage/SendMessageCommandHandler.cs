using Chatly.Contracts;
using Chatly.Contracts.Dtos;
using Chatly.WebApi.Common.Infrastructure;
using Chatly.WebApi.Common.Infrastructure.Persistence;
using Chatly.WebApi.Common.Shared.Exceptions;
using Chatly.WebApi.Features.Chats.Hubs;
using Chatly.WebApi.Features.Chats.Models;
using Chatly.WebApi.Features.Messages.Models;
using Mediator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chatly.WebApi.Features.Messages.Endpoints.SendMessage;

internal sealed class SendMessageCommandHandler(
    ChatlyDbContext context,
    UserContext userContext,
    IHubContext<ChatHub, IChatClient> hubContext): ICommandHandler<SendMessageCommand>
{
    public async ValueTask<Unit> Handle(SendMessageCommand command, CancellationToken cancellationToken)
    {
        var isMember = await context.ChatMembers.AnyAsync(
            member =>
                member.ChatId == command.ChatId &&
                member.UserId == userContext.UserId,
            cancellationToken);

        if (!isMember)
        {
            throw new EntityNotFoundException<Chat>(command.ChatId.Value);
        }

        var message = new Message(
            command.ChatId,
            userContext.UserId,
            command.Content);

        context.Messages.Add(message);

        await context.SaveChangesAsync(cancellationToken);
        
        await hubContext.Clients.Group(command.ChatId.Value.ToString())
                                .MessageReceived(new MessageReceived(userContext.UserId.Value, command.ChatId.Value, message.Content));
        
        return Unit.Value;
    }
}