using Chatly.Contracts.Users.Results;
using Mediator;

namespace Chatly.WebApi.Features.Users.Endpoints.UpdateUsername;

public sealed class UpdateUsernameCommandHandler : ICommandHandler<UpdateUsernameCommand, CurrentUserResponse>
{
    public ValueTask<CurrentUserResponse> Handle(UpdateUsernameCommand command, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}